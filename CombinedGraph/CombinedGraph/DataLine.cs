using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using CombinedGraph;


public class FreqData
{
    public float Ampl { get; set; }
    public decimal Freq { get; set; }
    
}
public class DataLine
{
   
    const int SIZEREAD = 40000;
    int bugCount = 0;
    int rowcount = 0;




    public List<FreqData> ConvertToDouble(List<FreqData> Values)
    {
        float sumAmpl = 0;
        int Count = 0;

        List<FreqData> AverageValues = new List<FreqData>();
        // fix list of freq data
        if (Values.Count < 40000)
        {
            this.bugCount++;
        }

       

            for (int j = 0; j < Values.Count - Values.Count % 40000; j += 40)
            {
                for (int i = j; i < j + 40; i++)
                {
                    sumAmpl += Values[i].Ampl;
                }

                AverageValues.Add(new FreqData() { Ampl = sumAmpl / 40, Freq = Values[j + 20].Freq });
                Count++;
                sumAmpl = 0;
            }
        
        return AverageValues;
    }
}

