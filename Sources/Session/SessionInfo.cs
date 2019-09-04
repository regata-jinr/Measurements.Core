using System.ComponentModel.DataAnnotations;

namespace Measurements.Core
{
    public class SessionInfo
    {
        [Key]
        public string  Name           { get; set; }
        public string  DetectorsNames { get; set; }
        public string  Type           { get; set; }
        public string  CountMode      { get; set; }
        public string  SpreadOption   { get; set; }
        public int     Duration       { get; set; }
        public decimal Height         { get; set; }
        public string  Assistant      { get; set; }
        public string  Note           { get; set; }

        public override string ToString()
        {
            return $"{Name}--{DetectorsNames}--{Type}--{CountMode}--{SpreadOption}--{Duration}--{Height}";
        }
    }
}
