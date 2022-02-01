using IsIdentifiable.Options;
using IsIdentifiableReviewer;

namespace IsIdentifiable
{
    internal class GlobalOptions
    {
        public IsIdentifiableBaseOptions IsIdentifiableOptions { get; set; }

        public IsIdentifiableReviewerOptions IsIdentifiableReviewerOptions { get; set; }
    }
}