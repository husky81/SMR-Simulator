using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCK.SmrSimulation.Main
{
    class SMR
    {
        public smrConcreteStructure structure;
        public SMR()
        {
            structure = new smrConcreteStructure();
        }
    }
    internal class smrConcreteStructure
    {
        public double length;
        public double width;
        public double height;
        public smrConcreteStructure()
        {
            length = 10;
            width = 5;
            height = 0.3;
        }
    }
}
