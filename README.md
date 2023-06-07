# ICO_Enumerator5000
 Hate seeing that bland icon of your browser everytime you make a website shortcut on your desktop? Tired of manually having to find, download, set an icon up for each shortcut, and being abused by your parents?

Well sucks to be you I guess. Give this program a try and at least you won't have to the first couple of things.

ICO Enumerator 5000 is a simple command line utility that will pull a PNG from https://icon.horse/ and set it as each shortcuts icon for you.

That's basically it. Cool. Thanks.

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
