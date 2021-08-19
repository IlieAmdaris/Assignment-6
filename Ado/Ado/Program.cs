using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Ado
{
    class Program
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["sqlconnection"].ConnectionString;
        static string creationString = @"D:\C#\SQL\Assignment6\Creation.txt";
        static string insert = @"D:\C#\SQL\Assignment6\insert.txt";
        public static DataTable usersTable;
        public static SqlDataAdapter adapter;
        static void Main(string[] args)
        {
            CreateSQLCommands(creationString);
            CreateSQLCommands(insert);
            adapter = GetUsersAdapter();
            usersTable = GetUsersTable();
            adapter.Fill(usersTable);
            InsertRowInUsersTable(new User { FirstName = "Sam", LastName = "Green", Age = 20 });
            UpdateRowInUsersTable(new User { FirstName = "Neil", LastName = "Breen", Age = 21 }, 1);
            DeleteRowInUserstable(2);
            Print();
        }
        static void CreateSQLCommands(string source)
        {
            string sqlCommandText = GetCommand(source);
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var sqlCommand = new SqlCommand(sqlCommandText, connection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
        private static DataTable GetUsersTable()
        {
            DataTable users = new DataTable("Users");
            users.Columns.Add("FirstName", typeof(string));
            users.Columns.Add("LastName", typeof(string));
            users.Columns.Add("Age", typeof(int));
            users.Columns.Add("Id", typeof(int));
            return users;
        }
        private static DataTable GetPostsTable()
        {
            DataTable posts = new DataTable("Posts");
            posts.Columns.Add("Id", typeof(int));
            posts.Columns.Add("Content", typeof(string));
            return posts;
        }
        private static SqlDataAdapter GetUsersAdapter()
        {
            SqlDataAdapter tempAdapter = new SqlDataAdapter("SELECT * FROM Users;", connectionString);
            tempAdapter.InsertCommand = new SqlCommand("INSERT INTO Users(FirstName, LastName, Age) " +
                "VALUES (@FirstName,@LastName,@Age)");
            tempAdapter.InsertCommand.Parameters.Add("@FirstName", SqlDbType.VarChar, 50, "FirstName");
            tempAdapter.InsertCommand.Parameters.Add("@LastName", SqlDbType.VarChar, 50, "LastName");
            tempAdapter.InsertCommand.Parameters.Add("@Age", SqlDbType.Int,120,"Age");

            tempAdapter.UpdateCommand = new SqlCommand("UPDATE Users" +
                " SET FirstName = @FirstName, LastName = @LastName, Age = @Age WHERE Id = @Id");
            tempAdapter.UpdateCommand.Parameters.Add("@FirstName", SqlDbType.VarChar, 50, "FirstName");
            tempAdapter.UpdateCommand.Parameters.Add("@LastName", SqlDbType.VarChar, 50, "LastName");
            tempAdapter.UpdateCommand.Parameters.Add("@Age", SqlDbType.Int, 120, "Age");
            SqlParameter UpdateParameter = tempAdapter.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int);
            UpdateParameter.SourceColumn = "Id";
            UpdateParameter.SourceVersion = DataRowVersion.Original;
            tempAdapter.DeleteCommand = new SqlCommand("DELETE FROM Users WHERE Id = @Id");
            SqlParameter deleteParameter = tempAdapter.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int);
            deleteParameter.SourceColumn = "Id";
            deleteParameter.SourceVersion = DataRowVersion.Original;
            return tempAdapter;
        }
        public static void UpdateRowInUsersTable(User updatedUser,int id)
        {
            foreach (DataRow row in usersTable.Rows)
            {
                if ((int) row["Id"] == id)
                {
                    row["FirstName"] = updatedUser.FirstName;
                    row["LastName"] = updatedUser.LastName;
                    row["Age"] = updatedUser.Age;
                }
            }
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                adapter.UpdateCommand.Connection = sqlConnection;
                adapter.Update(usersTable);
            }
        }
        public static void DeleteRowInUserstable(int id)
        {
            foreach (DataRow row in usersTable.Rows)
            {
                if ((int)row["Id"] == id)
                {
                    row.Delete();
                }
            }
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                adapter.DeleteCommand.Connection = sqlConnection;
                adapter.UpdateCommand.Connection = sqlConnection;
                adapter.Update(usersTable);
            }
        }
        public static void Print()
        {
            foreach(DataRow row in usersTable.Rows)
            {
                Console.WriteLine($"{row["FirstName"]} " +
                    $"{row["LastName"]} {row["Age"] } Id: {row["Id"]}" );
            }
        }
        public static void InsertRowInUsersTable(User insertUser)   
        {
            var row = usersTable.NewRow();
            row["FirstName"] = insertUser.FirstName;
            row["LastName"] = insertUser.LastName;
            row["Age"] = insertUser.Age;
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                usersTable.Rows.Add(row);
                adapter.UpdateCommand.Connection = sqlConnection;
                adapter.InsertCommand.Connection = sqlConnection;
                adapter.Update(usersTable);
                usersTable = GetUsersTable();
                adapter.Fill(usersTable);
            }
        }
        static string GetCommand(params string[] filePath)
        {
            
            string command = "";
            foreach(string line in filePath)
            {
                List<string> lines = new List<string>();
                lines = File.ReadAllLines(line).ToList();
                foreach (string entry in lines)
                {
                    command += entry + "\n";
                }
            }
            return command;
        }
        public class User
        {
            public string FirstName { get; set;}
            public string LastName { get; set; }
            public int Age { get; set; }
            public User(string firstName,string lastName,int age)
            {
                FirstName = firstName;
                LastName = lastName;
                Age = age;
            }
            public User()
            {

            }
        }
    }
}
