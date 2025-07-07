using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace RandomTrainTrailers
{
    abstract class AbstractConfigLoader
    {
        /// <summary>
        /// The filename of this type of config file.
        /// </summary>
        public abstract string FileName
        {
            get;
        }

        /// <summary>
        /// Called when a loader should prepare for a new loading run.
        /// Reset state here.
        /// </summary>
        public abstract void Prepare();

        /// <summary>
        /// Called when a file was found in a mod or asset directory.
        /// </summary>
        /// <param name="path">Full path to the found file</param>
        /// <param name="name">Name of the asset/mod</param>
        /// <param name="isMod">Indicates if it's from a mod directory</param>
        public abstract void OnFileFound(string path, string name, bool isMod);
    }
}
