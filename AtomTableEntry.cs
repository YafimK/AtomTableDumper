using System;

namespace AtomTableDumper
{
    [Serializable]
    public class AtomTableEntry
    {
        public int processId { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }

        public Boolean DelphiRelated { get; set; }

        public Boolean Deleted { get; set; }
    }
}
