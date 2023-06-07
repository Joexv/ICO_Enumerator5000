# ICO_Enumerator5000
 Adds icons to URL links on the desktop automatically using https://icon.horse/

# Arguments
-d {PathToURLs}
Optional Directory where you keep your URL shortcuts default is the Desktop. Use quotes for directors with spaces.
-i {No arguments}
When included, all icons will be redownloaded regardless of if they exist or not.
-y {No arguments}
When included the program will skip over user URL check. Helpful for automation.
-f {0,1,2}
0 - Program will do nothing when done.
1 - IconCache.db will be deleted and File Explorer restarted.
2 - Runs the command "ie4uinit.exe -show" (Windows 8.1 and below)