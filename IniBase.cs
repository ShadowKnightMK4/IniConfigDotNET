using System.IO;
using static System.Net.WebRequestMethods;
using System.IO.Compression;
using System.Text;
using static System.Collections.Specialized.BitVector32;
using System.Collections.Generic;

namespace IniConfigDotNet
{


    

    /// <summary>
    /// Throw when a section is not found.  Really just a <see cref="KeyNotFoundException"/>
    /// </summary>
    public class SectionNotFoundException : KeyNotFoundException 
    {
        public SectionNotFoundException() { }

        public SectionNotFoundException(string MissingSection) : base(string.Format("A section named \"{0}\" was not found in the ini file", MissingSection))
        {
        }
    }


    /*
     * Parse discards comments when reading them.  
     */
    /// <summary>
    /// We handle the file in memory until needing to dispose and commmit it.
    /// </summary>
    public class IniBase: IDisposable
    {
        #region constants
        public const string CompressedMarker = "ZIPPED.INI";
        public enum FileFormat
        {
            /// <summary>
            /// Read and write Ini with nothing extra as defined here <see href="https://en.wikipedia.org/wiki/INI_file"/>
            /// </summary>
            Simple = 1,
            /// <summary>
            /// Read and write a 'fancy' dialect. 
            /// </summary>
            Fancy = 2,
        }
        #endregion
        #region Variables and settings

        protected FileStream OutsideFile;
        private bool disposedValue;

        /// <summary>
        /// Setting to true allows creation of section names, ects with the index operator if not already existing.
        /// </summary>
        public bool AllowFlexibleIndex = false;

        private bool Changed;

        public FileFormat FileFormatControl = FileFormat.Fancy;
        /// <summary>
        /// The ini file itself.  There is a special case of using ; as the first char in the section value to mark as comment
        /// </summary>
        readonly Dictionary<string, Dictionary<string, string>> IniSettings = new Dictionary<string, Dictionary<string, string>>();

        #endregion
        #region file handling

        
        public override string ToString()
        {
            using (MemoryStream tmp = new MemoryStream())
            {

                SaveInfo(tmp);
                tmp.Position = 0;
                byte[] Data = tmp.GetBuffer();
                return Encoding.UTF8.GetString(Data, 0, Data.Length);
            }
        }
        public void WriteToStream(Stream stream)
        {
            SaveInfo(stream);
        }
        protected void WriteText(Stream Out, string text, Encoding encoding)
        {
            byte[] Data = encoding.GetBytes(text);
            Out.Write(Data, 0, Data.Length);
        }
        protected void WriteTextLn(Stream Out, string text, Encoding encoding)
        {
            byte[] Data = encoding.GetBytes(text + "\n");
            Out.Write(Data, 0, Data.Length);
        }
        protected void SaveInfo(Stream Out)
        {
            
            switch (this.FileFormatControl)
            {
                case FileFormat.Simple:
                    {
                        
                        foreach (string SectionName in IniSettings.Keys)
                        {
                            WriteTextLn(Out, SectionName, Encoding.UTF8);
                            foreach (string EntryName in IniSettings[SectionName].Keys)
                            {
                                if (EntryName.StartsWith("COMMENT"))
                                {
                                    if (IniSettings[SectionName][EntryName].StartsWith(";") ||
                                        IniSettings[SectionName][EntryName].StartsWith("//"))
                                    {
                                        // ITS A COMMENT
                                        WriteTextLn(Out, IniSettings[SectionName][EntryName], Encoding.UTF8);
                                    }
                                }
                                else
                                {
                                    WriteTextLn(Out, string.Format("{0}={1}", EntryName, IniSettings[SectionName][EntryName]), Encoding.UTF8);
                                }
                            }
                        }

                    }
                    break;
                case FileFormat.Fancy:
                    {

                    }
                    break;
                default: throw new NotImplementedException(Enum.GetName(typeof(FileFormat), this.FileFormatControl));
            }
        }

        protected void LoadInfo(Stream In)
        {
        }


        #endregion

        #region Section Routines

        #region Creating Sections

        /// <summary>
        /// Create a new section and set it to an empty dictionary. Does nothing if section exists already
        /// </summary>
        /// <param name="Section">Section to create</param>
        public void CreateSection(string Section)
        {
            try
            {
                IniSettings.Add(Section, new Dictionary<string, string>());
            }
            catch (ArgumentException)
            {
                throw;
            }
        }

    
        #endregion

        #region Section Existance

        /// <summary>
        /// Does it have a section with the passed name
        /// </summary>
        /// <param name="name">name to check for</param>
        /// <returns>true if it does and false if it does not</returns>
        public bool ContainsSectionName(string name)
        {
            return IniSettings.ContainsKey(name);
        }
        #endregion

        #region Section Name Getting
            /// <summary>
            /// Get the section names in this instance as an array
            /// </summary>
            /// <returns>returns the section names in this instance as an array</returns>
        public string[] GetSectionNames()
        {
            return IniSettings.Keys.ToArray();
        }


        /// <summary>
        /// Get the section names in this instance as a list with the option to sort
        /// </summary>
        /// <param name="Sort">true if sort by default sort</param>
        /// <returns>return the section names in this instance as a list with it sorted if Sort is set</returns>
        public List<string> GetSectionNamesAsList(bool Sort)
        {
            var ret = IniSettings.Keys.ToList();
            if (Sort)
            {
                ret.Sort();
            }
            return ret;
        }
        #endregion

        #region Section Data Getting

        /// <summary>
        /// Get specified entry on the specified data.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Entry"></param>
        /// <returns></returns>
        public string GetSectionEntry(string Section, string Entry)
        {
            return IniSettings[Section][Entry];
        }


        /// <summary>
        /// Return the dictionary contained within the passed section
        /// </summary>
        /// <param name="Section"></param>
        /// <returns>returns one dictionary. key is value name, value is value itself</returns>
        public Dictionary<string, string> GetSectionData(string Section)
        {
            return IniSettings[Section];
        }
        #endregion

        #region Section Updating

        /// <summary>
        /// Replace the existing section with a new set of value and names
        /// </summary>
        /// <param name="section">Section to replace</param>
        /// <param name="data">data to change</param>
        public void ReplaceSectionData(string section, Dictionary<string, string> data)
        {
            IniSettings[section] = data;
        }





        /// <summary>
        /// Set specific section entry to new value. Creates the section and entry if nonexistant
        /// </summary>
        /// <param name="Section">Section to set</param>
        /// <param name="Entry">value to set</param>
        /// <param name="val">what to set value to</param>  
        public void SetSectionEntry(string Section, string Entry, string val)
        {
            SetSectionEntry(Section, Entry, val, true);
        }

        /// <summary>
        /// Set specific section entry to new value with the option of choosing to fail if not already existing
        /// </summary>
        /// <param name="Section">Section to set</param>
        /// <param name="Entry">value to set</param>
        /// <param name="val">what to set value to</param>
        /// <param name="CreateIfNeeded">if section or entry does not exist, add it. Otherwise </param>
        /// <exception cref="SectionNotFoundException">Can be thrown if the section does not exist and CreateIfNeeded is false </exception>

        public void SetSectionEntry(string Section, string Entry, string val, bool CreateIfNeeded)
        {
            if (CreateIfNeeded)
            {
                if (IniSettings.ContainsKey(Section) == false)
                {
                    IniSettings.Add(Section, new Dictionary<string, string>());
                }

                if (IniSettings[Section].ContainsKey(Entry) == false)
                {
                    IniSettings[Section][Entry] = val;
                }
            }
            else
            {
                if (IniSettings.ContainsKey(Section) == false)
                {
                    throw new SectionNotFoundException(Section);
                }


                IniSettings[Section][Entry] = val;


            }
        }

        #endregion
        #region The this accessor
        public Dictionary<string, string> this[string Section]
        {
            get
            {
                try
                {
                    return IniSettings[Section];
                }
                catch (KeyNotFoundException)
                {
                    if (!this.AllowFlexibleIndex)
                    {
                        throw new SectionNotFoundException(Section);
                    }
                    else
                    {
                        CreateSection(Section);
                        return IniSettings[Section];
                    }
                }
            }
        }
        #endregion

        #endregion


        #region Constructors
        public IniBase()
        {
           
        }
        #endregion












        public bool DeleteSectionName(string name)
        {
            return IniSettings.Remove(name);
        }
   
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (OutsideFile != null)
                    {
                        OutsideFile.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~IniBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}