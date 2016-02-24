using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombinedGraph
{
    class Validation
    {
        private void Validation_Input(int Min, int Max)
        {
            if (Min < 0 || Max > 3100)
            {
                throw (new IndexOutOfRangeException());
            }
            
        }

       
        
       
    }
}
