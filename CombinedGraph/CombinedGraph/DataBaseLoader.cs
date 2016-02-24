using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombinedGraph
{
     public class DataBaseLoader
    {
        DateTime? CurrentDateTime = null;
        
        public  List<FreqData> DataLoader()
        {
           int  Exit = 0;
            using (var data = new airscanEntities())
            {
                if (this.CurrentDateTime == null)
                    {
                    var firstDate = data.measurings.FirstOrDefault().date_time;
                    this.CurrentDateTime = firstDate;
                    var lstValues = data.measurings.Where(s => s.date_time == firstDate).Select(s => new FreqData { Ampl = s.pdbm, Freq = s.frequency }).ToList();

                     return lstValues;
                    }
                else
                {

                    try
                    {
                        var nextdate = data.measurings.FirstOrDefault(s => s.date_time > CurrentDateTime).date_time;
                        this.CurrentDateTime = nextdate;
                        var lstValues = data.measurings.Where(s => s.date_time == nextdate).Select(s => new FreqData { Ampl = s.pdbm, Freq = s.frequency }).ToList();
                        return lstValues;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("End of Stream" + e.ToString());
                        Exit = 1;
                        return null;
                    }

                }

            }
        }
    }
}
