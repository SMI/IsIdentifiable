using System.Collections.Generic;
using IsIdentifiable.Options;

namespace IsIdentifiable.Reporting.Reports
{
    public class ToMemoryFailureReport : IFailureReport
    {
        public List<Failure> Failures { get; } = new List<Reporting.Failure>();


        public void AddDestinations(IsIdentifiableBaseOptions options)
        {
            
        }

        public void DoneRows(int numberDone)
        {
            
        }

        public void Add(Reporting.Failure failure)
        {
            Failures.Add(failure);
        }

        public void CloseReport()
        {
            
        }
    }
}