using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsIdentifiableReviewer
{
    internal class IsIdentifiableReviewerGlobalOptions
    {
        /// <summary>
        /// Location of database connection strings file (for issuing UPDATE statements)
        /// </summary>
        public string TargetsFile { get; set; }

        /// <summary>
        /// File containing rules for ignoring validation errors
        /// </summary>
        public string IgnoreList { get; set; }

        /// <summary>
        /// File containing rules for when to issue UPDATE statements
        /// </summary>
        public string RedList { get; set; }

        /// <summary>
        /// Sets the user interface to use a specific color palette yaml file
        /// </summary>
        public string Theme { get; set; }

        
    }
}
