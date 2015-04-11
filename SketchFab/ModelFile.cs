using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SketchFab
{
    class ModelFile
    {
        private static Regex ZIP_RGX = new Regex("zip", RegexOptions.IgnoreCase);
        private static string ZIP_CONTENT = "application/zip";
        private static string OTHER_CONTENT = "application/octet-stream";

        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public ModelFile(byte[] file) : this(file, null) { }
        public ModelFile(byte[] file, string filename) : this(file, filename, null) { }
        public ModelFile(byte[] file, string filename, string extention)
        {
            File = file;
            FileName = filename;
            ContentType = ZIP_RGX.IsMatch(extention) ? ZIP_CONTENT : OTHER_CONTENT;
        }
    }
}
