---
layout: post
title: New release v0.7 supports bends, sustains
---

A new release of RocksmithToTab is available. This version adds preliminary support for sustains and bends. Let me know if you find any issues with it!

### Changelog

* Added support for sustains. Notes are, by default, sustained until the next note or until there is a pause longer than a full measure. If the note has a sustain value attached, it will be sustained for at least as long as that value says.
* Added preliminary support for bends. Due to limitations in the gpx file format, more complex bends may not show up correctly.
* Improved handling of slides, so there should be fewer incomplete slides in the generated files.

### Download

You can [download this release][dl] from GitHub. 



[dl]: https://github.com/fholger/RocksmithToTab/releases/tag/v0.7
