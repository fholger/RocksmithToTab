---
layout: post
title: RocksmithToTab v1.0 released - with incremental tab generation
---

Finally! RocksmithToTab v1.0 is done and ready for download :)

This update fixes a number of bugs, including endless loops and invalid output files that couldn't be opened in Guitar Pro. Additionally, I improved on the GUI so that it now remembers settings and optionally only processes arrangements that were installed since the last time RocksmithToTab ran. That way, you don't have to wait for all of the base Rocksmith songs to convert.

If you happen to spot any new bugs, please report them over on [GitHub][issues].


### Changelog
* Support for 5-string bass arrangements added.
* Fixed an infinite loop bug in the tab conversion.
* Fixed several bugs in generated .gp5 and .gpx files.
* Files in subfolders of the dlc folder will now be picked up by the GUI program.
* Remember settings in the GUI for next use.
* Provide option to only convert files which were added since the last run (incremental updates).


### Download

As of this release, I'm abandoning SourceForge due to their ad infestation. The downloads can now be found on [FossHub][dl]. You'll find binary packages for Windows and Mac there as well as a source code archive. Alternatively, you can grab the source code over on [GitHub][source].


[dl]: http://code.fosshub.com/RocksmithToTab/downloads/
[source]: https://github.com/fholger/RocksmithToTab/releases/tag/v1.0
[issues]: https://github.com/fholger/RocksmithToTab/issues