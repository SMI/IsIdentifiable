using IsIdentifiable.Options;

namespace IsIdentifiable;

public class GlobalOptions
{
    public IsIdentifiableBaseOptions? IsIdentifiableOptions { get; set; }

    public IsIdentifiableReviewerOptions? IsIdentifiableReviewerOptions { get; set; }
}