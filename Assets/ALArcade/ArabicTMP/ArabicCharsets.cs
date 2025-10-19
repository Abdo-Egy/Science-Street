using UnityEngine;

namespace ALArcade.ArabicTMP
{
    /// <summary>
    /// Static class providing character sets for Arabic font creation
    /// </summary>
    public static class ArabicCharsets
    {
        /// <summary>
        /// Returns the complete Arabic character set for font asset creation
        /// </summary>
        public static string GetCompleteArabicCharset()
        {
            return
                "0600-06FF\n" +  // Arabic Basic
                "0750-077F\n" +  // Arabic Supplement
                "08A0-08FF\n" +  // Arabic Extended-A
                "FB50-FDFF\n" +  // Arabic Presentation Forms-A
                "FE70-FEFF\n" +  // Arabic Presentation Forms-B
                "0020-007F\n";   // Basic Latin for mixed text
        }

        /// <summary>
        /// Returns the basic Arabic character set for font asset creation (less comprehensive)
        /// </summary>
        public static string GetBasicArabicCharset()
        {
            return
                "0600-06FF\n" +  // Arabic Basic
                "FE70-FEFF\n" +  // Arabic Presentation Forms-B
                "0020-007F\n";   // Basic Latin for mixed text
        }

        /// <summary>
        /// Returns the essential Persian characters (چ پ ژ گ) to add to an Arabic font
        /// </summary>
        public static string GetPersianCharset()
        {
            return "067E, 0686, 0698, 06AF\n"; // Persian letters پ چ ژ گ
        }
    }
}