using System;
using System.IO;
using System.Threading.Tasks;

namespace Mill.Persistence
{
    public class MillFileDataAccess : IMillDataAccess
    {

        private String? _directory = String.Empty;

        public MillFileDataAccess(String? saveDirectory = null)
        {
            _directory = saveDirectory;
        }
        public async Task SaveAsync(String path, MillTable table)
        {
            if (!String.IsNullOrEmpty(_directory))
                path = Path.Combine(_directory, path);

            try
            {
                using (StreamWriter writer = new StreamWriter(path)) // open file
                {
                    writer.Write(table.LastPlayer); // 
                    await writer.WriteLineAsync(" " + table.Player1UnusedToken 
                                                + " " + table.Player2UnusedToken 
                                                + " " + table.Player1TokenInTable 
                                                + " " + table.Player2TokenInTable 
                                                + " " + table.CurrentAction);
                    for (Int32 i = 0; i < table.Fields.Length; i++)
                    {
                        await writer.WriteAsync(table.GetField(i).Player + " ");
                    }
                }
            }
            catch
            {
                throw new MillDataException(path);
            }
        }

        public async Task<MillTable> LoadAsync(String path)
        {
            if (!String.IsNullOrEmpty(_directory))
                path = Path.Combine(_directory, path);

            try
            {
                using (StreamReader reader = new StreamReader(path)) // open file
                {
                    String line = await reader.ReadLineAsync();
                    String[] numbers = line.Split(' '); 
                    //first line in file:
                    //last player who stepped,
                    //first player's talon number 
                    //sexond player's talon number
                    //first player token in table
                    //second player token in table
                    //next action that will happen after load
                    int lastPlayer = int.Parse(numbers[0]); 
                    int player1UnusedToken = int.Parse(numbers[1]);
                    int player2UnusedToken = int.Parse(numbers[2]);
                    int player1TokenInTable = int.Parse(numbers[3]);
                    int player2TokenInTable = int.Parse(numbers[4]);
                    int currentAction = int.Parse(numbers[5]);
                    MillTable table = new MillTable();
                    table.LastPlayer = lastPlayer;
                    table.Player1UnusedToken= player1UnusedToken;
                    table.Player2UnusedToken= player2UnusedToken;
                    table.Player1TokenInTable = player1TokenInTable;
                    table.Player2TokenInTable = player2TokenInTable;
                    table.CurrentAction = currentAction;

                    line = await reader.ReadLineAsync();
                    numbers = line.Split(' ');
                    for (int i = 0; i < table.Fields.Length; i++)
                    {
                        table.GetField(i).Player = int.Parse(numbers[i]);
                    }

                    return table;
                }
            }
            catch (Exception ex)
            {
                throw new MillDataException(path);
            }
            
        }
    }
}
