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
using System.Diagnostics;


public class DataWaterfall
{
    
    const int SIZEREAD = 40000;
    int bugCount = 0;


    
   
    
    public static List<FreqData> Normalise(List<FreqData> AmplAverage)
    {

        float Max = AmplAverage.Max(p => p.Ampl);
        float Min = AmplAverage.Min(m => m.Ampl);
        float max = 1;
        float min = 0;
        List<FreqData> AmplNormal = new List<FreqData>();
        for (int i = 0; i < 1000; i++)
        {
            AmplNormal.Add(new FreqData() { Ampl = (float)(min + ((AmplAverage[i].Ampl - Min) * (max - min)) / (Max - Min)), Freq = AmplAverage[i].Freq });
        }
        return AmplNormal;
    }

    public List<FreqData> ConvertToDouble(List<FreqData> Amplitudes)
    {
       
        float sum = 0;
        int Count = 0, den = 0;
        List<FreqData> AmplAverage = new List<FreqData>();
        if (Amplitudes.Count < 40000)
        {
            this.bugCount++;
        }
        decimal Range;
       // Amplitudes.RemoveAt(0);
        var First = Amplitudes.FirstOrDefault();
        var Last = Amplitudes.LastOrDefault();
        Range = Last.Freq - First.Freq;
        decimal Ratio = Range / 1000;

        for (decimal s = First.Freq; s < Last.Freq; s += Ratio)
        {


            
                for (int i = Count; i < Amplitudes.Count && Amplitudes[i].Freq <= (s + Ratio); Count++, i++)
                {
                    sum += Amplitudes[i].Ampl;

                    den++;

                }
            if (sum == 0)
            {
                sum = AmplAverage[AmplAverage.Count-1].Ampl;
                den = 1;
            }
                AmplAverage.Add(new FreqData { Ampl = sum / den, Freq = s });
                sum = 0;
                den = 0;
           
            }



        

        return AmplAverage;
    }

}

