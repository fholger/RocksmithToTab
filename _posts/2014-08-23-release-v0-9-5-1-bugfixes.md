---
layout: post
title: New release v0.9.5.1 with lots of bugfixes
---

I released RocksmithToTab v0.9.5.1, which is entirely a bugfix result. I encourage you to upgrade to this version, as it will produce much nicer output.

### Changelog

* Fixed bass tunings, so that bass playback should now sound in the right octave.
* Fixed several errors in relation with bends that would produce errors in GP5.
* Updated Toolkit DLL to fix a bug with single note fret hand mutes not showing up.
* Fixed an issue where some silent bars at the end of a track could end up having negative duration, which would screw up the rhythm notation or even lead to an infinite loop.
* Fixed an issue with incorrectly placed ties accidentally hiding notes that should be played.
* In GP5, tracks could accidentally be assigned to a midi channel reserved for drum tracks. Fixed.
* Write text in gp5 files with CP-1252 encoding to fix a problem where files with certain non-ASCII characters could not be loaded in GP5.
* Altered the way slides are implemented in the exporter. This fixes several errors in rhythm duration and should also make the slide display more accurate to what was intended in Rocksmith.
* Fixed some nasty bugs in the rhythm detection that could potentially lead to infinite loops; this might also improve rhythm detection in a few edge cases.
* Fixed a regression in the rhythm detection code that resulted in poor results when dealing with triplet rhythms.



### Download

You can [download this release][dl] from GitHub. 


[dl]: https://github.com/fholger/RocksmithToTab/releases/tag/v0.9.5.1
