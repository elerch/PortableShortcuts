using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Markup;


namespace PortableShortcuts
{


    internal class Directory : MarkupExtension
    {
        public ICollection<Directory> Entries { get; private set; }
        public string Name { get; set; }

        private string _path;
        public virtual string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                var subDirectories = new Dictionary<string, Directory>();
                foreach (var file in
                    System.IO.Directory.GetFiles(Path, "*.exe", SearchOption.AllDirectories)
                        .Union(System.IO.Directory.GetFiles(Path, "*.bat", SearchOption.AllDirectories))) {
                    var fi = new FileInfo(file);
                    if (!subDirectories.ContainsKey(fi.DirectoryName))
                        subDirectories.Add(fi.DirectoryName, new DirectoryEntry
                        {
                            Name = fi.DirectoryName,
                            IsDirectory = true,
                            Path = fi.DirectoryName
                        });
                    subDirectories[fi.DirectoryName].Entries.Add(new DirectoryEntry
                    {
                        Name = fi.Name,
                        IsDirectory = false,
                        Path = fi.FullName
                    });
                }
                var dir = Directory.BuildHeirarchy("\\d\\bin", subDirectories);
                Name = dir.Name;
                Entries = dir.Entries;
            }
        }

        public Directory()
        {
            Entries = new List<Directory>();
        }

        public static DirectoryEntry BuildHeirarchy(string startingDirectory, IDictionary<string, Directory> directories)
        {
            var directory = new DirectoryInfo(startingDirectory);
            var myEntry = new DirectoryEntry
            {
                Name = directory.Name,
                Path = directory.FullName
            };
            if (directories.ContainsKey(startingDirectory))
                foreach (var entry in directories[startingDirectory].Entries)
                    myEntry.Entries.Add(entry);

            foreach (var subdir in directory.GetDirectories()) {
                if (directories.ContainsKey(subdir.FullName)) {
                    myEntry.Entries.Add(directories[subdir.FullName]);
                    foreach (var entry in BuildHeirarchy(subdir.FullName, directories).Entries)
                        myEntry.Entries.Add(entry);
                }
            }
            return myEntry;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Path;
        }
    }

    internal class DirectoryEntry : Directory
    {
        public bool IsIncluded { get; set; }
        public bool IsDirectory { get; set; }

        private string _path;
        public override string Path
        {
            get { return _path; }
            set { _path = value; }
        }
    }

}
