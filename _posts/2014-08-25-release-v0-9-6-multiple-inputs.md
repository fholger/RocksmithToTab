---
layout: post
title: New release v0.9.6 supports multiple input files
---

A new release of RocksmithToTab v0.9.6 is available. In this version, it is now possible to specify multiple input files to make it easier to batch convert Rocksmith 2014 DLC. 

### Changelog

* Can supply more than one input file to the program, and simple glob style (\*) file matching is supported. So you could do `RocksmithToTab.exe \path\to\Rocksmith2014\dlc\*.psarc` to convert all your dlc content. 
* With the parameter `--xml`, all input files are treated as xml arrangements. Primarily useful during CDLC creation.
* For custom DLCs, the program tries to extract the CDLC author and CDLC version. Both are written in the comments section of the song information in Guitar Pro.
* Fixes a bug in the toolkit that resulted in single note frethand mutes not showing up properly.



### Download

You can [download this release][dl] from GitHub. 


[dl]: https://github.com/fholger/RocksmithToTab/releases/tag/v0.9.6
