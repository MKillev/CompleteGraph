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

public class Sub
{
    

 

  

    public static void ColorFunction(double value, out int r, out int g, out int b)
    {

        double R = 0, G = 0, B = 0;

        if (value <= 0.25 && value >= 0)
        {
            R = 0;
            G = 2*value;
            B = 1 - (4/3)*value;
        }
        else if (value <= 0.50 && value >= 0.25)
        {
            R = (4/3)*value - (1/3);
            G = 2 * value;
            B = 1 - (4 / 3) * value;
        }
        else if (value <= 0.750 && value >= 0.50)
        {
            R = (4 / 3) * value - (1 / 3);
            G = 2- 2 * value;
            B = 1 - (4 / 3) * value;
        }
        else if (value <= 1.00 && value >= 0.75)
        {
            R = (4 / 3) * value - (1 / 3);
            G = 2 - 2 * value;
            B = 0;
        }

        r = (int) (R*255);
        g = (int) (G*255);
        b = (int) (B*255);

    }

   public static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
    {
        double H =360*h;
        while (H < 0) { H += 360; };
        while (H >= 360) { H -= 360; };
        double R, G, B;
        S = 1;
        V = 1;
        if (V <= 0)
        { R = G = B = 0; }
        else if (S <= 0)
        {
            R = G = B = V;
        }
        
        else
        {
            double hf = H / 60.0;
            int i = (int)Math.Floor(hf);
            double f = hf - i;
            double pv = V * (1 - S);
            double qv = V * (1 - S * f);
            double tv = V * (1 - S * (1 - f));
            switch (i)
            {

                // Red is the dominant color

                case 0:
                    R = V;
                    G = tv;
                    B = pv;
                    break;
                
                // Green is the dominant color
                case 1:
                    R = qv;
                    G = V;
                    B = pv;
                    break;
                
                case 2:
                    R = pv;
                    G = V;
                    B = tv;
                    break;

                // Blue is the dominant color

                case 3:
                    R = pv;
                    G = qv;
                    B = V;
                    break;
                case 4:
                    R = tv;
                    G = pv;
                    B = V;
                    break;

                // Red is the dominant color

                case 5:
                    R = V;
                    G = pv;
                    B = qv;
                    break;

                // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                case 6:
                    R = V;
                    G = tv;
                    B = pv;
                    break;
                case -1:
                    R = V;
                    G = pv;
                    B = qv;
                    break;

                // The color is not defined, we should throw an error.

                default:
                    //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                    R = G = B = V; // Just pretend its black/white
                    break;
            }
        }
        r = Clamp((int)(R * 255.0));
        g = Clamp((int)(G * 255.0));
        b = Clamp((int)(B * 255.0));
    }

    /// <summary>
    /// Clamp a value to 0-255
    /// </summary>
    static int Clamp(int i)
    {
        if (i < 0) return 0;
        if (i > 255) return 255;
        return i;
    }
}