using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RocksmithToolkitLib;
using RocksmithToolkitLib.PSARC;


namespace RSTabConverterLib
{
    /// <summary>
    /// Reads in a Rocksmith PSARC archive and collects information on the 
    /// contained tracks. Can extract specific tracks for processing.
    /// </summary>
    public class PsarcBrowser
    {
        private PSARC archive;

        /// <summary>
        /// Create a new PsarcBrowser from a specified archive file.
        /// </summary>
        /// <param name="fileName">Path of the .psarc file to open.</param>
        public PsarcBrowser(string fileName)
        {
            archive = new PSARC();
            using (var stream = File.OpenRead(fileName))
            {
                archive.Read(stream);
            }
        }

        public IList<TrackInfo> GetTrackList()
        {
            // Each song has a corresponding .json file within the archive containing
            // information about it.
            var infoFiles = archive.Entries.Where(x => x.Name.StartsWith(@"manifests/songs")
                && x.Name.EndsWith(".json")).OrderBy(x => x.Name);

            var trackList = new List<TrackInfo>();

            foreach (var entry in infoFiles)
            {
                Console.WriteLine("Entry found: {0}", entry.Name);
                using (var reader = new StreamReader(entry.Data))
                {
                    JObject o = JObject.Parse(reader.ReadToEnd());
                    var attributes = o["Entries"].First.Last["Attributes"];
                    var title = attributes["SongName"].ToString();
                    var artist = attributes["ArtistName"].ToString();
                    var album = attributes["AlbumName"].ToString();
                    var year = attributes["SongYear"].ToString();

                    var info = new TrackInfo()
                    {
                        Title = attributes["SongName"].ToString(),
                        Artist = attributes["ArtistName"].ToString(),
                        Album = attributes["AlbumName"].ToString(),
                        Year = attributes["SongYear"].ToString()
                    };
                    trackList.Add(info);
                }                
            }

            return trackList;
        }
    }


    public class TrackInfo
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Year { get; set; }
        public string Key { get; set; }
    }
}
