using System.Data.SQLite;

namespace ServerConsole
{
    internal class SQLiteCredentialsChecker
    {
        public static string connectionString;
        public static int CheckCredentials(string command,string login, string password)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                var selectCommand = new SQLiteCommand(
                    "SELECT Password,IsBanned,IsOnline FROM Users WHERE Username = @username",
                    connection
                );
                selectCommand.Parameters.AddWithValue("@username", login);
                using (var reader = selectCommand.ExecuteReader())
                {
                    if (reader.Read() && command.StartsWith("L")) // such user was found
                    {
                        string truePassword = reader.GetString(0);
                        if (truePassword.Equals(password))
                        {
                            bool isBanned = reader.GetBoolean(1);
                            if (isBanned)//Checking if user is banned
                            {
                                return 3;
                            }
                            bool isAlreadyOnline = reader.GetBoolean(2);
                            if (isAlreadyOnline)
                            {
                                return 2;
                            }
                            var updateCommand = new SQLiteCommand("UPDATE Users SET IsOnline = 1 WHERE Username = @username",connection);
                            updateCommand.Parameters.AddWithValue("@username", login);
                            updateCommand.ExecuteNonQuery();
                            return 0; //Credentials are valid, user is not band and not already online
                        }
                        else
                        {
                            return 1;
                        }
                    }
                    if (command.StartsWith("S")) 
                    {
                        var insertCommand = new SQLiteCommand("INSERT INTO Users(Username,Password,IsBanned,IsOnline) values(@username,@password,0,1)",connection);
                        insertCommand.Parameters.AddWithValue("@username", login);
                        insertCommand.Parameters.AddWithValue("@password", password);
                        insertCommand.ExecuteNonQuery();
                        return 0;
                    }
                    return 1;
                }
            }
            //check if they are valid, AND wether he is already logged in
            //send 0 if everything is ok
            //send 1 if invalid password or login not found
            //send 2 if user is already logged in
            //send 3 if user is banned
        }

        public static void LogOut(string login)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var updateCommand = new SQLiteCommand("UPDATE Users SET IsOnline = 0 WHERE Username = @username", connection);
                updateCommand.Parameters.AddWithValue("@username", login);
                updateCommand.ExecuteNonQuery();
            }
        }
    }
}
