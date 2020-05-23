using BCK.SmrSimulator.finiteElementMethod;
using BCK.SmrSimulator.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace _FosSimulator.Tests
{
    [TestClass()]
    public class CommandWindowTests
    {
        readonly string initialCommandLineText = "Command: ";
        readonly MainWindow main = new MainWindow();
        readonly CommandWindow cmd;
        readonly FEM fem;

        public CommandWindowTests()
        {
            cmd = main.Cmd;
            fem = main.Fem;
        }

        public void EraseAll()
        {
            cmd.Call("Erase");
            cmd.Call("All");
        }

        [TestMethod()]
        public void EraseAllTest()
        {
            EraseAll();
            Assert.AreEqual(0, main.Fem.Model.Nodes.Count);
            Assert.AreEqual(0, main.Fem.Model.Elems.Count);
        }

        [TestMethod()]
        public void MakeOneLineTest()
        {
            EraseAll();
            cmd.Call("Line");
            cmd.Call("0,0");
            cmd.Call("10,0");
            cmd.Call(" ");

            Assert.AreEqual(2, fem.Model.Nodes.Count);
            Assert.AreEqual(1, fem.Model.Elems.Count);
            Assert.IsTrue(cmd.GetLastLine().Equals(initialCommandLineText));
        }

        [TestMethod()]
        public void DivideTest()
        {
            EraseAll();
            cmd.Call("Line");
            cmd.Call("0,0");
            cmd.Call("10,0");
            cmd.Call(" ");
            cmd.Call("Select");
            cmd.Call("Element");
            cmd.Call("1");
            cmd.Call("Divide");
            cmd.Call("10");
            Assert.AreEqual(11, fem.Model.Nodes.Count);
            Assert.AreEqual(10, fem.Model.Elems.Count);
            Assert.IsTrue(cmd.GetLastLine().Equals(initialCommandLineText));
        }

        [TestMethod()]
        public void ExtrudeTest()
        {
            for(int i = 0; i < 2; i++)
            {
                cmd.Call("Erase");
                cmd.Call("All");
                cmd.Call("Line");
                cmd.Call("0,0");
                cmd.Call("10,0");
                cmd.Call(" ");
                cmd.Call("Select");
                cmd.Call("Element");
                cmd.Call("1");
                cmd.Call("Extrude");
                cmd.Call("@0,1");
                cmd.Call("5");
                Assert.AreEqual(12, fem.Model.Nodes.Count);
                Assert.AreEqual(5, fem.Model.Elems.Plates.Count);
                Assert.IsTrue(cmd.GetLastLine().Equals(initialCommandLineText));
            }
        }

    }
}