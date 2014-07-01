using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CRYLAB
{
    class HelperFunctions
    {
        public static List<SuperCell> BatchImport(string[] filenames, SuperCell parent)
        {
            List<SuperCell> superList = new List<SuperCell>();
            foreach (string filename in filenames)
            {
                superList.Add(SuperCell.ReadMol2_simple(filename, parent));
            }
            return superList;
        }
    }
}
