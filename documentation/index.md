---
layout: default
title: Documentation
---

# Documentation
RocksmithToTab is, at its core, a command line program. However, if you are not familiar with the command line, do not worry. I added a graphical user interface that should be straight-forward to use. Please follow the quickstart instructions below.

A detailed description of all the GUI options can be found here: [GUI overview](/documentation/gui).

To learn about how to use the command line version of the program, see here: [Command line options](/documentation/commandline).

You may also check out the [wiki](https://github.com/fholger/RocksmithToTab/wiki).

## Quickstart

### Prerequisites
RocksmithToTab is a .Net 4.0 program. If you are a Windows user, you should be good to go, as the .Net platform is shipped with newer Windows versions. If you are using a Mac, you must install [Mono](http://www.mono-project.com/download/).

### Installation
Download the latest version of RocksmithToTab. The program comes in the form of a compressed archive, a zip file for Windows or a tar.gz archive for Mac. Extract the archive anywhere you like. You should get a folder called RocksmithToTab.

### Converting the songs
In the RocksmithToTab folder you just extracted, run the RocksmithToTabGUI.exe (Mac users: run the file RocksmithToTabGUI w/o a file ending). A window should open, looking like this:
![The RocksmithToTab GUI](/images/gui_1_0.jpg)
The default options should be fine, the most significant settings are the first two, which specify the path to Rocksmith 2014 and where to save the converted tabs. The program should auto-detect your Rocksmith folder, but if it doesn't, you can generally find it in your Steam folder under steamapps\common\Rocksmith2014, e.g.

    c:\program files (x86)\steam\steamapps\common\Rocksmith2014

For the second options, if you are not happy with the default, specify any folder on your computer to save the generated tabs to. Please note that the program will overwrite existing files, so if you have a folder with non-Rocksmith tab files whose names might conflict with Rocksmith arrangements, you might not want to use that folder.

Once you are happy with the settings, simply hit the "Create tabs!" button at the bottom and watch the magic happen. RocksmithToTab will produce tabs for all songs that you have currently installed. If, at a later time, you have bought new DLC or installed additional CDLC, simply rerun the program again.

### Using the generated tabs
RocksmithToTab's output is in the Guitar Pro format, either .gp5 or .gpx. The best option to view and work with these files is obviously [Guitar Pro](http://www.guitar-pro.com). However, if you do not own Guitar Pro and do not want to purchase it, there are at least three free and open source programs which can open Guitar Pro files in principle:
  
  * [TuxGuitar](http://www.tuxguitar.com.ar)
  * [MuseScore](https://musescore.org)
  * [PowerTab](https://github.com/powertab/powertabeditor/wiki/Power-Tab-Editor-2.0,-Here-at-last!)