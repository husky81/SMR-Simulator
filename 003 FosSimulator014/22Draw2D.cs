using Draw3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draw2D
{
    class Draw2D
    {
        public Shapes2D shapes = new Shapes2D();

        public Draw2D()
        {
            // 싱크로 하면 수정사항 다 반영되나 궁금하네. 회사에서 주석 써봄. 200514 17:30
        }
    }
    class Shapes2D : List<Shape2D>
    {

    }
    class Shape2D
    {

    }
    class Lines2D : List<Line2D>
    {

    }
    class Line2D : Shape2D
    {

    }
}
