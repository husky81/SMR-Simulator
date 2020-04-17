﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _003_FosSimulator014
{
    class FEM
    {
        public readonly ModelFem model = new ModelFem();
        public readonly FemLoads loads = new FemLoads();
        internal bool solved = false;

        public FemSelection selection = new FemSelection();

        internal void Initialize()
        {
            selection.elems.Clear();
            selection.nodes.Clear();

            model.materials.Clear();
            model.materials.maxNum = 1;
            model.sections.Clear();
            model.sections.maxNum = 1;
            model.nodes.Clear();
            model.nodes.maxNum = 1;
            model.elems.Clear();
            model.elems.maxNum = 1;
            model.elems.frames.Clear();
            model.elems.plates.Clear();
            model.elems.solids.Clear();
            model.boundaries.Clear();

            loads.Clear();
            loads.maxNum = 1;

            solved = false;
        }

        internal void Select(Element element)
        {
            selection.elems.Add(element);
        }
        internal void Select(Node node)
        {
            selection.nodes.Add(node);
        }
        internal void SelectElems(int strElemNum, int endElemNum)
        {
            foreach (Element elem in model.elems)
            {
                if (strElemNum <= elem.num & elem.num <= endElemNum)
                {
                    selection.elems.Add(elem);
                }
            }
        }
        internal void SelectElemAll()
        {
            selection.elems = new Elements();
            foreach (Element elem in model.elems)
            {
                selection.elems.Add(elem);
            }
        }
        internal void DeselectAll()
        {
            selection.nodes.Clear();
            selection.elems.Clear();
        }
        internal void DeleteSelectedNodes()
        {
            foreach (Node node in selection.nodes)
            {
                model.nodes.Remove(node);
            }
            selection.nodes.Clear();
        }
        internal void DeleteSelectedElems()
        {
            foreach (Element e in selection.elems)
            {
                model.elems.Remove(e);
            }
            selection.elems.Clear();

        }
        internal void DeleteSelected()
        {
            DeleteSelectedElems();
            DeleteSelectedNodes();
        }

        internal void Solve()
        {
            double[,] gloK = model.GloK();
            double[] gloF = GloF();

            //double[,] testMatrix = new double[3, 3] { { 2, -1, 0 }, { 1, 0, -1 },{ 1, 0, 1 } };
            //double[,] testMatrixInv = GF.InverseMatrix_GaussSolver(testMatrix);
            //double[,] textResult = GF.Multiply(testMatrixInv, testMatrix);

            double[,] gloKinv = GF.InverseMatrix_GaussSolver(gloK);
            double[] gloD = GF.Multiply(gloKinv, gloF);

            model.GloD(gloD);

            solved = true;
        }
        internal double[] GloF()
        {
            double[] gloF = new double[model.dof];
            foreach (FemLoad load in loads)
            {
                foreach (NodalLoad nodalLoad in load.nodalLoads)
                {
                    Node node = nodalLoad.node;
                    gloF[node.id[0]] += nodalLoad.force.X;
                    gloF[node.id[1]] += nodalLoad.force.Y;
                    gloF[node.id[2]] += nodalLoad.force.Z;
                    if (node.id[3] != -1) gloF[node.id[3]] += nodalLoad.moment.X;
                    if (node.id[4] != -1) gloF[node.id[4]] += nodalLoad.moment.Y;
                    if (node.id[5] != -1) gloF[node.id[5]] += nodalLoad.moment.Z;
                }
            }
            return gloF;
        }

        internal void Divide(int numDivide)
        {
            foreach (Element elem in selection.elems)
            {
                switch (elem.type)
                {
                    case 21:
                        Frame f = (Frame)elem;
                        Vector3D dir = f.nodes[1].c0 - f.nodes[0].c0;
                        Vector3D ndir = dir / numDivide;
                        Node[] n = new Node[numDivide + 1];
                        n[0] = f.nodes[0];
                        n[numDivide] = f.nodes[1];
                        for (int i = 1; i < numDivide; i++)
                        {
                            Point3D nP = f.nodes[0].c0 + ndir * i;
                            n[i] = model.nodes.Add(nP);
                        }

                        Frame[] fs = new Frame[numDivide];
                        for (int i = 0; i < numDivide; i++)
                        {
                            fs[i] = model.elems.AddFrame(n[i], n[i + 1]);
                            fs[i].type = f.type;
                            fs[i].material = f.material;
                            fs[i].Section(f.Section());
                            fs[i].Section2(f.Section2());


                        }
                        model.elems.Remove(f);

                        break;
                    default:
                        break;
                }
            }
            selection.elems.Clear();
        }
        internal Elements Extrude(Elements elems, Vector3D dir, int iter)
        {
            Elements extrudedElems = new Elements();

            Frames frames = elems.frames;
            if (frames.Count > 0)
            {
                Elements extrudedFrames = Extrude_Frame(frames, dir, iter);
                extrudedElems.Add(extrudedFrames);
            }

            Plates plates = elems.plates;
            if (plates.Count > 0)
            {
                Elements extrudedPlates = Extrude_Plate(plates, dir, iter);
                extrudedElems.Add(extrudedPlates);
            }

            foreach (Element e in elems)
            {
                model.elems.Remove(e);
            }
            return extrudedElems;
        }
        internal Elements Extrude(Vector3D dir, int iter)
        {
            return Extrude(selection.elems, dir, iter);
        }
        private Elements Extrude_Frame(Frames frames, Vector3D dir, int iter)
        {
            Elements extrudedElems = new Elements();

            Nodes nodes = frames.ConnectedNodes();
            Nodes nodesDdp = nodes.Copy(); //de-duplicated
            List<int> nodesNumber = new List<int>();
            List<int> nodesNumberDdp = new List<int>();
            ExtrudedNodeList(nodes, nodesDdp, nodesNumber, nodesNumberDdp);

            Node[,] nodeMatrix = ExtrudedNodeMatrix_AddNode(nodesDdp, iter, dir);

            for (int i = 0; i < iter; i++)
            {
                for (int j = 0; j < frames.Count * 2; j += 2)
                {
                    Plate p = model.elems.AddPlate(nodeMatrix[i, nodesNumber[j]], nodeMatrix[i, nodesNumber[j + 1]], nodeMatrix[i + 1, nodesNumber[j + 1]], nodeMatrix[i + 1, nodesNumber[j]]);
                    p.material = frames[j / 2].material;
                    extrudedElems.Add(p);
                }
            }
            return extrudedElems;
        }
        private Elements Extrude_Plate(Plates plates, Vector3D dir, int iter)
        {
            Elements extrudedElems = new Elements();

            Nodes nodes = plates.ConnectedNodes();
            Nodes nodesDdp = nodes.Copy(); //de-duplicated
            List<int> nodesNumber = new List<int>();
            List<int> nodesNumberDdp = new List<int>();
            ExtrudedNodeList(nodes, nodesDdp, nodesNumber, nodesNumberDdp);

            Node[,] nodeMatrix = ExtrudedNodeMatrix_AddNode(nodesDdp, iter, dir);

            for (int i = 0; i < iter; i++)
            {
                for (int j = 0; j < plates.Count * 4; j += 4)
                {
                    Solid s = model.elems.AddSolid(
                        nodeMatrix[i, nodesNumber[j]], nodeMatrix[i, nodesNumber[j + 1]], nodeMatrix[i, nodesNumber[j + 2]], nodeMatrix[i, nodesNumber[j + 3]],
                        nodeMatrix[i + 1, nodesNumber[j]], nodeMatrix[i + 1, nodesNumber[j + 1]], nodeMatrix[i + 1, nodesNumber[j + 2]], nodeMatrix[i + 1, nodesNumber[j + 3]]
                        );
                    s.material = plates[j / 4].material;
                    extrudedElems.Add(s);
                }
            }
            return extrudedElems;
        }
        private void ExtrudedNodeList(Nodes nodes, Nodes nodesDdp, List<int> nodesNumber, List<int> nodesNumberDdp)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodesNumber.Add(i);
            }
            for (int i = 0; i < nodesDdp.Count; i++)
            {
                nodesNumberDdp.Add(i);
            }
            int count = nodesDdp.Count;
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (nodesDdp[i] == nodesDdp[j])
                    {
                        nodesNumber[nodesNumberDdp[j]] = nodesNumber[nodesNumberDdp[i]];
                        for (int k = nodesNumberDdp[j] + 1; k < nodesNumber.Count; k++)
                        {
                            nodesNumber[k] -= 1;
                        }
                        nodesDdp.RemoveAt(j);
                        nodesNumberDdp.RemoveAt(j);
                        j -= 1;
                        count -= 1;
                    }
                }
            }
        }
        private Node[,] ExtrudedNodeMatrix_AddNode(Nodes nodesDdp, int iter, Vector3D dir)
        {
            Node[,] nodeMatrix = new Node[iter + 1, nodesDdp.Count];
            for (int n = 0; n < nodesDdp.Count; n++)
            {
                nodeMatrix[0, n] = nodesDdp[n];
            }
            for (int i = 1; i < iter + 1; i++)
            {
                for (int n = 0; n < nodesDdp.Count; n++)
                {
                    Node newNode = model.nodes.Add(nodesDdp[n].c0 + dir * i);
                    nodeMatrix[i, n] = newNode;
                }
            }
            return nodeMatrix;
        }

        internal void SelectByWindow(System.Windows.Point p1, System.Windows.Point p2)
        {
            
        }

        internal void SelectByInfinitePyramid(Point3D p0, Vector3D v0, Vector3D v1, Vector3D v2, Vector3D v3)
        {
            Vector3D plane0 = Vector3D.CrossProduct(v0, v1);
            Vector3D plane1 = Vector3D.CrossProduct(v1, v2);
            Vector3D plane2 = Vector3D.CrossProduct(v2, v3);
            Vector3D plane3 = Vector3D.CrossProduct(v3, v0);

            Point3D p;
            bool isOn0;
            bool isOn1;
            bool isOn2;
            bool isOn3;
            foreach (Node node in model.nodes)
            {
                p = node.c0;
                isOn0 = GF.IsPointOnPlane(p, p0, plane0);
                isOn1 = GF.IsPointOnPlane(p, p0, plane1);
                isOn2 = GF.IsPointOnPlane(p, p0, plane2);
                isOn3 = GF.IsPointOnPlane(p, p0, plane3);
                if (isOn0 & isOn1 & isOn2 & isOn3)
                {
                    selection.AddNode(node);
                }
            }
        }

    }
    class FemSelection
    {
        public Nodes nodes = new Nodes();
        public Elements elems = new Elements();
        public int Count
        {
            get
            {
                return nodes.Count + elems.Count;
            }
        }
        public FemSelection()
        {

        }
        internal void AddNode(Node node)
        {
            node.selected = true;
            nodes.Add(node);
        }

        internal void UnselectAll()
        {
            foreach (Node node in nodes)
            {
                node.selected = false;
            }
            nodes.Clear();
            foreach (Element element in elems)
            {
                element.selected = false;
            }
            elems.Clear();

        }
    }
    class FemLoads : List<FemLoad>
    {
        public int maxNum = 1;

        internal double maxForce = 1;
        private double maxMoment = 1;
        internal double viewScale = 1; // 1.0 / 최대하중

        internal NodalLoad AddNodal(Node node, Vector3D force, Vector3D moment)
        {
            NodalLoad n = new NodalLoad(node, force, moment);
            n.num = maxNum;
            maxNum++;

            double norm;
            norm = force.Length;
            if (maxForce < norm) maxForce = norm;

            base.Add(n);
            return n;
        }

        internal double GetMaxLoadLength()
        {
            double maxLoadLength = 1;
            foreach (FemLoad load in this)
            {
                foreach (NodalLoad nodalLoad in load.nodalLoads)
                {
                    if (maxLoadLength < nodalLoad.force.Length)
                    {
                        maxLoadLength = nodalLoad.force.Length;
                    }
                }
            }
            return maxLoadLength;
        }
    }
    class FemLoad
    {
        internal int numNodalLoad = 0;
        internal List<NodalLoad> nodalLoads = new List<NodalLoad>();
        public NodalLoad Add(NodalLoad nodalLoad)
        {
            nodalLoads.Add(nodalLoad);
            numNodalLoad = nodalLoads.Count;
            return nodalLoad;
       }

    }
    class NodalLoad : FemLoad
    {
        internal int num;
        internal Node node;
        internal Vector3D force;
        internal Vector3D moment;
        double[] gloF = new double[6];

        public NodalLoad(Node node, double[] locF)
        {
            this.node = node;
            this.gloF = locF;
            force.X = locF[0];
            force.Y = locF[1];
            force.Z = locF[2];
            moment.X = locF[3];
            moment.Y = locF[4];
            moment.Z = locF[5];
            nodalLoads.Add(this);
        }
        public NodalLoad(Node node, Vector3D vec,Vector3D tor)
        {
            this.node = node;
            force = vec;
            moment = tor;
            gloF[0] = vec.X;
            gloF[1] = vec.Y;
            gloF[2] = vec.Z;
            gloF[3] = tor.X;
            gloF[4] = tor.Y;
            gloF[5] = tor.Z;
            nodalLoads.Add(this);
        }
    }

    class ModelFem
    {
        public readonly Nodes nodes = new Nodes();
        public readonly Elements elems = new Elements();
        public readonly Sections sections = new Sections();
        public readonly MaterialsFem materials = new MaterialsFem();
        public readonly Boundaries boundaries = new Boundaries();

        internal int dof;

        public ModelFem()
        {

        }

        internal void GloD(double[] gloD)
        {
            nodes.UpdateGloD(gloD);
            elems.UpdateGloD();
            UpdateReactionForce();
        }

        internal double[,] GloK()
        {
            SetNodeElemID();

            double[,] glok = new double[dof, dof];

            foreach (Element e in elems)
            {
                double[,] elemGloK = e.GloK();
                for(int i = 0; i < e.dof; i++)
                {
                    for (int j = 0; j < e.dof; j++)
                    {
                        if(e.id[i] >= 0 & e.id[j] >= 0)
                        {
                            glok[e.id[i], e.id[j]] += elemGloK[i, j];
                        }
                    }
                }


            }


            return glok;
        }

        private void SetNodeElemID()
        {
            foreach (Node node in nodes)
            {
                node.id = new int[6] { -1, -1, -1, -1, -1, -1 };
            } // 모든 노드에 -1을 배정
            foreach(Element elem in elems)
            {
                switch (elem.type)
                {
                    case 21: //Frame
                        foreach (Node node in elem.nodes)
                        {
                            node.id = new int[6] {1,1,1,1,1,1};
                        }
                        break;
                    case 40: //Plate
                        foreach (Node node in elem.nodes)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                node.id[i] = 1;
                            }
                        }
                        break;
                    case 80: //Solid
                        foreach (Node node in elem.nodes)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                node.id[i] = 1;
                            }
                        }
                        break;
                    default:
                        break;
                }
            } // 요소에 포함되는 절점만 1을 배정.
            foreach (Boundary boundary in boundaries)
            {
                Node node = boundary.node;
                for (int i = 0; i < 6; i++)
                {
                    if (boundary.condition[i] == 1)
                    {
                        node.id[i] = -1;
                    }
                }
            } // 경계조건은 다시 -1로 배정
            int id = 0;
            foreach (Node n in nodes)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (n.id[i] >= 0)
                    {
                        n.id[i] = id;
                        id++;
                    }
                }
            } // 1로 배정된 모든 노드의 ID를 0부터 순차적으로 ID 배급
            dof = id; // 전체 fem의 자유도 설정
            foreach (Element elem in elems)
            {
                elem.SetID();
            }
        }
        internal void UpdateReactionForce()
        {
            foreach (Node node in nodes)
            {
                node.reactionForce = new double[6];
            }
            foreach (Element elem in this.elems)
            {
                for (int n = 0; n < elem.numNode; n++)
                {
                    switch (elem.type)
                    {
                        case 21:
                            for (int i = 0; i < 6; i++)
                            {
                                elem.nodes[n].reactionForce[i] += elem.gloF[n * 6 + i];
                            }
                            break;
                        case 40:
                        case 80:
                            for (int i = 0; i < 3; i++)
                            {
                                elem.nodes[n].reactionForce[i] += elem.gloF[n * 3 + i];
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    class Nodes : List<Node>
    {
        internal int maxNum = 1;
        internal bool show = true;
        internal bool showNumber = false;

        public Nodes()
        {
        }
        public new Node Add(Node node)
        {
            if (node.num == 0)
            {
                node.num = maxNum;
                maxNum += 1;
            }

            base.Add(node);
            return node;
        }

        internal Node Add(double x, double y, double z)
        {
            return Add(new Node(x, y, z));
        }

        internal Node Add(Point3D point)
        {
            return Add(new Node(point));
        }

        internal Node GetNode(int nodeNumber)
        {
            foreach (Node node in this)
            {
                if (node.num == nodeNumber)
                {
                    return node;
                }
            }
            return null;
        }

        internal void InitializeGloF()
        {
            foreach (Node n in this)
            {
                n.gloF = new double[6] { 0, 0, 0, 0, 0, 0 };
            }
        }

        internal void UpdateGloD(double[] gloD)
        {
            foreach (Node node in this)
            {
                node.gloD = new double[6];
                for (int i = 0; i < 6; i++)
                {
                    if (node.id[i] >= 0)
                    {
                        node.gloD[i] = gloD[node.id[i]];
                    }
                }
                node.UpdateC1();
            }
        }

        internal void RemoveDuplicates()
        {
            Nodes list = this;
            int count = list.Count;
            for (int i = 0; i < count - 1; i++)
            {
                for(int j = i + 1; j < count; j++)
                {
                    if (list[i] == list[j])
                    {
                        list.RemoveAt(j);
                        j -= 1;
                        count -= 1;
                    }
                }
            }
        }

        internal Nodes Copy()
        {
            Nodes newNodes = new Nodes();

            for (int i = 0; i < this.Count; i++)
            {
                Node nn = new Node(0, 0, 0);
                newNodes.Add(nn);
                newNodes[i] = this[i];

            }
            return newNodes;
        }
    }
    class Node
    {
        public int num;
        public Point3D c0; //initialPoint
        public Point3D c1; //initialPoint + disp;
        double x, y, z;
        public int[] id;
        public double[] gloF;
        internal double[] gloD;
        internal double[] reactionForce = { 0, 0, 0, 0, 0, 0 };
        internal bool selected = false;

        public Node(Point3D point)
        {
            this.c0 = point;
            x = point.X;
            y = point.Y;
            z = point.Z;
        }
        public Node(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            c0 = new Point3D(x, y, z);
        }
        internal void UpdateC1()
        {
            c1.X = c0.X + gloD[0];
            c1.Y = c0.Y + gloD[1];
            c1.Z = c0.Z + gloD[2];

        }

    }
    class Elements : List<Element>
    {
        public int maxNum = 1;
        internal int countTruss = 0;
        internal int countFrame = 0;
        internal int countCable = 0;
        internal int countPlate = 0;
        internal int countSolid = 0;
        //internal List<Frame> frames = new List<Frame>();
        internal Frames frames = new Frames();
        internal Plates plates = new Plates();
        internal List<Solid> solids = new List<Solid>();
        internal bool show = true;
        internal bool showNumber = false;

        internal new void Add(Element elem)
        {
            switch (elem.type)
            {
                case 21:
                    Frame f = (Frame)elem;
                    frames.Add(f);
                    break;
                case 40:
                    Plate p = (Plate)elem;
                    plates.Add(p);
                    break;
                case 80:
                    Solid s = (Solid)elem;
                    solids.Add(s);
                    break;
                default:
                    break;
            }
            base.Add(elem);
        }
        internal void Add(Elements elems)
        {
            foreach (Element element in elems)
            {
                Add(element);
            }
        }
        internal Frame AddFrame(Node n1, Node n2)
        {
            Frame f = new Frame(n1, n2);
            f.num = maxNum;
            maxNum++;

            frames.Add(f);
            base.Add(f);
            return f;
        }
        internal Plate AddPlate(Node n1, Node n2, Node n3, Node n4)
        {
            Plate p = new Plate(n1, n2, n3, n4);
            p.num = maxNum;
            maxNum += 1;

            plates.Add(p);
            base.Add(p);
            return p;
        }
        internal Solid AddSolid(Node n1, Node n2, Node n3, Node n4, Node n5, Node n6, Node n7, Node n8)
        {
            Solid s = new Solid(n1, n2, n3, n4, n5, n6, n7, n8);
            s.num = maxNum;
            maxNum += 1;

            solids.Add(s);
            base.Add(s);
            return s;
        }
        internal new bool Remove(Element elem)
        {
            switch (elem.type) //20:Truss, 21:Frame, 25:Cable, 40:Plate, 80:Solid
            {
                case 21:
                    Frame e = (Frame)elem;
                    frames.Remove(e);
                    break;
                case 40:
                    Plate p = (Plate)elem;
                    plates.Remove(p);
                    break;
                case 80:
                    Solid s = (Solid)elem;
                    solids.Remove(s);
                    break;
                default:
                    break;
            }
            return base.Remove(elem);
        }

        internal void UpdateGloD()
        {
            foreach (Element element in this)
            {
                element.UpdateMemberForce001();
            }
        }
        internal void CountElems()
        {
            countTruss = 0;
            countFrame = 0;
            countCable = 0;
            countPlate = 0;
            countSolid = 0;

            foreach (Element element in this)
            {
                switch (element.type) //20:Truss, 21:Frame, 25:Cable, 40:Plate, 80:Solid
                {
                    case 20:
                        countTruss += 1;
                        break;
                    case 21:
                        countFrame += 1;
                        break;
                    case 25:
                        countCable += 1;
                        break;
                    case 40:
                        countPlate += 1;
                        break;
                    case 80:
                        countSolid += 1;
                        break;
                    default:
                        break;
                }

            }
        }
        internal Nodes ConnectedNodes()
        {
            Nodes connectedNodes = new Nodes();
            foreach (Element element in this)
            {
                connectedNodes.AddRange(element.nodes);
            }
            connectedNodes.RemoveDuplicates();
            return connectedNodes;
        } // Elements와 연결된 모든 노드 찾기
        internal static Nodes ConnectedNodes(Elements elems)
        {
            Nodes connectedNodes = new Nodes();
            foreach (Element element in elems)
            {
                connectedNodes.AddRange(element.nodes);
            }
            //connectedNodes.RemoveDuplicates();
            return connectedNodes;
        } // Elements와 연결된 모든 노드 찾기
    }
    class Element
    {
        public int num;
        public int numNode;
        public MaterialFem material;
        public Section section;
        internal List<Node> nodes = new List<Node>();

        internal int type; //20:Truss, 21:Frame, 25:Cable, 40:Plate, 80:Solid
        public int dof;
        public int[] id;

        internal double[,] locK;
        internal double[,] gloK;
        internal double[] locD;
        internal double[] gloD;
        internal double[] gloF;
        internal double[] locF;

        internal double[,] trans;
        internal bool selected = false;

        public Point3D Center
        {
            get
            {
                Point3D center = new Point3D();
                foreach (Node node in nodes)
                {
                    center.X += node.c1.X;
                    center.Y += node.c1.Y;
                    center.Z += node.c1.Z;
                }
                center.X /= numNode;
                center.Y /= numNode;
                center.Z /= numNode;
                return center;
            }
        }

        internal double[,] GloK()
        {
            switch (type)
            {
                case 21:
                    Frame frame = (Frame)this;
                    frame.SetLocK();
                    frame.SetTrans();
                    //LElem(i) % GloK = MATMUL(TRANSPOSE(LElem(i) % Trans), MATMUL(LElem(i) % TanLocK, LElem(i) % Trans))
                    gloK = GF.Multiply(GF.Transpose(trans), GF.Multiply(locK,trans));
                    break;
                case 80:
                    Solid solid = (Solid)this;
                    solid.SetLocK();
                    solid.SetTrans();
                    gloK = locK;
                    break;
                default:
                    gloK = null;
                    break;
            }
            return gloK;
        }

        internal void SetID()
        {
            switch (type)
            {
                case 21:
                    Frame f = (Frame)this;
                    f.id = new int[12];
                    for (int i = 0; i < 6; i++)
                    {
                        f.id[i] = f.nodes[0].id[i];
                        f.id[i+6] = f.nodes[1].id[i];
                    }
                    break;
                case 40:
                    Plate p = (Plate)this;
                    p.id = new int[12];
                    for(int i = 0; i < 3; i++)
                    {
                        p.id[i] = p.nodes[0].id[i];
                        p.id[i + 3] = p.nodes[1].id[i];
                        p.id[i + 6] = p.nodes[2].id[i];
                        p.id[i + 9] = p.nodes[3].id[i];
                    }
                    break;
                case 80:
                    Solid s = (Solid)this;
                    s.id = new int[24];
                    for (int i = 0; i < 3; i++)
                    {
                        s.id[i] = s.nodes[0].id[i];
                        s.id[i+3] = s.nodes[1].id[i];
                        s.id[i+6] = s.nodes[2].id[i];
                        s.id[i+9] = s.nodes[3].id[i];
                        s.id[i+12] = s.nodes[4].id[i];
                        s.id[i+15] = s.nodes[5].id[i];
                        s.id[i+18] = s.nodes[6].id[i];
                        s.id[i+21] = s.nodes[7].id[i];
                    }
                    break;
                default:
                    break;
            }
        }

        internal void UpdateMemberForce001()
        {
            gloD = new double[dof];
            for (int n = 0; n < nodes.Count; n++)
            {
                switch (type)
                {
                    case 21:
                        for (int i = 0; i < 6; i++)
                        {
                            gloD[n * 6 + i] = nodes[n].gloD[i];
                        }
                        break;
                    case 40:
                    case 80:
                        for (int i = 0; i < 3; i++)
                        {
                            gloD[n * 3 + i] = nodes[n].gloD[i];
                        }
                        break;
                    default:
                        break;
                }
            }
            locD = GF.Multiply(trans, gloD);
            gloF = GF.Multiply(gloK, gloD);
            locF = GF.Multiply(trans, gloF);

        }
        internal void UpdateMemberForce()
        {
            gloD = new double[dof];
            for (int n = 0; n < nodes.Count; n++)
            {
                for (int i = 0; i < 6; i++)
                {
                    gloD[n * 6 + i] = nodes[n].gloD[i];
                }
            }
            locD = GF.Multiply(trans, gloD);
            gloF = GF.Multiply(gloK, gloD);
            locF = GF.Multiply(trans, gloF);

        }
    }
    class Frames : List<Frame>
    {
        internal Nodes ConnectedNodes()
        {
            Nodes connectedNodes = new Nodes();
            foreach (Element element in this)
            {
                connectedNodes.AddRange(element.nodes);
            }
            //connectedNodes.RemoveDuplicates();
            return connectedNodes;
        } // Elements와 연결된 모든 노드 찾기
    }
    class Frame : Element
    {
        //internal Node node1, node2;
        private Section section2;
        double E, G, Iy, Iz, J, A, Asy, Asz, L, Piy, Piz, Eq, w, Fx;
        double L1, L2, L3, EIz, EIy, EAoL, GJoL, EqAoL, Lox;
        double AxialDirectionAngle;

        internal void Section(Section sect1)
        {
            section = sect1;
            section2 = sect1;
        }
        internal Section Section()
        {
            return section;
        }
        internal void Section2(Section sect2)
        {
            section2 = sect2;
        }
        internal Section Section2()
        {
            return section2;
        }

        public Frame(Node n1, Node n2)
        {
            type = 21;
            numNode = 2;
            dof = 12;
            nodes.Add(n1);
            nodes.Add(n2);

            L = (n2.c0 - n1.c0).Length;
            AxialDirectionAngle = 0;
        }

        internal void SetTrans()
        {
            trans = new double[12, 12];

            Vector3D dxyz = nodes[1].c0 - nodes[0].c0;
            double dx = dxyz.X;
            double dy = dxyz.Y;
            double dz = dxyz.Z;
            double[,] XYZRM;

            bool ZyxTransform = true;
            if (ZyxTransform)
            {
                XYZRM = GF.TransformMatrix3_ZYX(dx, dy, dz, AxialDirectionAngle);
            }
            else
            {
                XYZRM = GF.TransformMatrix3_YZX(dx, dy, dz, AxialDirectionAngle);
            }
            for (int k = 0; k < 10; k += 3)
            {
                for(int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        trans[k + i, k + j] = XYZRM[i, j];
                    }
                }
            }
        }

        internal void SetLocK()
        {
            locK = new double[12, 12];

            //Get Properities
            E = material.E;
            G = material.G;
            Iy = (section.Iy + section2.Iy) / 2.0d;
            Iz = (section.Iz + section2.Iz) / 2.0d;
            J = (section.J + section2.J) / 2.0d;
            A = (section.A + section2.A) / 2.0d;
            Asy = (section.Asy + section2.Asy) / 2.0d;
            Asz = (section.Asz + section2.Asz) / 2.0d;

            //Variable set
            L1 = L; L2 = L * L; L3 = L2 * L;
            EIz = E * Iz; EIy = E * Iy; EAoL = E * A / L; GJoL = G * J / L;
            Piy = 0.0d; if (Asy != 0) Piy = 12.0d * E * Iz / (G * Asy * L2);
            Piz = 0.0d; if (Asz != 0) Piz = 12.0d * E * Iy / (G * Asz * L2);
            EAoL = E * A / L;

            //First BEnding Matrix
            locK[1, 1] = 12.0d / (L3 * (1.0d + Piy)) * EIz;
            locK[1, 5] = 6.0d / (L2 * (1.0d + Piy)) * EIz;
            locK[1, 7] = -12.0d / (L3 * (1.0d + Piy)) * EIz;
            locK[1, 11] = 6.0d / (L2 * (1.0d + Piy)) * EIz;
            locK[5, 5] = (4.0d + Piy) / (L1 * (1.0d + Piy)) * EIz;
            locK[5, 7] = -6.0d / (L2 * (1.0d + Piy)) * EIz;
            locK[5, 11] = (2.0d - Piy) / (L1 * (1.0d + Piy)) * EIz;
            locK[7, 7] = 12.0d / (L3 * (1.0d + Piy)) * EIz;
            locK[7, 11] = -6.0d / (L2 * (1.0d + Piy)) * EIz;
            locK[11, 11] = (4.0d + Piy) / (L1 * (1.0d + Piy)) * EIz;
            locK[5, 1] = locK[1, 5];
            locK[7, 1] = locK[1, 7];
            locK[7, 5] = locK[5, 7];
            locK[11, 1] = locK[1, 11];
            locK[11, 5] = locK[5, 11];
            locK[11, 7] = locK[7, 11];

            //Second BEnding Matrix
            locK[2, 2] = 12.0d / (L3 * (1.0d + Piz)) * EIy;
            locK[2, 4] = -6.0d / (L2 * (1.0d + Piz)) * EIy;
            locK[2, 8] = -12.0d / (L3 * (1.0d + Piz)) * EIy;
            locK[2, 10] = -6.0d / (L2 * (1.0d + Piz)) * EIy;
            locK[4, 4] = (4.0d + Piz) / (L1 * (1.0d + Piz)) * EIy;
            locK[4, 8] = 6.0d / (L2 * (1.0d + Piz)) * EIy;
            locK[4, 10] = (2.0d - Piz) / (L1 * (1.0d + Piz)) * EIy;
            locK[8, 8] = 12.0d / (L3 * (1.0d + Piz)) * EIy;
            locK[8, 10] = 6.0d / (L2 * (1.0d + Piz)) * EIy;
            locK[10, 10] = (4.0d + Piz) / (L1 * (1.0d + Piz)) * EIy;
            locK[4, 2] = locK[2, 4];
            locK[8, 2] = locK[2, 8];
            locK[8, 4] = locK[4, 8];
            locK[10, 2] = locK[2, 10];
            locK[10, 4] = locK[4, 10];
            locK[10, 8] = locK[8, 10];
            //Axial Matrix
            locK[0, 0] = EAoL;
            locK[0, 6] = -EAoL;
            locK[6, 0] = -EAoL;
            locK[6, 6] = EAoL;
            //!Torsional Matrix
            locK[3, 3] = GJoL;
            locK[3, 9] = -GJoL;
            locK[9, 3] = -GJoL;
            locK[9, 9] = GJoL;
        }
        void SetLocK_old()
        {
            //Get Properities
            E = material.E;
            G = material.G;
            Iy = (section.Iy + section2.Iy) / 2.0d;
            Iz = (section.Iz + section2.Iz) / 2.0d;
            J = (section.J + section2.J) / 2.0d;
            A = (section.A + section2.A) / 2.0d;
            Asy = (section.Asy + section2.Asy) / 2.0d;
            Asz = (section.Asz + section2.Asz) / 2.0d;

            //Variable set
            L1 = L; L2 = L * L; L3 = L2 * L;
            EIz = E * Iz; EIy = E * Iy; EAoL = E * A / L; GJoL = G * J / L;
            Piy = 0.0d; if (Asy != 0) Piy = 12.0d * E * Iz / (G * Asy * L2);
            Piz = 0.0d; if (Asz != 0) Piz = 12.0d * E * Iy / (G * Asz * L2);
            EAoL = E * A / L;

            //First BEnding Matrix
            locK[2, 2] = 12.0d / (L3 * (1.0d + Piy)) * EIz;
            locK[2, 6] = 6.0d / (L2 * (1.0d + Piy)) * EIz;
            locK[2, 8] = -12.0d / (L3 * (1.0d + Piy)) * EIz;
            locK[2, 12] = 6.0d / (L2 * (1.0d + Piy)) * EIz;
            locK[6, 6] = (4.0d + Piy) / (L1 * (1.0d + Piy)) * EIz;
            locK[6, 8] = -6.0d / (L2 * (1.0d + Piy)) * EIz;
            locK[6, 12] = (2.0d - Piy) / (L1 * (1.0d + Piy)) * EIz;
            locK[8, 8] = 12.0d / (L3 * (1.0d + Piy)) * EIz;
            locK[8, 12] = -6.0d / (L2 * (1.0d + Piy)) * EIz;
            locK[12, 12] = (4.0d + Piy) / (L1 * (1.0d + Piy)) * EIz;
            locK[6, 2] = locK[2, 6];
            locK[8, 2] = locK[2, 8];
            locK[8, 6] = locK[6, 8];
            locK[12, 2] = locK[2, 12];
            locK[12, 6] = locK[6, 12];
            locK[12, 8] = locK[8, 12];
            //Second BEnding Matrix
            locK[3, 3] = 12.0d / (L3 * (1.0d + Piz)) * EIy;
            locK[3, 5] = -6.0d / (L2 * (1.0d + Piz)) * EIy;
            locK[3, 9] = -12.0d / (L3 * (1.0d + Piz)) * EIy;
            locK[3, 11] = -6.0d / (L2 * (1.0d + Piz)) * EIy;
            locK[5, 5] = (4.0d + Piz) / (L1 * (1.0d + Piz)) * EIy;
            locK[5, 9] = 6.0d / (L2 * (1.0d + Piz)) * EIy;
            locK[5, 11] = (2.0d - Piz) / (L1 * (1.0d + Piz)) * EIy;
            locK[9, 9] = 12.0d / (L3 * (1.0d + Piz)) * EIy;
            locK[9, 11] = 6.0d / (L2 * (1.0d + Piz)) * EIy;
            locK[11, 11] = (4.0d + Piz) / (L1 * (1.0d + Piz)) * EIy;
            locK[5, 3] = locK[3, 5];
            locK[9, 3] = locK[3, 9];
            locK[9, 5] = locK[5, 9];
            locK[11, 3] = locK[3, 11];
            locK[11, 5] = locK[5, 11];
            locK[11, 9] = locK[9, 11];
            //Axial Matrix
            locK[1, 1] = EAoL;
            locK[1, 7] = -EAoL;
            locK[7, 1] = -EAoL;
            locK[7, 7] = EAoL;
            //!Torsional Matrix
            locK[4, 4] = GJoL;
            locK[4, 10] = -GJoL;
            locK[10, 4] = -GJoL;
            locK[10, 10] = GJoL;
        }

    }
    class Truss : Element
    {
        Node n1, n2;
        public Truss()
        {
            type = 20;
        }

    }
    class Plates : List<Plate>
    {
        internal Nodes ConnectedNodes()
        {
            Nodes connectedNodes = new Nodes();
            foreach (Element element in this)
            {
                connectedNodes.AddRange(element.nodes);
            }
            //connectedNodes.RemoveDuplicates();
            return connectedNodes;
        } // Elements와 연결된 모든 노드 찾기
    }
    class Plate : Element
    {
        public Plate(Node n1, Node n2, Node n3, Node n4)
        {
            type = 40;
            numNode = 4;
            dof = 12;
            nodes.Add(n1);
            nodes.Add(n2);
            nodes.Add(n3);
            nodes.Add(n4);
        }
    }
    class Solid : Element
    {
        //internal Node node1, node2, node3, node4, node5, node6, node7, node8;

        private double[] ri = new double[9];
        private double[] si = new double[9];
        private double[] ti = new double[9];
        private double[] xyz1to8;
        double[,] jacobianMatrix_rst;

        public Solid(Node n1, Node n2, Node n3, Node n4, Node n5, Node n6, Node n7, Node n8)
        {
            type = 80;
            numNode = 8;
            dof = 24;
            nodes.Add(n1);
            nodes.Add(n2);
            nodes.Add(n3);
            nodes.Add(n4);
            nodes.Add(n5);
            nodes.Add(n6);
            nodes.Add(n7);
            nodes.Add(n8);

        }

        internal void SetLocK()
        {
            //ref. https://www.sciencedirect.com/topics/engineering/hexahedron-element

            //xyz1to8 = new double[24];
            //for(int i = 0; i < 8; i++)
            //{
            //    xyz1to8[i * 3 + 0] = nodes[i].c0.X;
            //    xyz1to8[i * 3 + 1] = nodes[i].c0.Y;
            //    xyz1to8[i * 3 + 2] = nodes[i].c0.Z;
            //}

            locK = new double[dof, dof];

            ri[1] = -1; si[1] = -1; ti[1] = -1;
            ri[2] = +1; si[2] = -1; ti[2] = -1;
            ri[3] = +1; si[3] = +1; ti[3] = -1;
            ri[4] = -1; si[4] = +1; ti[4] = -1;
            ri[5] = -1; si[5] = -1; ti[5] = +1;
            ri[6] = +1; si[6] = -1; ti[6] = +1;
            ri[7] = +1; si[7] = +1; ti[7] = +1;
            ri[8] = -1; si[8] = +1; ti[8] = +1;


            double gp = 0.57735;
            locK = GF.MatrixPlus(locK, locKrst(-gp, -gp, -gp));
            locK = GF.MatrixPlus(locK, locKrst(+gp, -gp, -gp));
            locK = GF.MatrixPlus(locK, locKrst(+gp, +gp, -gp));
            locK = GF.MatrixPlus(locK, locKrst(-gp, +gp, -gp));
            locK = GF.MatrixPlus(locK, locKrst(+gp, -gp, +gp));
            locK = GF.MatrixPlus(locK, locKrst(-gp, -gp, +gp));
            locK = GF.MatrixPlus(locK, locKrst(-gp, +gp, +gp));
            locK = GF.MatrixPlus(locK, locKrst(+gp, +gp, +gp));

            double[,] locKrst(double r, double s, double t)
            {
                jacobianMatrix_rst = JacobianMatrix(r, s, t);

                //double[] xyz = GF.Multiply(matN(r, s, t), xyz1to8); //rst를 xyz로 좌표변환

                double[,] matb = matB(r, s, t);
                double E = material.E;
                double[,] matBE = GF.Multiply(matb, E);

                double[,] lockrst = GF.Multiply(GF.Transpose(matBE), matb);
                double detJ = GF.Determinant(jacobianMatrix_rst);
                //detJ = 1;
                lockrst = GF.Multiply(lockrst, detJ);
                return lockrst;
            }

            double[,] matB(double r, double s, double t)
            {
                double[,] matb = new double[6, 3 * 8];
                double[,] dnixyz8 = dNixyz8(r, s, t);

                for(int i=1; i <= 8; i++)
                {
                    matb[0, (i - 1) * 3 + 0] = dnixyz8[0, i]; //dNidx
                    matb[1, (i - 1) * 3 + 1] = dnixyz8[1, i]; //dNidy
                    matb[2, (i - 1) * 3 + 2] = dnixyz8[2, i]; //dNidz
                    matb[3, (i - 1) * 3 + 0] = dnixyz8[1, i]; //dNidy
                    matb[3, (i - 1) * 3 + 1] = dnixyz8[0, i]; //dNidx
                    matb[4, (i - 1) * 3 + 1] = dnixyz8[2, i]; //dNidz
                    matb[4, (i - 1) * 3 + 2] = dnixyz8[1, i]; //dNidy
                    matb[5, (i - 1) * 3 + 0] = dnixyz8[2, i]; //dNidz
                    matb[5, (i - 1) * 3 + 2] = dnixyz8[0, i]; //dNidx
                }
                return matb;
            }

            double[,] dNixyz8(double r, double s, double t)
            {
                double[,] dnixyz8 = new double[3,8+1];
                
                double[,] invJacobi = GF.InverseMatrix_GaussSolver(jacobianMatrix_rst);
                //double[,] iii = GF.Multiply(jacobianMatrix, invJacobi); //jacobianMatrix 검산

                for(int i = 1; i <= 8; i++)
                {
                    double[] dnixyz = GF.Multiply(invJacobi, dNidrst(i, r, s, t));
                    dnixyz8[0, i] = dnixyz[0];
                    dnixyz8[1, i] = dnixyz[1];
                    dnixyz8[2, i] = dnixyz[2];
                }
                return dnixyz8;
            }
            double[] dNixyz(int i, double r, double s, double t)
            {
                //Jacobian을 반복해서 생성하게되므로 가능한 dNixyz8을 사용하는것이 좋음.
                double[] dnixyz = new double[3];
                double[,] invJacobi = GF.InverseMatrix_GaussSolver(jacobianMatrix_rst);
                dnixyz = GF.Multiply(invJacobi, dNidrst(i, r, s, t));
                return dnixyz;
            }

            double[] dNidrst(int i, double r, double s, double t)
            {
                double[] dnidrst = new double[3];
                dnidrst[0] = dNidr(i, r, s, t);
                dnidrst[1] = dNids(i, r, s, t);
                dnidrst[2] = dNidt(i, r, s, t);
                return dnidrst;
            }

            double[,] JacobianMatrix(double r, double s, double t)
            {
                double[,] jacobianMatrix = new double[3, 3];
                for (int i = 1; i <= 8; i++)
                {
                    double dnidr = dNidr(i, r, s, t);
                    jacobianMatrix[0, 0] += dnidr * nodes[i - 1].c0.X; // dx/dr
                    jacobianMatrix[0, 1] += dnidr * nodes[i - 1].c0.Y; // dy/dr
                    jacobianMatrix[0, 2] += dnidr * nodes[i - 1].c0.Z; // dz/dr
                    double dnids = dNids(i, r, s, t);                  //
                    jacobianMatrix[1, 0] += dnids * nodes[i - 1].c0.X; // dx/ds
                    jacobianMatrix[1, 1] += dnids * nodes[i - 1].c0.Y; // dy/ds
                    jacobianMatrix[1, 2] += dnids * nodes[i - 1].c0.Z; // dz/ds
                    double dnidt = dNidt(i, r, s, t);                  //
                    jacobianMatrix[2, 0] += dnidt * nodes[i - 1].c0.X; // dx/dt
                    jacobianMatrix[2, 1] += dnidt * nodes[i - 1].c0.Y; // dy/dt
                    jacobianMatrix[2, 2] += dnidt * nodes[i - 1].c0.Z; // dz/dt
                }
                return jacobianMatrix;
            }

            double dNidr(int i, double r, double s, double t)
            {
                return ri[i] * (1 + s * si[i]) * (1 + t * ti[i]) / 8;
            }
            double dNids(int i, double r, double s, double t)
            {
                return si[i] * (1 + r * ri[i]) * (1 + t * ti[i]) / 8;
            }
            double dNidt(int i, double r, double s, double t)
            {
                return ti[i] * (1 + r * ri[i]) * (1 + s * si[i]) / 8;
            }
            double[,] matN(double r, double s, double t)
            {
                double[,] matn = new double[3, 24];
                for (int i = 1; i <= 8; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        matn[j, (i - 1) * 3 + j] = nirst(i, r, s, t);
                    }
                }
                return matn;
            }

            double nirst(int i, double r, double s, double t)
            {

                double rlt = (1 + r * ri[i]) * (1 + s * si[i]) * (1 + t * ti[i]) / 8;
                return rlt;
            }

        }

        internal void SetTrans()
        {
            trans = GF.IdentityMatrix(dof);
        }
    }

    class MaterialsFem : List<MaterialFem>
    {
        public int maxNum = 1;
        MaterialFem activeMaterial;
        internal MaterialFem AddConcrete(string materialName)
        {
            MaterialFem m;

            if (materialName.Equals("C30"))
            {
                m = new MaterialFem(2e5, 1.5e5);
                m.name = "C30";
                m.num = maxNum;
                maxNum++;

            }
            else
            {
                m = null;
            }

            activeMaterial = m;
            base.Add(m);
            return m;
        }
    }
    class MaterialFem
    {
        internal int num;
        public double E, G;
        internal string name;

        public MaterialFem()
        {
        }
        public MaterialFem(double E, double G)
        {
            this.E = E;
            this.G = G;
        }
    }

    class Sections : List<Section>
    {
        public int maxNum=1;
        Section activeSection;
        internal Section AddRectangle(double width, double height)
        {
            Section s = new RectangleSection(width,height);
            s.num = maxNum;
            maxNum++;

            activeSection = s;

            base.Add(s);
            return s;
        }
    }
    class Section
    {
        internal int num;
        public double Iy, Iz, J, A, Asy, Asz;
        public SectionPoly poly = new SectionPoly();
        internal bool hasSectionPoly = false;

        public Section()
        {

        }
    }
    class RectangleSection : Section
    {
        double b;
        double h;

        public RectangleSection(double b, double h)
        {
            this.b = b;
            this.h = h;
            A = b * h;
            Iy = b * Math.Pow(h, 3) / 12;
            Iz = h * Math.Pow(b, 3) / 12;
            J = Iy + Iz;
            Asy = A; //Todo : 전단면적 고칠 것.
            Asz = A;

            hasSectionPoly = true;
            poly.Add(-b / 2, -h / 2);
            poly.Add(b / 2, -h / 2);
            poly.Add(b / 2, h / 2);
            poly.Add(-b / 2, h / 2);
        }
    }
    
    class Boundaries : List<Boundary>
    {
        int maxNum = 1;
        public Boundary AddBoundary(Node node,int Dx, int Dy, int Dz, int Rx, int Ry, int Rz)
        {
            Boundary b = new Boundary(node, Dx, Dy, Dz, Rx, Ry, Rz);
            b.num = maxNum;
            maxNum++;
            base.Add(b);
            return b;
        }
    }
    class Boundary
    {
        internal int num;
        internal Node node;
        internal int[] condition = new int[6]; //1은 고정, 0은 자유
        public Boundary(Node node, int Dx, int Dy, int Dz, int Rx, int Ry, int Rz)
        {
            this.node = node;
            condition[0] = Dx;
            condition[1] = Dy;
            condition[2] = Dz;
            condition[3] = Rx;
            condition[4] = Ry;
            condition[5] = Rz;
        }
    }

}
