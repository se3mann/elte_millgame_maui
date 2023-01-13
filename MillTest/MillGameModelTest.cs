using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using Mill.Persistence;
using Mill.Model;

namespace MillTest
{
    [TestClass]
    public class MillGameModelTest
    {
        private MillGameModel? _model;
        private MillTable? _mockedTable;
        private Mock<IMillDataAccess>? _mock;

        [TestInitialize]
        public void Initialize()
        {
            /*  
             *  Top left field is filled with first player token
             *  top middle field is filled by second player token
             *  First player will turn with an adding from talon.
             */
            _mockedTable = new MillTable();
            _mockedTable.GetField(0).Player = 0;
            _mockedTable.GetField(1).Player = 1;
            _mockedTable.LastPlayer = 2;
            _mockedTable.Player1UnusedToken = 8;
            _mockedTable.Player2UnusedToken = 8;
            _mockedTable.Player1TokenInTable = 1;
            _mockedTable.Player2TokenInTable = 1;
            _mockedTable.CurrentAction = 0;

            _mock = new Mock<IMillDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult(_mockedTable));

            _model = new MillGameModel(_mock.Object);

            _model.GameAdvanced += new EventHandler<MillEventArgs>(Model_GameAdvanced);
            _model.GameOver += new EventHandler<MillEventArgs>(Model_GameOver);
            _model.PlayerHasToPass += new EventHandler<PassingEventArgs>(Model_PlayerHasToPass);
        }

        [TestMethod]
        public async Task MillGameModelLoadTest()
        {
            _model.NewGame();

            await _model.LoadGameAsync(String.Empty);

            for (int i = 0; i < _model.Table.Fields.Length; i++)
            {
                Assert.AreEqual(_mockedTable.GetField(i), _model.Table.GetField(i));
            }

            Assert.AreEqual(Player.Player1, _model.CurrentPlayer);
            Assert.AreEqual(2, _model.Table.LastPlayer);

            _mock.Verify(dataAccess => dataAccess.LoadAsync(String.Empty), Times.Once());
        }

        private void Model_GameAdvanced(Object sender, MillEventArgs e)
        {
            Assert.AreEqual(e.CurrentPlayer, _model.CurrentPlayer);
            Assert.AreEqual(e.CurrentAction, _model.CurrentAction);
            Assert.AreEqual(e.CurrentPlayerWin, false);
            Assert.AreEqual(e.Player1Talon, _model.Table.Player1UnusedToken);
            Assert.AreEqual(e.Player2Talon, _model.Table.Player2UnusedToken);
        }

        private void Model_GameOver(Object sender, MillEventArgs e)
        {
            Assert.IsTrue(_model.IsGameOver);
            Assert.IsTrue(e.CurrentPlayerWin);
            Assert.AreEqual(e.CurrentAction, Mill.Model.Action.Removing);
        }

        private void Model_PlayerHasToPass(Object sender, PassingEventArgs e)
        {
            bool moveToken;
            if (_model.CurrentAction == Mill.Model.Action.MoveDest) moveToken = true;
            else moveToken = false;
            Assert.AreEqual(e.MoveToken, moveToken);
        }

        [TestMethod]
        public void MillGameModelNewGameTest()
        {
            _model.NewGame();
            Assert.AreEqual(_model.CurrentAction, Mill.Model.Action.Adding);
            Assert.AreEqual(_model.CurrentPhase, GamePhase.Phase1);
            Assert.AreEqual(_model.CurrentPlayer, Player.Player1);
            Assert.AreEqual(_model.CurrentPlayerWon, false);
            Assert.AreEqual(_model.Table.Player1UnusedToken, 9);
            Assert.AreEqual(_model.Table.Player2UnusedToken, 9);

            for (int i = 0; i < _model.Table.Fields.Length; i++)
            {
                Assert.AreEqual(_model.Table.GetField(i).Player, 0);
            }
        }

        [TestMethod]
        public void MillGameModelStepTest()
        {
            _model.NewGame();

            _model.Step(0); //First player to left upper corner field
            Assert.AreEqual(_model.Table.GetField(0).Player, 1);
            Assert.AreEqual(_model.CurrentPlayer, Player.Player2); //after step, we swapped player

            _model.Step(1); //Second player next to that field, left direction
            Assert.AreEqual(_model.Table.GetField(1).Player, 2);
            Assert.AreEqual(_model.CurrentPlayer, Player.Player1);

            _model.Step(1); //failed step from First player, field not free
            Assert.AreEqual(_model.Table.GetField(1).Player, 2); //nothing happened
            Assert.AreEqual(_model.CurrentPlayer, Player.Player1);
        }

        [TestMethod]
        public void MillGameModelMillTest()
        {
            _model.NewGame();

            _model.Step(0); //first player
            _model.Step(10); //second player
            _model.Step(1); //first player
            _model.Step(6); //second player
            _model.Step(2); //first player, this is a mill
            Assert.IsTrue(_model.Table.CheckFieldInMill(1, 0)); //params: player, fieldIndex
            Assert.IsTrue(_model.Table.CheckFieldInMill(1, 1));
            Assert.IsTrue(_model.Table.CheckFieldInMill(1, 2));
            Assert.AreEqual(_model.CurrentPlayer, Player.Player1);
            Assert.AreEqual(_model.CurrentAction, Mill.Model.Action.Removing);

            //removing
            _model.Step(10);
            Assert.AreEqual(_model.CurrentPlayer, Player.Player2);
            Assert.AreEqual(_model.Table.GetField(10).Player, 0);
        }

        [TestMethod]
        public void MillGameModelWin()
        {
            _model.NewGame();
            _model.Table.Player1UnusedToken = 0;
            _model.Table.Player2UnusedToken = 0;
            _model.Table.Player2TokenInTable = 3;
            _model.Table.Player2TokenInTable = 3;
            _model.CurrentPhase = GamePhase.Phase2;
            _model.CurrentAction = Mill.Model.Action.MoveDest;
            _model.Table.GetField(0).Player = 1;
            _model.Table.GetField(1).Player = 1;
            _model.Table.GetField(14).Player = 1;
            _model.Table.GetField(3).Player = 2;
            _model.Table.GetField(4).Player = 2;
            _model.Table.GetField(6).Player = 2;

            _model.Step(14); //first player select token to move
            _model.Step(2); //first player make mill
            _model.Step(3); //first player remove

            Assert.AreEqual(_model.CurrentPlayerWon, true);
        }
    }
}