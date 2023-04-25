using RandomTrainTrailers.Definition;
using System;
using System.IO;
using System.Xml.Serialization;

namespace RandomTrainTrailers
{
    /// <summary>
    /// Handles the default configuration for the mod.
    /// 
    /// When first loading the mod it is stored in the mod's directory as SAVENAME and subsequent loads are done via this file.
    /// This way the user may modify it but we can still update it via the Steam Workshop.
    /// 
    /// User made configurations should be done via the user definition, see TrailerManager for more info.
    /// </summary>
    class DefaultTrailerConfig
    {
        private const string SAVENAME = "RTT-Default-Definition.xml";
        private static TrailerDefinition cachedDefinition;

        public static TrailerDefinition DefaultDefinition
        {
            get
            {
                if(cachedDefinition == null)
                {
                    string path = Path.Combine(Util.ModDirectory, SAVENAME);
                    try
                    {
                        if(File.Exists(path))
                        {
                            TrailerDefinition definition;

                            using(StreamReader sr = new StreamReader(path))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(TrailerDefinition));
                                definition = (TrailerDefinition)serializer.Deserialize(sr);
                            }

                            Util.Log("Finished loading default definition from " + path);
                            cachedDefinition = definition;
                        }
                        else
                        {
                            Util.LogError("No default definition found at " + path);
                        }
                    }
                    catch(Exception e)
                    {
                        Util.LogError("Exception trying to load definition\r\n" + path + "\r\nException:\r\n" + e.Message + "\r\n" + e.StackTrace);
                    }
                }
               
                return cachedDefinition;
            }
        }
    }
}
