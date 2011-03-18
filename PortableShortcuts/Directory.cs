using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Markup;
using System.ComponentModel;


namespace PortableShortcuts
{

    /// <summary>
    /// Represents the root node and entry point for XAML data binding
    /// </summary>
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
                bool isDriveSpecified = value.Contains(":");
                var subDirectories = new Dictionary<string, Directory>();
                foreach (var file in
                    System.IO.Directory.GetFiles(Path, "*.exe", SearchOption.AllDirectories)
                        .Union(System.IO.Directory.GetFiles(Path, "*.bat", SearchOption.AllDirectories))) {
                    var fi = new FileInfo(file);
                    var key = isDriveSpecified ? fi.DirectoryName : fi.DirectoryName.Substring(fi.DirectoryName.IndexOf(':') + 1);
                    if (!subDirectories.ContainsKey(key))
                        subDirectories.Add(key, new DirectoryEntry
                        {
                            Name = fi.DirectoryName,
                            IsDirectory = true,
                            Path = fi.DirectoryName
                        });
                    subDirectories[key].Entries.Add(new DirectoryEntry
                    {
                        Name = fi.Name,
                        IsDirectory = false,
                        Path = fi.FullName
                    });
                }
                var dir = BuildHeirarchy(value, subDirectories);
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
                }
                else if (directories.ContainsKey(subdir.FullName.Substring(subdir.FullName.IndexOf(':') + 1))) {
                    myEntry.Entries.Add(directories[subdir.FullName.Substring(subdir.FullName.IndexOf(':') + 1)]);
                }
            }
            return myEntry;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Entries;
        }

        public override string ToString() {
            return Path;
        }
        
    }

    /// <summary>
    /// Sub node POCO object.  Note that the Path property is reverted to default behavior.
    /// </summary>
    internal class DirectoryEntry : Directory, INotifyPropertyChanged
    {
        private bool _isIncluded;
        public bool IsIncluded {
            get { return _isIncluded; }
            set {
                if (_isIncluded != value) {
                    _isIncluded = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this,new PropertyChangedEventArgs("IsIncluded"));
                }
            }
        }

        public bool IsDirectory { get; set; }

        public override string Path { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
