using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private const int DEFAULT_RANDOM_CHANCE = 94;

        private static TrailerDefinition cachedDefinition;

        /*static TrailerDefinition defaultDefinition = new TrailerDefinition()
        {
            #region Default definition
            Vehicles = new List<TrailerDefinition.Vehicle>
            {
                // Acc3ssViolation
                new TrailerDefinition.Vehicle() { AssetName = "NS 2200 Mixed Cargo", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() {AssetName = "869739118.Freightliner Class 66 Intermodal_Data", AllowDefaultTrailers = false, RandomTrailerChance = DEFAULT_RANDOM_CHANCE,
                    Trailers = new List<TrailerDefinition.Trailer>()
                    {
                        // Default
                        new TrailerDefinition.Trailer("20/20/20 Hamburg + 40/20 Maersk")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal0"),                                    // 3x20ft Hamburg Sud
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal1") { InvertProbability = 100 },        // 40+20ft Maersk
                            }
                        },
                        new TrailerDefinition.Trailer("20/0/20 Maersk + 40 MSC")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal2"),                                    // 2x20ft Maersk
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal3") { InvertProbability = 100 },        // 40ft MSC
                            }
                        },
                        new TrailerDefinition.Trailer("40/20 MSC Hamburg + 40/20 Maersk")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal4"),                                    // 40ft MSC + 20ft Hamburg
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal1") { InvertProbability = 100 },
                            }
                        },
                        new TrailerDefinition.Trailer("40/20 MSC Hamburg + 20/0/20 Maersk")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal4"),
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal2") { InvertProbability = 100 },
                            }
                        },
                        // Extra
                        new TrailerDefinition.Trailer("40/20 MSC Hamburg + 20/20/20 Hamburg")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal0"),
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal4") { InvertProbability = 100 },
                            }
                        },
                        new TrailerDefinition.Trailer("2x 40ft MSC")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal3"),
                                new TrailerDefinition.Trailer("869739118.TrailerClass66FreightlinerIntermodal3") { InvertProbability = 100 },
                            }
                        },
                    }
                },

                // ron_fu-ta
                new TrailerDefinition.Vehicle() { AssetName = "781198069.BLS Re 465 Cargo_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() { AssetName = "457431411.Taurus Cargo_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() { AssetName = "470121022.Taurus Chirpy Cargo Express_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() { AssetName = "747940048.TRAXX Cargo (BLS Re 485)_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() { AssetName = "496645897.TRAXX Cargo (DB BR185)_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() { AssetName = "611092038.TRAXX Cargo (MRCE)_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() {
                    AssetName = "495578952.Taurus Cargo (OeBB)_Data",
                    AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE,
                    LocalBlacklist = new List<TrailerDefinition.BlacklistItem>()
                    {
                        new TrailerDefinition.BlacklistItem("495578952.Trailer0"),
                        new TrailerDefinition.BlacklistItem("495578952.Trailer1"),
                        new TrailerDefinition.BlacklistItem("495578952.Trailer2"),
                        new TrailerDefinition.BlacklistItem("495578952.Trailer3"),
                        new TrailerDefinition.BlacklistItem("495578952.Trailer4"),
                    },
                    Trailers = new List<TrailerDefinition.Trailer>()
                    {
                        new TrailerDefinition.Trailer("Double Italia/Evergreen Intermodal")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("495578952.Trailer0"),
                                new TrailerDefinition.Trailer("495578952.Trailer1"),
                            },
                            Weight = 8,
                        },
                        new TrailerDefinition.Trailer("Double Evergreen Intermodal")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("495578952.Trailer2"),
                                new TrailerDefinition.Trailer("495578952.Trailer1"),
                            },
                            Weight = 8,
                        },
                        new TrailerDefinition.Trailer("Double Tex Intermodal")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("495578952.Trailer3"),
                                new TrailerDefinition.Trailer("495578952.Trailer4"),
                            },
                            Weight = 8,
                        },
                    }
                },
                new TrailerDefinition.Vehicle() {
                    AssetName = "500655948.TRAXX Cargo (SBB Re482)_Data",
                    AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE,
                    LocalBlacklist = new List<TrailerDefinition.BlacklistItem>()
                    {
                        new TrailerDefinition.BlacklistItem("500655948.Trailer1"),
                        new TrailerDefinition.BlacklistItem("500655948.Trailer2"),
                        new TrailerDefinition.BlacklistItem("500655948.Trailer3"),
                        new TrailerDefinition.BlacklistItem("500655948.Trailer4"),
                        new TrailerDefinition.BlacklistItem("500655948.Trailer5"),
                    },
                    Trailers = new List<TrailerDefinition.Trailer>()
                    {
                        new TrailerDefinition.Trailer("Double Maersk Intermodal")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("500655948.Trailer1"),
                                new TrailerDefinition.Trailer("500655948.Trailer2"),
                            },
                            Weight = 8,
                        },
                        new TrailerDefinition.Trailer("Double Maersk/Tex Intermodal")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("500655948.Trailer1"),
                                new TrailerDefinition.Trailer("500655948.Trailer3"),
                            },
                            Weight = 8,
                        },
                        new TrailerDefinition.Trailer("Double Tex/Evergreen Intermodal")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("500655948.Trailer4"),
                                new TrailerDefinition.Trailer("500655948.Trailer5"),
                            },
                            Weight = 8,
                        },
                    }
                },
                new TrailerDefinition.Vehicle() {
                    AssetName = "516681588.TRAXX Cargo (CAPTRAIN)_Data",
                    AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE,
                    LocalBlacklist = new List<TrailerDefinition.BlacklistItem>()
                    {
                        new TrailerDefinition.BlacklistItem("516681588.Trailer0"),
                        new TrailerDefinition.BlacklistItem("516681588.Trailer1"),
                    },
                    Trailers = new List<TrailerDefinition.Trailer>()
                    {
                        new TrailerDefinition.Trailer("Double K Line Intermodal")
                        {
                            SubTrailers = new List<TrailerDefinition.Trailer>()
                            {
                                new TrailerDefinition.Trailer("516681588.Trailer0"),
                                new TrailerDefinition.Trailer("516681588.Trailer1"),
                            },
                            Weight = 8,
                        }
                    }
                },
                // bsquiklehausen
                new TrailerDefinition.Vehicle() { AssetName = "684699512.AFT 2 Short Container_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() { AssetName = "684700650.AFT 2 Short Empty Container_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() { AssetName = "684701453.AFT 2 Short Mixed 2_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                new TrailerDefinition.Vehicle() { AssetName = "518591623.AFT 2 Long Mixed_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE, LocalBlacklist = new List<TrailerDefinition.BlacklistItem>() {
                    new TrailerDefinition.BlacklistItem("518591623.Trailer0"),
                }, StartOffset = 1 },
                new TrailerDefinition.Vehicle() { AssetName = "684698602.AFT 2 Long Mixed 2_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE, LocalBlacklist = new List<TrailerDefinition.BlacklistItem>() {
                    new TrailerDefinition.BlacklistItem("684698602.Trailer0"),
                }, StartOffset = 2 },
                new TrailerDefinition.Vehicle() { AssetName = "518858167.AFT 2 Long Container_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE, LocalBlacklist = new List<TrailerDefinition.BlacklistItem>() {
                    new TrailerDefinition.BlacklistItem("518858167.Trailer0"),
                }, StartOffset = 1 },
                new TrailerDefinition.Vehicle() { AssetName = "518853978.AFT 2 Short Mixed_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE, LocalBlacklist = new List<TrailerDefinition.BlacklistItem>() {
                    new TrailerDefinition.BlacklistItem("518853978.Trailer0"),
                }, StartOffset = 1 },
                new TrailerDefinition.Vehicle() { AssetName = "684697924.AFT 2 Long Hopper_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE, LocalBlacklist = new List<TrailerDefinition.BlacklistItem>() {
                    new TrailerDefinition.BlacklistItem("684697924.Trailer0"),
                }, StartOffset = 1 },
                // fatfluffycat / bsquiklehausen
                new TrailerDefinition.Vehicle() { AssetName = "811844671.FCC Train 3_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE, LocalBlacklist = new List<TrailerDefinition.BlacklistItem>() {
                    new TrailerDefinition.BlacklistItem("811844671.Trailer0"),
                }, StartOffset = 1 },
                new TrailerDefinition.Vehicle() { AssetName = "813150923.FCC Intermodal 2_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE, LocalBlacklist = new List<TrailerDefinition.BlacklistItem>() {
                    new TrailerDefinition.BlacklistItem("813150923.TrailerFCC Intermodal 20"),
                }, StartOffset = 1 },
                new TrailerDefinition.Vehicle() { AssetName = "814160957.FCC Intermodal 3_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE, LocalBlacklist = new List<TrailerDefinition.BlacklistItem>() {
                    new TrailerDefinition.BlacklistItem("814160957.TrailerFCC Intermodal 30"),
                }, StartOffset = 2 },
                // Von Roth
                new TrailerDefinition.Vehicle() { AssetName = "735034206.DSB MY Cargo train_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                // Thaok
                new TrailerDefinition.Vehicle() {AssetName = "583808337.218 Sea Containers_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                // skyu
                new TrailerDefinition.Vehicle() {AssetName = "704821933.korail 7600 container_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                // Tim the Terrible
                new TrailerDefinition.Vehicle() {AssetName = "498869526.TRAXX Freight Train_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
                // Matt Crux/Shroomblaze
                new TrailerDefinition.Vehicle() {AssetName = "497362717.Union Pacific Cargo Train_Data", AllowDefaultTrailers = true, RandomTrailerChance = DEFAULT_RANDOM_CHANCE },
            }
            #endregion
        };*/

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
                            //SaveDefaultConfig();
                        }
                    }
                    catch(Exception e)
                    {
                        Util.LogError("Exception trying to load definition\r\n" + path + "\r\nException:\r\n" + e.Message + "\r\n" + e.StackTrace);
                    }
                }
               
                return cachedDefinition;// defaultDefinition.Copy();
            }
        }

        /*private static bool SaveDefaultConfig()
        {
            string path = Path.Combine(Util.ModDirectory, SAVENAME);
            try
            {
                if(File.Exists(path))
                {
                    Util.LogError("File at " + path + " already exists, remove it before trying to save the default config!");
                    return false;
                }
                TrailerDefinition definition = defaultDefinition;

                using(StreamWriter sw = new StreamWriter(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TrailerDefinition));
                    serializer.Serialize(sw, definition);
                }

                Util.Log("Finished saving definition to " + path);
                return true;
            }
            catch(Exception e)
            {
                Util.LogError("Exception trying to load definition\r\n" + path + "\r\nException:\r\n" + e.Message + "\r\n" + e.StackTrace);
            }
            return false;
        }*/
    }
}
