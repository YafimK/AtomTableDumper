using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Xml.Serialization;

namespace AtomTableDumper
{
    [Serializable]
    public class AtomTable
    {
        public int DeletedDelphiProcessesCount { get; set; }
        public int DelphiProcessCount { get; set; }
        public AtomTable()
        {
            RegisteredWindowMessages = new List<AtomTableEntry>();
            GlobalAtoms = new List<AtomTableEntry>();
            DelphiProcessCount = 0;
            DeletedDelphiProcessesCount = 0;
        }

        public int RegisteredWindowMessageCount { get; set; }

        public int GlobalAtomCount { get; set; }

        [XmlArrayItem(ElementName = "RegisteredWindowMessage")]
        public List<AtomTableEntry> RegisteredWindowMessages { get; set; }

        [XmlArrayItem(ElementName = "GlobalAtom")]
        public List<AtomTableEntry> GlobalAtoms { get; set; }

        private void GetAtomTableEntries(IList<AtomTableEntry> atomTableEntries, Func<int, StringBuilder, int, int> getAtomTableEntry)
        {
            atomTableEntries.Clear();
            var buffer = new StringBuilder(1024);

            for (int index = 0xC000; index <= 0xFFFF; index++)
            {
                int bufferLength = getAtomTableEntry(index, buffer, buffer.Capacity);
                if (bufferLength > 0)
                {
                    var atomTableEntry = new AtomTableEntry
                    {
                        Index = index,
                        Name = buffer.ToString(0, bufferLength),
                    };
                    atomTableEntry.DelphiRelated = IsDelphiRelated(atomTableEntry.Name);
                    if(atomTableEntry.DelphiRelated) { DelphiProcessCount++;}

                    if (atomTableEntry.DelphiRelated && CheckForDelphiProcessId(atomTableEntry) &&  IsAtomUnActive(atomTableEntry))
                    {
                        NativeMethods.GlobalDeleteAtom((ushort) atomTableEntry.Index);
                        if (NativeMethods.GetLastError() == 0)
                        {
                            atomTableEntry.Deleted = false;
                        }
                        else
                        {
                            atomTableEntry.Deleted = true;
                            DeletedDelphiProcessesCount++;
                        }

                    }
                    atomTableEntries.Add(atomTableEntry);
                }
            }
        }

        private static bool CheckForDelphiProcessId(AtomTableEntry atomTableEntry)
        {
            if (atomTableEntry.Name.Length > 8)
            {
                var delphiProcessIdCut = atomTableEntry.Name.Substring(atomTableEntry.Name.Length - 8, 8);
                int delphiProcessId;
                if (!int.TryParse(delphiProcessIdCut, out delphiProcessId))
                {
                    atomTableEntry.DelphiRelated = false;
                    return false;
                }
                atomTableEntry.processId = delphiProcessId;
            }
            return false;
        }


        private static bool IsAtomUnActive(AtomTableEntry atomTableEntry)
        {

            Process[] processlist = Process.GetProcesses();
            processlist.FirstOrDefault(pr => pr.Id == atomTableEntry.processId);
            if (processlist.Length == 0) { return true; }
            return false;
        }

        private static bool IsDelphiRelated(string name)
        {
            if (name.StartsWith("Delphi") || name.StartsWith("WndProcPtr") || name.StartsWith("ControlOfs") || name.StartsWith("DlgInstancePtr") )
            {
                return true;
            }
            return false;
        }

        public void Load()
        {
            GetAtomTableEntries(RegisteredWindowMessages, (index, buffer, bufferCapacity) => NativeMethods.GetClipboardFormatName((uint)index, buffer, bufferCapacity));
            GetAtomTableEntries(GlobalAtoms, (index, buffer, bufferCapacity) => (int)NativeMethods.GlobalGetAtomName((ushort)index, buffer, bufferCapacity));
            //
            // These two fields are provided as a convenience in the output file, so the counts are easy to see when viewing the file in a text editor
            //
            RegisteredWindowMessageCount = RegisteredWindowMessages.Count;
            GlobalAtomCount = GlobalAtoms.Count;
        }
    }
}
