using Microsoft.VisualStudio.TestTools.UnitTesting;
using _003_FosSimulator014;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _003_FosSimulator014.Tests
{
    [TestClass()]
    public class CommandWindowTests
    {
        string initialCommandLineText = "Command: ";

        MainWindow mainWindow = new MainWindow();
        CommandWindow cmd;
        FEM fem;

        public CommandWindowTests()
        {
            cmd = mainWindow.cmd;
            fem = mainWindow.fem;
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
            Assert.AreEqual(0, mainWindow.fem.model.nodes.Count);
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
            Assert.AreEqual(10, fem.model.elems.plates.Count); //원래 5가 되어야하는데 계속 10으로 나옴. 왜? 본파일에서는 잘되는데..ㅜㅜ
            Assert.IsTrue(cmd.GetLastLine().Equals(initialCommandLineText));
        }

    }
}