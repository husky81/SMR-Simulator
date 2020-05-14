using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace _FosSimulator.Tests
{
    [TestClass()]
    public class CommandWindowTests
    {
        string initialCommandLineText = "Command: ";

        MainWindow main = new MainWindow();
        CommandWindow cmd;
        FEM fem;

        public CommandWindowTests()
        {
            cmd = main.cmd;
            fem = main.fem;
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
            Assert.AreEqual(0, main.fem.model.nodes.Count);
            Assert.AreEqual(0, main.fem.model.elems.Count);
        }

        [TestMethod()]
        public void MakeOneLineTest()
        {
            EraseAll();
            cmd.Call("Line");
            cmd.Call("0,0");
            cmd.Call("10,0");
            cmd.Call(" ");

            Assert.AreEqual(2, fem.model.nodes.Count);
            Assert.AreEqual(1, fem.model.elems.Count);
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
            Assert.AreEqual(11, fem.model.nodes.Count);
            Assert.AreEqual(10, fem.model.elems.Count);
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
                Assert.AreEqual(12, fem.model.nodes.Count);
                Assert.AreEqual(5, fem.model.elems.plates.Count);
                Assert.IsTrue(cmd.GetLastLine().Equals(initialCommandLineText));
            }
        }

    }
}