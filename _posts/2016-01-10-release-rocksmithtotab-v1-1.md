---
layout: post
title: RocksmithToTab v1.1 released - Better handling of many files
---

As it turns out, I introduced a new bug in v1.0 that prevents you from using the program if you have too many DLCs and CDLCs installed. I did a bit of refactoring to fix this, it should be working now. As a consequence, the command line version also gained new features and can now batch-process a given directory.

### Changelog
* Fixed a problem with the handling of many DLCs/CDLCs.
* Added options to the command-line program to batch-convert all songs found in a given directory.


### Download

You can download the new release at [FossHub][dl]. You'll find binary packages for Windows and Mac there as well as a source code archive. Alternatively, you can grab the source code over on [GitHub][source].


[dl]: http://code.fosshub.com/RocksmithToTab/downloads/
[source]: https://github.com/fholger/RocksmithToTab/releases/tag/v1.1
