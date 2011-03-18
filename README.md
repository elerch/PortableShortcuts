PortableShortcuts
=================

Simple WPF utility to blow in shortcuts for all executable files in \d\bin and subdirectories.

Current limitations:

1. I think it's set to compile to .NET 4 (although there's no good reason for this)
2. \d\bin is hard-coded in the XAML.  There should be textbox to control this, but it currently fits my personal needs
3. There's no current detection of existing shortcuts.  This is meant to be blown in (hit the checkbox on "All" to watch it go, so I didn't bother pre-selecting the items (relatively easy to do, tho)
4. All shortcuts are thrown into a PortableShortcuts folder in the user's start menu, with no way to change it.
5. If you have "a.exe" in the main directory and "a.exe" in a sub directory, you'll get a link to the second a.exe.  There's no conflict detection/avoidance code.

