# AtomTableDumper and Delphi ATOM table leak cleaner

This C# command line tool, based on the ideas in [Atom Table Monitor](https://github.com/JordiCorbilla/atom-table-monitor) by Jordi Corbilla, saves its output as an XML file.

I've added cleaning method for unused Delphi processes in the ATOM table basing on AenBleidd delphi program - 
https://github.com/AenBleidd/AtomCleaner

The output file name can be passed as the first parameter on the command line. This file name will have environment variables replaced and the {0} placeholder will be replaced with the current date and time. The format specifiers supported by `string.Format()` can be used.
