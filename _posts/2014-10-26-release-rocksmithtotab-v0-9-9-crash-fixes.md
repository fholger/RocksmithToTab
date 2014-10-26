---
layout: post
title: RocksmithToTab v0.9.9 released - fixes crashes and endless loops
---

Unfortunately, I don't currently have as much time as I'd like to keep working on RocksmithToTab. There are still a number of features I would like to implement, but I have to put them on hold for now. In the meantime, I'm releasing a small update that should take care of a few major bugs that were reported. There were a couple of instances where RocksmithToTab would crash or be caught in an endless loop. These instances should hopefully be resolved now. Please note that there are a few songs out there (particularly CDLC) for which the gp5 output is broken. If you can, please try the gpx versions, instead. I will try to fix the output in a future update.


### Changelog
* Hopefully fixes a crash of the RocksmithToTab GUI if Steam is not found.
* Fixes an endless loop bug in some arrangements that prevented the program from generating tabs.
* Fixes an issue where the presence of additional vocal tracks could crash RocksmithToTab.
* Implements the {artist_sort} tag for filename templates.

### Download

You can [download this release][dl] at SourceForge. If you want to grab the source code, visit [GitHub][source].


[dl]: http://sourceforge.net/projects/rocksmithtotab/files/Releases/
[source]: https://github.com/fholger/RocksmithToTab/releases/tag/v0.9.9