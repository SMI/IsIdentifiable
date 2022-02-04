using CommandLine;

namespace IsIdentifiable.Options
{
    /// <summary>
    /// Options for any verb that operates on dicom datasets (either from mongo, from file etc).
    /// </summary>
    public abstract class IsIdentifiableDicomOptions: IsIdentifiableBaseOptions
    {
        /// <summary>
        /// Optional. Generate a tree storage report which represents failures according to their position in the DicomDataset.
        /// </summary>
        [Option(HelpText = "Optional. Generate a tree storage report which represents failures according to their position in the DicomDataset.")]
        public bool TreeReport { get; set; }
    }
}