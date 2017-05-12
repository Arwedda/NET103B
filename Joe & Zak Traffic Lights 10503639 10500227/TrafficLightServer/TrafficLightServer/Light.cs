using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Joseph Kellaway   10503639
//Zakaria Robinson  10500227

namespace TrafficLightServer
{
    class Light
    {
        public int ID { get; set; }
        public int LightTime { get; set; }
        public int CarsWaiting { get; set; }
        public string Colour { get; set; }

        public Light()
        {
            ID = 0;
            LightTime = 4;
            CarsWaiting = 0;
            Colour = "Red";
        }

        public Light(int locID)
        {
            ID = locID;
            LightTime = 4;
            CarsWaiting = 0;
            Colour = "Red";
        }
    }
}
