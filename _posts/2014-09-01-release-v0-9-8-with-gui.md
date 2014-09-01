---
layout: post
title: RocksmithToTab v0.9.8 release - now with GUI!
---

A new version of RocksmithToTab is available, v0.9.8. It features a few minor improvements, and it comes with a graphical user interface. If you don't like dealing with the command line, this GUI will allow you to easily convert all of the songs that you have installed. Go check it out!

![The new RocksmithToTab GUI](/images/gui.jpg)

### Changelog
* Sort arrangements in order lead, rhythm, bass to match the game's order.
* Improve track names. Removed level indication and added number distinction where necessary.
* Default output path is now `rocksmith_tabs`, so as not to accidentally pollute the current working dir. 
* Allow customisation of the output filenames via a filename template that can be specified with the `-n` option.
* Added a simple GUI interface which converts all songs found in the Rocksmith 2014 install directory.

### Download

You can [download this release][dl] at SourceForge. If you want to grab the source code, visit [GitHub][source].


[dl]: http://sourceforge.net/projects/rocksmithtotab/files/Releases/
[source]: https://github.com/fholger/RocksmithToTab/releases/tag/v0.9.8