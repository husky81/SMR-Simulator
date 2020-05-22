using bck.SMR_simulator.general_functions;
using bck.SMR_simulator.main;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace bck.SMR_simulator.finiteElementMethod
{
    public class FEM
    {
        public readonly FemModel model = new FemModel();
        public readonly FemLoadCollection loads = new FemLoadCollection();
        internal bool solved = false;

        public FemSelection selection;


        public FEM()
        {
            selection = new FemSelection(this);
        }
        internal void Initialize()
        {
            DeleteAllNode();

            model.materials.Clear();
            model.materials.maxNum = 1;
            model.sections.Clear();
            model.sections.maxNum = 1;
        }

        internal void Select(FemNode node)
        {
            selection.AddNode(node);
        }
        internal void SelectNode(int n)
        {
            foreach (FemNode node in model.nodes)
            {
                if (node.num == n)
                {
                    selection.AddNode(node);
                }
            }
        }
        internal void SelectNode(List<int> nList)
        {
            foreach (int i in nList)
            {
                SelectNode(i);
            }
        }
        internal void SelectElem(int elemNumber)
        {
            foreach (FemElement elem in model.elems)
            {
                if (elem.Num == elemNumber)
                {
                    selection.AddElement(elem);
                    break;
                }
            }
        }
        internal void SelectElem(List<int> nList)
        {
            foreach (int i in nList)
            {
                SelectElem(i);
            }
        }
        internal void Select(FemElement element)
        {
            selection.AddElement(element);
        }
        internal void SelectElems(int strElemNum, int endElemNum)
        {
            foreach (FemElement elem in model.elems)
            {
                if (strElemNum <= elem.Num & elem.Num <= endElemNum)
                {
                    selection.AddElement(elem);
                }
            }
        }
        internal void SelectElemAll()
        {
            selection.Clear();
            selection.AddElement(model.elems);
        }
        /// <summary>
        /// 꼭지점과 4개의 벡터로 표현되는 무한 피라미드의 내부에 있는 모든 요소를 선택합니다.
        /// </summary>
        internal void SelectByInfinitePyramid(Point3D p0, Vector3D v0, Vector3D v1, Vector3D v2, Vector3D v3)
        {
            FemNodeCollection selectedNodes = new FemNodeCollection();

            Vector3D plane0 = Vector3D.CrossProduct(v0, v1);
            Vector3D plane1 = Vector3D.CrossProduct(v1, v2);
            Vector3D plane2 = Vector3D.CrossProduct(v2, v3);
            Vector3D plane3 = Vector3D.CrossProduct(v3, v0);

            Point3D p;
            bool isOn0;
            bool isOn1;
            bool isOn2;
            bool isOn3;
            foreach (FemNode node in model.nodes)
            {
                node.selectedAtThisTime = false;
            }
            foreach (FemNode node in model.nodes)
            {
                if (IsNodeOn(node))
                {
                    selection.AddNode(node);
                    node.selectedAtThisTime = true;
                }

                selectedNodes.Add(node);
            }
            bool flag;
            foreach (FemElement element in model.elems)
            {
                flag = true;
                foreach (FemNode node in element.nodes)
                {
                    if (node.selectedAtThisTime == false) flag = false;
                }
                if (flag)
                {
                    selection.AddElement(element);
                }
            }

            //중복제거
            selection.nodes.Distinct();
            selection.elems.Distinct();

            return;

            bool IsNodeOn(FemNode node)
            {
                p = node.c0;
                isOn0 = GF.IsPointUpperPlane(p, p0, plane0);
                isOn1 = GF.IsPointUpperPlane(p, p0, plane1);
                isOn2 = GF.IsPointUpperPlane(p, p0, plane2);
                isOn3 = GF.IsPointUpperPlane(p, p0, plane3);
                if (isOn0 & isOn1 & isOn2 & isOn3)
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 꼭지점과 4개의 벡터로 표현되는 무한 피라미드의 내부에 있는 모든 요소를 선택합니다.
        /// 라인에 걸리는거 다 선택.
        /// </summary>
        internal void SelectByInfinitePyramid_Cross(Point3D p0, Vector3D v0, Vector3D v1, Vector3D v2, Vector3D v3)
        {
            SelectByInfinitePyramid(p0, v0, v1, v2, v3);
            SelectByInfiniteTriangle(p0, v0, v1);
            SelectByInfiniteTriangle(p0, v1, v2);
            SelectByInfiniteTriangle(p0, v2, v3);
            SelectByInfiniteTriangle(p0, v3, v0);
        }
        /// <summary>
        /// 꼭지점과 2개의 벡터로 표현되는 부채꼴 면에 걸리는 모든 요소를 선택.
        /// </summary>
        internal void SelectByInfiniteTriangle(Point3D p0, Vector3D v0, Vector3D v1)
        {
            FemElementCollections selected1 = new FemElementCollections();
            Vector3D plane = Vector3D.CrossProduct(v0, v1);

            //절점 하나라도 평면에 걸리면 선택
            foreach (FemElement element in model.elems)
            {
                bool firstFlag = GF.IsPointUpperPlane(element.nodes[0].c0, p0, plane);
                bool flag;
                for (int i = 1; i < element.nodes.Count; i++)
                {
                    flag = GF.IsPointUpperPlane(element.nodes[i].c0, p0, plane);
                    if (flag != firstFlag)
                    {
                        selected1.Add(element);
                        break;
                    }
                }
            }

            FemElementCollections selected2 = new FemElementCollections();
            Point3D n0;
            Point3D n1;
            Point3D crossPoint;
            Vector3D downPlaneAxis;
            Vector3D upPlaneAxis;
            bool dFlag;
            bool uFlag;
            foreach (FemElement elem in selected1)
            {
                for (int i = 0; i < elem.nodes.Count - 1; i++)
                {
                    n0 = elem.nodes[i].c0;
                    n1 = elem.nodes[i + 1].c0;
                    crossPoint = GF.CrossPointBetweenPlaneAndLine(plane, p0, n0, n1);
                    if (crossPoint.X == 0 & crossPoint.Y == 0 & crossPoint.Z == 0)
                    {
                        continue;
                    }

                    downPlaneAxis = Vector3D.CrossProduct(plane, v0);
                    upPlaneAxis = Vector3D.CrossProduct(v1, plane);
                    dFlag = GF.IsPointUpperPlane(crossPoint, p0, downPlaneAxis);
                    uFlag = GF.IsPointUpperPlane(crossPoint, p0, upPlaneAxis);
                    if (dFlag & uFlag)
                    {
                        selected2.Add(elem);
                    }
                }
            }

            selection.AddElement(selected2);
            selected1.Clear();
            selected2.Clear();
        }
        internal void DeselectAll()
        {
            selection.DeselectAll();
        }

        internal void DeleteSelectedNodes()
        {
            foreach (FemNode node in selection.nodes)
            {
                foreach (FemElement elem in node.connectedElements)
                {
                    model.elems.Remove(elem);
                    selection.elems.Remove(elem);
                }
                model.nodes.Remove(node);
            }
            selection.nodes.Clear();
        }
        internal void DeleteSelectedElems()
        {
            foreach (FemElement e in selection.elems)
            {
                model.elems.Remove(e);
            }
            selection.elems.Clear();

        }
        internal void DeleteSelection()
        {
            DeleteSelectedElems();
            DeleteSelectedNodes();
        }
        internal void DeleteAllNode()
        {
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

        internal void Check(CommandWindow cmd)
        {
            //재료와 단면이 설정되지 않은경우 아무거나 입력.
            if(model.materials.Count == 0)
            {
                WarningMessage("재료 모델이 설정되지 않아 임의 모델을 추가합니다.");
                model.materials.AddConcrete("C30");
            }
            if (model.sections.Count == 0)
            {
                WarningMessage("단면 정보가 설정되지 않아 임의 단면을 추가합니다.");
                model.sections.AddRectangle(0.2, 0.2);
            }

            //Material이 하나뿐인 경우 그냥 다 1번으로 지정
            if (model.materials.Count == 1)
            {
                FemMaterial material = model.materials[0];
                foreach (FemElement element in model.elems)
                {
                    element.Material = material;
                }
            }

            //Section이 하나뿐인 경우 그냥 다 1번으로 지정
            if (model.sections.Count == 1)
            {
                FemSection section = model.sections[0];
                foreach (FemElement element in model.elems)
                {
                    element.Section = section;
                }
            }
        }

        internal CommandWindow cmd; //FEM 메시지를 보내기위한 용도로만 활용.
        private void ErrorMessage(string message)
        {
            cmd.ErrorMessage(message);
        }
        private void WarningMessage(string message)
        {
            cmd.WarningMessage(message);
        }

        internal double[] GloF()
        {
            double[] gloF = new double[model.dof];
            foreach (FemLoad load in loads)
            {
                foreach (FemNodalLoad nodalLoad in load.nodalLoads)
                {
                    FemNode node = nodalLoad.node;
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

        internal void AddForceSelectedNodes(Vector3D force)
        {
            Vector3D moment = new Vector3D(0, 0, 0);
            AddNodalLoadSelectedNodes(force, moment);
        }
        internal void AddMomentSelectedNodes(Vector3D moment)
        {
            Vector3D force = new Vector3D(0, 0, 0);
            AddNodalLoadSelectedNodes(force, moment);
        }
        internal void AddNodalLoadSelectedNodes(Vector3D force, Vector3D moment)
        {
            foreach (FemNode node in selection.nodes)
            {
                loads.AddNodal(node, force, moment);
            }
        }

        internal void DivideSelectedElems(int numDivide)
        {
            selection.Deduplicate();
            foreach (FemElement elem in selection.elems)
            {
                switch (elem.type)
                {
                    case 21:
                        FemFrame f = (FemFrame)elem;
                        Vector3D dir = f.nodes[1].c0 - f.nodes[0].c0;
                        Vector3D ndir = dir / numDivide;
                        FemNode[] n = new FemNode[numDivide + 1];
                        n[0] = f.nodes[0];
                        n[numDivide] = f.nodes[1];
                        for (int i = 1; i < numDivide; i++)
                        {
                            Point3D nP = f.nodes[0].c0 + ndir * i;
                            n[i] = model.nodes.Add(nP);
                        }

                        FemFrame[] fs = new FemFrame[numDivide];
                        for (int i = 0; i < numDivide; i++)
                        {
                            fs[i] = model.elems.AddFrame(n[i], n[i + 1]);
                            fs[i].type = f.type;
                            fs[i].Material = f.Material;
                            fs[i].Section = f.Section;
                        }
                        model.elems.Remove(f);

                        break;
                    default:
                        break;
                }
            }
            selection.elems.Clear();
        }

        internal FemElementCollections Extrude(FemElementCollections elems, Vector3D dir, int iter)
        {
            FemElementCollections extrudedElems = new FemElementCollections();

            FemFrames frames = elems.frames;
            if (frames.Count > 0)
            {
                FemElementCollections extrudedFrames = Extrude_Frame(frames, dir, iter);
                extrudedElems.Add(extrudedFrames);
            }

            FemPlates plates = elems.plates;
            if (plates.Count > 0)
            {
                FemElementCollections extrudedPlates = Extrude_Plate(plates, dir, iter);
                extrudedElems.Add(extrudedPlates);
            }

            foreach (FemElement e in elems)
            {
                model.elems.Remove(e);
            }
            return extrudedElems;
        }
        internal FemElementCollections ExtrudeSelectedElems(Vector3D dir, int iter)
        {
            return Extrude(selection.elems, dir, iter);
        }
        internal void ExtrudeWoReturn(Vector3D dir, int iter)
        {
            Extrude(selection.elems, dir, iter);
        }
        private FemElementCollections Extrude_Frame(FemFrames frames, Vector3D dir, int iter)
        {
            FemElementCollections extrudedElems = new FemElementCollections();

            FemNodeCollection nodes = frames.ConnectedNodes();
            FemNodeCollection nodesDdp = nodes.Copy(); //de-duplicated
            List<int> nodesNumber = new List<int>();
            List<int> nodesNumberDdp = new List<int>();
            ExtrudedNodeList(nodes, nodesDdp, nodesNumber, nodesNumberDdp);

            FemNode[,] nodeMatrix = ExtrudedNodeMatrix_AddNode(nodesDdp, iter, dir);

            for (int i = 0; i < iter; i++)
            {
                for (int j = 0; j < frames.Count * 2; j += 2)
                {
                    FemPlate p = model.elems.AddPlate(nodeMatrix[i, nodesNumber[j]], nodeMatrix[i, nodesNumber[j + 1]], nodeMatrix[i + 1, nodesNumber[j + 1]], nodeMatrix[i + 1, nodesNumber[j]]);
                    p.Material = frames[j / 2].Material;
                    extrudedElems.Add(p);
                }
            }
            return extrudedElems;
        }
        private FemElementCollections Extrude_Plate(FemPlates plates, Vector3D dir, int iter)
        {
            FemNodeCollection nodes = plates.ConnectedNodes();
            FemNodeCollection nodesDdp = nodes.Copy(); //de-duplicated
            List<int> nodesNumber = new List<int>();
            List<int> nodesNumberDdp = new List<int>();

            //Extrude에 참여하는 노드를 요소단위로 중복한 노드 순서 및 중복 제거한 노드 순서 반환.
            ExtrudedNodeList(nodes, nodesDdp, nodesNumber, nodesNumberDdp);

            //중복을 제거한 노드리스트를 dir 방향으로 iter번 반복해서 절점 생성.
            FemNode[,] nodeMatrix = ExtrudedNodeMatrix_AddNode(nodesDdp, iter, dir);

            FemElementCollections extrudedElems = new FemElementCollections();
            for (int i = 0; i < iter; i++)
            {
                for (int j = 0; j < plates.Count * 4; j += 4)
                {
                    FemNode n0 = nodeMatrix[i, nodesNumber[j]];
                    FemNode n1 = nodeMatrix[i, nodesNumber[j + 1]];
                    FemNode n2 = nodeMatrix[i, nodesNumber[j + 2]];
                    FemNode n3 = nodeMatrix[i, nodesNumber[j + 3]];
                    FemNode n4 = nodeMatrix[i + 1, nodesNumber[j]];
                    FemNode n5 = nodeMatrix[i + 1, nodesNumber[j + 1]];
                    FemNode n6 = nodeMatrix[i + 1, nodesNumber[j + 2]];
                    FemNode n7 = nodeMatrix[i + 1, nodesNumber[j + 3]];
                    FemSolid s = model.elems.AddSolid(n0,n1,n2,n3,n4,n5,n6,n7);
                    s.Material = plates[j / 4].Material;
                    extrudedElems.Add(s);
                }
            }
            return extrudedElems;
        }
        private void ExtrudedNodeList(FemNodeCollection nodes, FemNodeCollection nodesDdp, List<int> nodesNumber, List<int> nodesNumberDdp)
        {
            //nodes : extrude에 참여하는 모든 요소의 절점을 순서대로 중복해서 저장한 배열임.
            //nodesDdp(Out) : 중복되는 노드를 제거한 배열.
            //nodesNumber(Out) : 중복 제거전 배열 크기 그대로, 중복 제거 노드리스트 nodesDdp의 위치를 저장한 리스트. 이 번호를 사용해서 요소를 생성하면 중복문제 해결~
            //nodesNumberDdp(Out) : nodesNumber를 만들기위해 보조적으로 사용됨.

            //nodeNumber와 nodeNumberDdp 생성.
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
                List<int> equalJs = new List<int>();

                //i번째 노드와 중복되는 노드의 번호들을 euqlJs에 저장.
                for (int j = i + 1; j < count; j++)
                {
                    //int tmpi = nodesNumberDdp[i];
                    //int tmpj = nodesNumberDdp[j];

                    if (nodesDdp[i] == nodesDdp[j])
                    {
                        if (nodesNumber[nodesNumberDdp[j]] != nodesNumber[nodesNumberDdp[i]]) //이미 중복넘버로 처리된 내용은 스킵.
                        {
                            nodesNumber[nodesNumberDdp[j]] = nodesNumber[nodesNumberDdp[i]];
                            equalJs.Add(j);
                        }
                    }
                }
                //i번째 노드와 중복되는 모든 번호들을 Ddp에서 제거하고, node넘버 땡김.
                if (equalJs.Count > 0)
                {
                    for (int j = equalJs.Count - 1; j >= 0; j--) //equalJs를 뒤에서 부터 하나씩 제거함.
                    {
                        for (int k = nodesNumberDdp[equalJs[j]] + 1; k < nodesNumber.Count; k++)
                        {
                            if (nodesNumber[k] > equalJs[j])  //이미 중복처리 된 노드번호들을 그대로 놔둠. 제거할 번호보다 앞에 있는 번호들은 그냥 놔둠.
                            {
                                nodesNumber[k] -= 1; //제거할 번호보다 뒤에 있는 번호들을 하나씩 빼서 제거 후에 노드번호로 맞춤.
                            }
                        }
                        nodesDdp.RemoveAt(equalJs[j]);
                        nodesNumberDdp.RemoveAt(equalJs[j]);
                        for (int k = equalJs.Count - 1; k > j; k--)
                        {
                            equalJs[k] -= 1;  //Ddp리스트에서 제거되면 equalJs 번호도 하나씩 빼줘야됨.
                        }
                        count -= 1; //중복되는 번호가 하나 빠지면 반복 회수도 줄여야함.
                    }
                }
            }
        }
        private FemNode[,] ExtrudedNodeMatrix_AddNode(FemNodeCollection nodesDdp, int iter, Vector3D dir)
        {
            //중복을 제거한 노드리스트를 dir 방향으로 iter번 반복해서 절점 생성.
            FemNode[,] nodeMatrix = new FemNode[iter + 1, nodesDdp.Count];
            for (int n = 0; n < nodesDdp.Count; n++)
            {
                nodeMatrix[0, n] = nodesDdp[n];
            }
            for (int i = 1; i < iter + 1; i++)
            {
                for (int n = 0; n < nodesDdp.Count; n++)
                {
                    FemNode newNode = model.nodes.Add(nodesDdp[n].c0 + dir * i);
                    nodeMatrix[i, n] = newNode;
                }
            }
            return nodeMatrix;
        }

    }
    public class FemSelection
    {
        private readonly FEM fem;
        internal FemNodeCollection nodes = new FemNodeCollection();
        internal FemElementCollections elems = new FemElementCollections();
        public int Count
        {
            get
            {
                return nodes.Count + elems.Count;
            }
        }

        public FemSelection(FEM fem)
        {
            this.fem = fem;
        }

        internal void AddNode(FemNode node)
        {
            node.selected = true;
            nodes.Add(node);
        }
        internal void AddElement(FemElement element)
        {
            element.selected = true;
            elems.Add(element);
        }
        internal void AddElement(FemElementCollections elements)
        {
            foreach (FemElement element in elements)
            {
                AddElement(element);
            }
        }
        internal void DeselectAll()
        {
            foreach (FemNode node in nodes)
            {
                node.selected = false;
            }
            nodes.Clear();
            foreach (FemElement element in elems)
            {
                element.selected = false;
            }
            elems.Clear();

        }
        internal void Delete()
        {
            fem.DeleteSelection();
        }
        internal void Clear()
        {
            DeselectAll();
        }
        internal void Deduplicate()
        {
            nodes.Deduplicate();
            elems.Deduplicate();
        }
    }

    public class FemModel
    {
        public readonly FemNodeCollection nodes = new FemNodeCollection();
        public readonly FemElementCollections elems = new FemElementCollections();
        public readonly FemSectionCollection sections = new FemSectionCollection();
        public readonly FemMaterialCollection materials = new FemMaterialCollection();
        public readonly FemBoundarieCollection boundaries = new FemBoundarieCollection();

        internal int dof;

        public FemModel()
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

            foreach (FemElement e in elems)
            {
                double[,] elemGloK = e.GloK();
                for (int i = 0; i < e.dof; i++)
                {
                    for (int j = 0; j < e.dof; j++)
                    {
                        if (e.id[i] >= 0 & e.id[j] >= 0)
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
            foreach (FemNode node in nodes)
            {
                node.id = new int[6] { -1, -1, -1, -1, -1, -1 };
            } // 모든 노드에 -1을 배정
            foreach (FemElement elem in elems)
            {
                switch (elem.type)
                {
                    case 21: //Frame
                        foreach (FemNode node in elem.nodes)
                        {
                            node.id = new int[6] { 1, 1, 1, 1, 1, 1 };
                        }
                        break;
                    case 40: //Plate
                        foreach (FemNode node in elem.nodes)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                node.id[i] = 1;
                            }
                        }
                        break;
                    case 80: //Solid
                        foreach (FemNode node in elem.nodes)
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
            foreach (FemBoundary boundary in boundaries)
            {
                FemNode node = boundary.node;
                for (int i = 0; i < 6; i++)
                {
                    if (boundary.condition[i] == 1)
                    {
                        node.id[i] = -1;
                    }
                }
            } // 경계조건은 다시 -1로 배정
            int id = 0;
            foreach (FemNode n in nodes)
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
            foreach (FemElement elem in elems)
            {
                elem.SetID();
            }
        }
        internal void UpdateReactionForce()
        {
            foreach (FemNode node in nodes)
            {
                node.reactionForce = new double[6];
            }
            foreach (FemElement elem in this.elems)
            {
                for (int n = 0; n < elem.NumNode; n++)
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
    public class FemMaterialCollection : List<FemMaterial>
    {
        public int maxNum = 1;
        FemMaterial activeMaterial;
        internal FemMaterial AddConcrete(string materialName)
        {
            FemMaterial m;

            if (materialName.Equals("C30"))
            {
                m = new FemMaterial(2e5, 1.5e5);
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
    public class FemMaterial
    {
        internal int num;
        public double E, G;
        internal string name;

        public FemMaterial()
        {
        }
        public FemMaterial(double E, double G)
        {
            this.E = E;
            this.G = G;
        }
    }
    public class FemSectionCollection : List<FemSection>
    {
        public int maxNum = 1;
        FemSection activeSection;
        internal FemSection AddRectangle(double width, double height)
        {
            FemSection s = new FemRectangleSection(width, height);
            s.num = maxNum;
            maxNum++;

            activeSection = s;

            base.Add(s);
            return s;
        }
    }
    public class FemSection
    {
        internal int num;
        public double Iy, Iz, J, A, Asy, Asz;
        public SectionPoly poly = new SectionPoly();
        internal bool hasSectionPoly = false;

        public FemSection()
        {

        }
    }
    public class FemRectangleSection : FemSection
    {
        double b;
        double h;

        public FemRectangleSection(double b, double h)
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

    public class FemNodeCollection : List<FemNode>
    {
        internal int maxNum = 1;
        internal bool visibility = true;
        private double positionTolerance = 0.0000001;

        public FemNodeCollection()
        {
        }

        internal void Add_NoReturn(Point3D p0)
        {
            Add(p0.X, p0.Y, p0.Z);
        }
        internal FemNode Add(double x, double y, double z)
        {
            return Add(new Point3D(x, y, z));
        }
        internal FemNode Add(Point3D p0)
        {
            //p0의 위치에 노드가 있는지 검사하고 있으면 그 노드를 반환하고 끝냄
            FemNode existedNode = GetNode(p0);
            if (existedNode != null) return existedNode;

            //노드 생성
            FemNode node = new FemNode(p0)
            {
                num = maxNum
            };
            maxNum += 1;
            base.Add(node);
            return node;
        }

        private FemNode GetNode(Point3D p0)
        {
            foreach (FemNode node in this)
            {
                if ((node.c0 - p0).Length < positionTolerance)
                {
                    return node;
                }
            }
            return null;
        }

        internal FemNode GetNode(int nodeNumber)
        {
            foreach (FemNode node in this)
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
            foreach (FemNode n in this)
            {
                n.gloF = new double[6] { 0, 0, 0, 0, 0, 0 };
            }
        }

        internal void UpdateGloD(double[] gloD)
        {
            foreach (FemNode node in this)
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
        internal void Deduplicate()
        {
            FemNodeCollection list = this;
            int count = list.Count;
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
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

        internal FemNodeCollection Copy()
        {
            FemNodeCollection newNodes = new FemNodeCollection();

            for (int i = 0; i < this.Count; i++)
            {
                FemNode nn = new FemNode(0, 0, 0);
                newNodes.Add(nn);
                newNodes[i] = this[i];

            }
            return newNodes;
        }
    }
    public class FemNode
    {
        public int num;
        /// <summary>
        /// 사용자가 입력한 절점의 좌표입니다. initialPoint.
        /// </summary>
        public Point3D c0;
        /// <summary>
        /// 해석결과로 나온 변위를 포함한 절점의 좌표입니다. initialPoint + disp;
        /// </summary>
        public Point3D c1;
        public int[] id;
        public double[] gloF;
        internal double[] gloD;
        internal double[] reactionForce = { 0, 0, 0, 0, 0, 0 };
        internal bool selected = false;
        internal FemElementCollections connectedElements = new FemElementCollections();
        internal bool selectedAtThisTime;

        public FemNode(Point3D point)
        {
            c0 = point;
        }
        public FemNode(double x, double y, double z)
        {
            c0 = new Point3D(x, y, z);
        }
        internal void UpdateC1()
        {
            c1.X = c0.X + gloD[0];
            c1.Y = c0.Y + gloD[1];
            c1.Z = c0.Z + gloD[2];

        }

    }

    public class FemElementCollections : List<FemElement>
    {
        public int maxNum = 1;
        //internal int countTruss = 0;
        //internal int countFrame = 0;
        //internal int countCable = 0;
        //internal int countPlate = 0;
        //internal int countSolid = 0;
        //internal List<Frame> frames = new List<Frame>();
        public FemFrames frames = new FemFrames();
        public FemPlates plates = new FemPlates();
        public List<FemSolid> solids = new List<FemSolid>();
        internal bool show = true;
        internal bool showNumber = false;

        internal new void Add(FemElement elem)
        {
            switch (elem.type)
            {
                case 21:
                    FemFrame f = (FemFrame)elem;
                    frames.Add(f);
                    break;
                case 40:
                    FemPlate p = (FemPlate)elem;
                    plates.Add(p);
                    break;
                case 80:
                    FemSolid s = (FemSolid)elem;
                    solids.Add(s);
                    break;
                default:
                    break;
            }
            base.Add(elem);
        }
        internal void Add(FemElementCollections elems)
        {
            foreach (FemElement element in elems)
            {
                Add(element);
            }
        }
        internal new void Clear()
        {
            base.Clear();
            frames.Clear();
            plates.Clear();
            solids.Clear();
        }
        internal FemFrame AddFrame(FemNode n1, FemNode n2)
        {
            FemFrame f = new FemFrame(n1, n2);
            f.Num = maxNum;
            maxNum++;

            frames.Add(f);
            base.Add(f);
            return f;
        }
        internal FemPlate AddPlate(FemNode n1, FemNode n2, FemNode n3, FemNode n4)
        {
            FemPlate p = new FemPlate(n1, n2, n3, n4);
            p.Num = maxNum;
            maxNum += 1;

            plates.Add(p);
            base.Add(p);
            return p;
        }
        internal FemSolid AddSolid(FemNode n1, FemNode n2, FemNode n3, FemNode n4, FemNode n5, FemNode n6, FemNode n7, FemNode n8)
        {
            FemSolid s = new FemSolid(n1, n2, n3, n4, n5, n6, n7, n8);
            s.Num = maxNum;
            maxNum += 1;

            solids.Add(s);
            base.Add(s);
            return s;
        }
        internal new bool Remove(FemElement elem)
        {
            switch (elem.type) //20:Truss, 21:Frame, 25:Cable, 40:Plate, 80:Solid
            {
                case 21:
                    FemFrame e = (FemFrame)elem;
                    frames.Remove(e);
                    break;
                case 40:
                    FemPlate p = (FemPlate)elem;
                    plates.Remove(p);
                    break;
                case 80:
                    FemSolid s = (FemSolid)elem;
                    solids.Remove(s);
                    break;
                default:
                    break;
            }
            return base.Remove(elem);
        }

        internal void UpdateGloD()
        {
            foreach (FemElement element in this)
            {
                element.UpdateMemberForce001();
            }
        }
        internal FemNodeCollection ConnectedNodes()
        {
            FemNodeCollection connectedNodes = new FemNodeCollection();
            foreach (FemElement element in this)
            {
                connectedNodes.AddRange(element.nodes);
            }
            connectedNodes.Deduplicate();
            return connectedNodes;
        } // Elements와 연결된 모든 노드 찾기
        internal static FemNodeCollection ConnectedNodes(FemElementCollections elems)
        {
            FemNodeCollection connectedNodes = new FemNodeCollection();
            foreach (FemElement element in elems)
            {
                connectedNodes.AddRange(element.nodes);
            }
            //connectedNodes.RemoveDuplicates();
            return connectedNodes;
        } // Elements와 연결된 모든 노드 찾기

        internal void Deduplicate()
        {
            FemElementCollections list = this;
            int count = list.Count;
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
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
    }
    public class FemElement
    {
        private int num;
        public int Num { get => num; set => num = value; }
        private int numNode;
        public int NumNode { get => numNode; set => numNode = value; }
        private FemMaterial material;
        public FemMaterial Material { get => material; set => material = value; }
        private FemSection section;
        public FemSection Section
        {
            get
            {
                return section;
            }
            set
            {
                section = value;
                section2 = value;
            }
        }
        private FemSection section2;
        public FemSection Section2
        {
            get
            {
                return section2;
            }
            set
            {
                section2 = value;
            }
        }


        internal List<FemNode> nodes = new List<FemNode>();

        internal int type; //20:Truss, 21:Frame, 25:Cable, 40:Plate, 80:Solid
        internal int dof;
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
                foreach (FemNode node in nodes)
                {
                    center.X += node.c1.X;
                    center.Y += node.c1.Y;
                    center.Z += node.c1.Z;
                }
                center.X /= NumNode;
                center.Y /= NumNode;
                center.Z /= NumNode;
                return center;
            }
        }

        internal double[,] GloK()
        {
            switch (type)
            {
                case 21:
                    FemFrame frame = (FemFrame)this;
                    frame.SetLocK();
                    frame.SetTrans();
                    //LElem(i) % GloK = MATMUL(TRANSPOSE(LElem(i) % Trans), MATMUL(LElem(i) % TanLocK, LElem(i) % Trans))
                    gloK = GF.Multiply(GF.Transpose(trans), GF.Multiply(locK, trans));
                    break;
                case 80:
                    FemSolid solid = (FemSolid)this;
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
                    FemFrame f = (FemFrame)this;
                    f.id = new int[12];
                    for (int i = 0; i < 6; i++)
                    {
                        f.id[i] = f.nodes[0].id[i];
                        f.id[i + 6] = f.nodes[1].id[i];
                    }
                    break;
                case 40:
                    FemPlate p = (FemPlate)this;
                    p.id = new int[12];
                    for (int i = 0; i < 3; i++)
                    {
                        p.id[i] = p.nodes[0].id[i];
                        p.id[i + 3] = p.nodes[1].id[i];
                        p.id[i + 6] = p.nodes[2].id[i];
                        p.id[i + 9] = p.nodes[3].id[i];
                    }
                    break;
                case 80:
                    FemSolid s = (FemSolid)this;
                    s.id = new int[24];
                    for (int i = 0; i < 3; i++)
                    {
                        s.id[i] = s.nodes[0].id[i];
                        s.id[i + 3] = s.nodes[1].id[i];
                        s.id[i + 6] = s.nodes[2].id[i];
                        s.id[i + 9] = s.nodes[3].id[i];
                        s.id[i + 12] = s.nodes[4].id[i];
                        s.id[i + 15] = s.nodes[5].id[i];
                        s.id[i + 18] = s.nodes[6].id[i];
                        s.id[i + 21] = s.nodes[7].id[i];
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
        protected void AddConnectedElementAtNodes(FemElement elem)
        {
            foreach (FemNode node in nodes)
            {
                node.connectedElements.Add(elem);
            }
        }
    }
    public class FemFrames : List<FemFrame>
    {
        internal FemNodeCollection ConnectedNodes()
        {
            FemNodeCollection connectedNodes = new FemNodeCollection();
            foreach (FemElement element in this)
            {
                connectedNodes.AddRange(element.nodes);
            }
            //connectedNodes.RemoveDuplicates();
            return connectedNodes;
        } // Elements와 연결된 모든 노드 찾기
    }
    public class FemFrame : FemElement
    {
        //internal Node node1, node2;
        double E, G, Iy, Iz, J, A, Asy, Asz, L, Piy, Piz, Eq, w, Fx;
        double L1, L2, L3, EIz, EIy, EAoL, GJoL, EqAoL, Lox;
        double AxialDirectionAngle;

        public FemFrame(FemNode n1, FemNode n2)
        {
            type = 21;
            NumNode = 2;
            dof = 12;
            nodes.Add(n1);
            nodes.Add(n2);

            AddConnectedElementAtNodes(this);

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
                for (int i = 0; i < 3; i++)
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
            E = Material.E;
            G = Material.G;
            Iy = (Section.Iy + Section2.Iy) / 2.0d;
            Iz = (Section.Iz + Section2.Iz) / 2.0d;
            J = (Section.J + Section2.J) / 2.0d;
            A = (Section.A + Section2.A) / 2.0d;
            Asy = (Section.Asy + Section2.Asy) / 2.0d;
            Asz = (Section.Asz + Section2.Asz) / 2.0d;

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
            E = Material.E;
            G = Material.G;
            Iy = (Section.Iy + Section2.Iy) / 2.0d;
            Iz = (Section.Iz + Section2.Iz) / 2.0d;
            J = (Section.J + Section2.J) / 2.0d;
            A = (Section.A + Section2.A) / 2.0d;
            Asy = (Section.Asy + Section2.Asy) / 2.0d;
            Asz = (Section.Asz + Section2.Asz) / 2.0d;

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
    public class FemTruss : FemElement
    {
        FemNode n1, n2;
        public FemTruss()
        {
            type = 20;
            AddConnectedElementAtNodes(this);
        }

    }
    public class FemPlates : List<FemPlate>
    {
        internal FemNodeCollection ConnectedNodes()
        {
            FemNodeCollection connectedNodes = new FemNodeCollection();
            foreach (FemElement element in this)
            {
                connectedNodes.AddRange(element.nodes);
            }
            //connectedNodes.RemoveDuplicates();
            return connectedNodes;
        } // Elements와 연결된 모든 노드 찾기
    }
    public class FemPlate : FemElement
    {
        public FemPlate(FemNode n1, FemNode n2, FemNode n3, FemNode n4)
        {
            type = 40;
            NumNode = 4;
            dof = 12;
            nodes.Add(n1);
            nodes.Add(n2);
            nodes.Add(n3);
            nodes.Add(n4);
            AddConnectedElementAtNodes(this);
        }
    }
    public class FemSolid : FemElement
    {
        //internal Node node1, node2, node3, node4, node5, node6, node7, node8;

        private double[] ri = new double[9];
        private double[] si = new double[9];
        private double[] ti = new double[9];
        private double[] xyz1to8;
        double[,] jacobianMatrix_rst;

        public FemSolid(FemNode n1, FemNode n2, FemNode n3, FemNode n4, FemNode n5, FemNode n6, FemNode n7, FemNode n8)
        {
            type = 80;
            NumNode = 8;
            dof = 24;
            nodes.Add(n1);
            nodes.Add(n2);
            nodes.Add(n3);
            nodes.Add(n4);
            nodes.Add(n5);
            nodes.Add(n6);
            nodes.Add(n7);
            nodes.Add(n8);
            AddConnectedElementAtNodes(this);
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
                double E = Material.E;
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

                for (int i = 1; i <= 8; i++)
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
                double[,] dnixyz8 = new double[3, 8 + 1];

                double[,] invJacobi = GF.InverseMatrix_GaussSolver(jacobianMatrix_rst);
                //double[,] iii = GF.Multiply(jacobianMatrix, invJacobi); //jacobianMatrix 검산

                for (int i = 1; i <= 8; i++)
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

    public class FemBoundarieCollection : List<FemBoundary>
    {
        internal bool visibility = true;
        int maxNum = 1;
        public FemBoundary AddBoundary(FemNode node,int Dx, int Dy, int Dz, int Rx, int Ry, int Rz)
        {
            FemBoundary b = new FemBoundary(node, Dx, Dy, Dz, Rx, Ry, Rz);
            b.num = maxNum;
            maxNum++;
            base.Add(b);
            return b;
        }
    }
    public class FemBoundary
    {
        internal int num;
        internal FemNode node;
        internal int[] condition = new int[6]; //1은 고정, 0은 자유
        public FemBoundary(FemNode node, int Dx, int Dy, int Dz, int Rx, int Ry, int Rz)
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

    public class FemLoadCollection : List<FemLoad>
    {
        public int maxNum = 1;

        internal double maxForce = 1;
        private double maxMoment = 1;
        internal double viewScale = 1; // 1.0 / 최대하중

        internal FemNodalLoad AddNodal(FemNode node, Vector3D force, Vector3D moment)
        {
            FemNodalLoad n = new FemNodalLoad(node, force, moment);
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
                foreach (FemNodalLoad nodalLoad in load.nodalLoads)
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
    public class FemLoad
    {
        internal int numNodalLoad = 0;
        internal List<FemNodalLoad> nodalLoads = new List<FemNodalLoad>();
        public FemNodalLoad Add(FemNodalLoad nodalLoad)
        {
            nodalLoads.Add(nodalLoad);
            numNodalLoad = nodalLoads.Count;
            return nodalLoad;
        }

    }
    public class FemNodalLoad : FemLoad
    {
        internal int num;
        internal FemNode node;
        internal Vector3D force;
        internal Vector3D moment;
        double[] gloF = new double[6];

        public FemNodalLoad(FemNode node, double[] locF)
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
        public FemNodalLoad(FemNode node, Vector3D vec, Vector3D tor)
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
}