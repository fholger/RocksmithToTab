---
layout: default
title: About
---

# About RocksmithToTab

RocksmithToTab is a little utility program I wrote to generate tabs for all the songs I own in Rocksmith 2014. Rocksmith is Ubisoft's guitar playing game / teaching tool which has you playing along to songs with a real guitar. While playing a song, Rocksmith will show you the notes or chords to play on a sort of note highway moving towards you, which is inspired by Guitar Hero and RockBand. Your guitar is connected to your computer or console with a special cable which allows the game to listen to what you play and determine which notes you played. 

RocksmithToTab takes the song arrangements from the game and turns the note highway into regular guitar tabs and music score. It outputs the tabs in Guitar Pro format, so you can view them in [Guitar Pro][] or [TuxGuitar][]. 

### Why did I write it?

As intuitive and easy to grasp Rocksmith's note highway may be, it has some drawbacks. It becomes very difficult to see what's happening in quick passages, where a lot of notes are coming at you. Although Rocksmith possesses the so-called Riff Repeater to slow things down, it's still not easy to get the whole picture as you can't move or stop the note highway on your own. Also, I personally found it very difficult to pick a strumming patterns in Rocksmith. 

In these situations, I prefer to look at the tablature of a song to get an impression of what I'm supposed to play. Seeing the rhythm notated for strumming sections helps a lot in figuring out the corresponding strumming pattern. Given that there was previously no way to see tabs for Rocksmith arrangements, I finally decided to write my own tool.

### What can it do?
It takes Rocksmith song arrangements, generally stored in archive files with the file ending .psarc, and converts them to tabs in the Guitar Pro formats .gp5 or .gpx, as you prefer. All of Rocksmith's techniques are supported, including legato techniques, string bends, chord diagrams, fingering hints and slap/pop techniques for Bass arrangements. All arrangements of a song (lead, rhythm, bass) are by default exported into the same tablature file as separate tracks, which you can switch between in Guitar Pro. Optionally, you can export all arrangements to separate tabs, if you prefer. The program works for all on-disc songs, purchased DLC and imported Rocksmith 1 songs, as well as for custom DLCs.

### Legal notice
When you purchase Rocksmith or any DLC songs, you are granted the right to use the song arrangements for your personal enjoyment. So if you use RocksmithToTab to generate tablature from these arrangements, please do not share them with others, as that is not covered by the rights granted to do. I won't take responsibility if you do it, anyway!



[Guitar Pro]: http://www.guitar-pro.com
[TuxGuitar]: http://www.tuxguitar.com.ar