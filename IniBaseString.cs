using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IniConfigDotNet
{
    public class IniBaseValueString
    {
        public enum Format
        {
            /// <summary>
            /// Do not modify the string
            /// </summary>
            None,   //
            /// <summary>
            /// Do not modify the string
            /// </summary>
            Literal = None,
            /// <summary>
            /// NOT IMPLEMENTED. String contains a series of 0-1.  Write as bitwise data and have a size value at the front
            /// </summary>
            BinaryString,

        }
        /// <summary>
        /// DETERMINES HOW THE STRING IS ENCODED IN FILES
        /// </summary>
        public Format Encoding = Format.None;
        public IniBaseValueString() { Value = string.Empty; }
        public object Value;

        
        /// <summary>
        /// Encode 
        /// </summary>
        /// <param name="Output"></param>
        public void ToStream(Stream Output)
        {
            ToStream(new BinaryWriter(Output), this);
        }

        /*
         */

        public static void ToStream(BinaryWriter Output, IniBaseValueString Data)
        { 
            if (Output == null)
            {
                throw new ArgumentNullException(nameof(Output));
            }
            if (Data == null)
            {
                throw new ArgumentNullException(nameof(Data));    
            }

            if (Data.Encoding != Format.Literal)
            {
                throw new NotImplementedException(Enum.GetName(typeof(Format), Data.Encoding));
            }
            Output.Write((ushort)Data.Encoding);
            
        }
    }
}
