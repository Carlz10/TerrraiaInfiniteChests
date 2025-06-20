﻿using Microsoft.Xna.Framework;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace TerrariaInfiniteChests
{
	public static class DB
	{
		private static IDbConnection db;

		public static void Connect()
		{
			switch (TShock.Config.Settings.StorageType.ToLower())
			{
				case "mysql":
					string[] dbHost = TShock.Config.Settings.MySqlHost.Split(':');
					db = new MySqlConnection()
					{
						ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
							dbHost[0],
							dbHost.Length == 1 ? "3306" : dbHost[1],
							TShock.Config.Settings.MySqlDbName,
							TShock.Config.Settings.MySqlUsername,
							TShock.Config.Settings.MySqlPassword)

					};
					break;

                case "sqlite":
                string sql = Path.Combine(TShock.SavePath, "InfChests3.sqlite");
                // Explicitly set connection string without any version parameter
                db = new SqliteConnection(new SqliteConnectionStringBuilder
                {
                    DataSource = sql,
                    // No Version parameter here!
                }.ConnectionString);
                break;
			}

			SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

			sqlcreator.EnsureTableStructure(new SqlTable("InfChests3",
				new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, Length = 7, AutoIncrement = true },
				new SqlColumn("UserID", MySqlDbType.Int32) { Length = 6 },
				new SqlColumn("X", MySqlDbType.Int32) { Length = 6 },
				new SqlColumn("Y", MySqlDbType.Int32) { Length = 6 },
				new SqlColumn("Items", MySqlDbType.Text) { Length = 500 },
				new SqlColumn("Public", MySqlDbType.Int32) { Length = 1 },
				new SqlColumn("Users", MySqlDbType.Text) { Length = 500 },
				new SqlColumn("Groups", MySqlDbType.Text) { Length = 500 },
				new SqlColumn("Refill", MySqlDbType.Int32) { Length = 5 },
				new SqlColumn("WorldID", MySqlDbType.Int32) { Length = 15 }));
		}

		public static InfChest GetChest(int x, int y)
		{
			string query = $"SELECT * FROM InfChests3 WHERE X = {x} AND Y = {y} AND WorldID = {Main.worldID};";
			using (var reader = db.QueryReader(query))
			{
				if (reader.Read())
				{
					InfChest chest = new InfChest(reader.Get<int>("UserID"), x, y, Main.worldID)
					{
						id = reader.Get<int>("ID"),
						isPublic = reader.Get<int>("Public") == 1 ? true : false,
						refill = reader.Get<int>("Refill")
					};
					chest.StringToGroups(reader.Get<string>("Groups"));
					chest.StringToUsers(reader.Get<string>("Users"));
					chest.StringToItems(reader.Get<string>("Items"));
					return chest;
				}
				else
					return null;
			}
		}

		public static InfChest GetChest(int id)
		{
			string query = $"SELECT * FROM InfChests3 WHERE ID = {id} AND WorldID = {Main.worldID};";
			using (var reader = db.QueryReader(query))
			{
				if (reader.Read())
				{
					InfChest chest = new InfChest(reader.Get<int>("UserID"), reader.Get<int>("X"), reader.Get<int>("Y"), Main.worldID)
					{
						id = id,
						isPublic = reader.Get<int>("Public") == 1 ? true : false,
						refill = reader.Get<int>("Refill")
					};
					chest.StringToGroups(reader.Get<string>("Groups"));
					chest.StringToUsers(reader.Get<string>("Users"));
					chest.StringToItems(reader.Get<string>("Items"));
					return chest;
				}
				else
					return null;
			}
		}

		public static List<Chest> GetAllChests()
		{
			string query = "SELECT * FROM InfChests3 WHERE WorldID = " + Main.worldID + ";";
			using (var reader = db.QueryReader(query))
			{
				List<Chest> chests = new List<Chest>();
				while (reader.Read())
				{
					InfChest ichest = new InfChest(-1, reader.Get<int>("X"), reader.Get<int>("Y"), Main.worldID);
					ichest.StringToItems(reader.Get<string>("Items"));
					Chest chest = new Chest()
					{
						item = ichest.items,
						x = ichest.x,
						y = ichest.y
					};
					chests.Add(chest);
				}
				return chests;
			}
		}

        public static void AddChest(InfChest chest)
        {
            if (db == null)
            {
                // Attempt to initialize the database connection
                Connect();
                if (db == null)
                {
                    throw new InvalidOperationException("Database connection is not initialized. Call DB.Connect() before using this method.");
                }
            }

            if (chest == null)
            {
                throw new ArgumentNullException(nameof(chest), "The chest object cannot be null.");
            }

            string query = $"INSERT INTO InfChests3 (UserID, X, Y, Items, Public, Users, Groups, Refill, WorldID) VALUES ({chest.userid}, {chest.x}, {chest.y}, '{chest.ItemsToString()}', {(chest.isPublic ? 1 : 0)}, '{chest.UsersToString()}', '{chest.GroupsToString()}', {chest.refill}, {Main.worldID});";
            db.Query(query);
        }


        public static void UpdateUser(InfChest chest)
		{
			string query = $"UPDATE InfChests3 SET UserID = {chest.userid} WHERE ID = {chest.id};";
			db.Query(query);
		}

		public static void UpdateItems(InfChest chest)
		{
			string query = $"UPDATE InfChests3 SET Items = '{chest.ItemsToString()}' WHERE ID = {chest.id};";
			db.Query(query);
		}

		public static void UpdatePublic(InfChest chest)
		{
			string query = $"UPDATE InfChests3 SET Public = {(chest.isPublic ? 1 : 0)} WHERE ID = {chest.id};";
			db.Query(query);
		}

		public static void UpdateUsers(InfChest chest)
		{
			string query = $"UPDATE InfChests3 SET Users = '{chest.UsersToString()}' WHERE ID = {chest.id};";
			db.Query(query);
		}

		public static void UpdateGroups(InfChest chest)
		{
			string query = $"UPDATE InfChests3 SET Groups = '{chest.GroupsToString()}' WHERE ID = {chest.id};";
			db.Query(query);
		}

		public static void UpdateRefill(InfChest chest)
		{
			string query = $"UPDATE InfChests3 SET Refill = {chest.refill} WHERE ID = {chest.id};";
			db.Query(query);
		}

		public static int SearchChests(int itemID)
		{
			string query = $"SELECT * FROM InfChests3 WHERE Items LIKE '%~{itemID},%' AND WorldID = {Main.worldID};";
			int count = 0;
			using (var reader = db.QueryReader(query))
			{
				while (reader.Read())
					count++;
			}
			return count;
		}

		public static void DeleteChest(int id)
		{
			string query = $"DELETE FROM InfChests3 WHERE ID = {id};";
			db.Query(query);
		}

        public static List<Point> DeleteEmptyChests()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 40; i++)
            {
                sb.Append('~'); // Use Append(char) instead of Append(string) for a single character
                sb.Append("0,0,0");
            }
            sb.Append('~'); // Use Append(char) instead of Append(string) for a single character

            List<Point> points = new List<Point>();

            string query = $"SELECT * FROM InfChests3 WHERE Items = '{sb.ToString()}' AND WorldID = {Main.worldID} AND Refill = -1;";
            using (var reader = db.QueryReader(query))
            {
                while (reader.Read())
                {
                    Point point = new Point(reader.Get<int>("X"), reader.Get<int>("Y"));
                    points.Add(point);
                }
            }

            query = $"DELETE FROM InfChests3 WHERE Items = '{sb}' AND WorldID = {Main.worldID} AND Refill = -1;";
            db.Query(query);
            return points;
        }

        public static void DeleteAllChests()
		{
			string query = $"DELETE FROM InfChests3 WHERE WorldID = {Main.worldID};";
			db.Query(query);
		}

		public static int GetCount()
		{
			string query = "SELECT * FROM InfChests3 WHERE WorldID = " + Main.worldID + ";";
			using (var reader = db.QueryReader(query))
			{
				int count = 0;
				while (reader.Read())
				{
					count++;
				}
				return count;
			}
		}

		public static int TransferV1()
		{
			InfMain.lockChests = true;
			List<InfChest> chests = new List<InfChest>();

			if (TShock.Config.Settings.StorageType == "sqlite")
			{
				// We have to open a new db connection, as these tables are in different .sqlite files
				try
				{
                    using (var v1conn = new SqliteConnection($"Data Source={Path.Combine(TShock.SavePath, "chests.sqlite")}"))
                    {
                        string query = $"SELECT * FROM Chests WHERE WorldID = {Main.worldID};";
						using (var reader = v1conn.QueryReader(query))
						{
							while (reader.Read())
							{
								int x = reader.Get<int>("X");
								int y = reader.Get<int>("Y");
								string account = reader.Get<string>("Account");
								string rawitems = reader.Get<string>("Items");
								int refill = reader.Get<int>("RefillTime");
								UserAccount user = TShock.UserAccounts.GetUserAccountByName(account);
								InfChest chest = new InfChest(user == null ? -1 : user.ID, x, y, Main.worldID);
								if (refill > -1)
									chest.refill = refill;
								string[] items = rawitems.Split(',');
								for (int i = 0; i < 40; i++)
								{
									Item item = new Item();
									item.SetDefaults(int.Parse(items[3 * i]));
									item.stack = int.Parse(items[3 * i + 1]);
									item.prefix = byte.Parse(items[3 * i + 2]);
									chest.items[i] = item;
								}
								chests.Add(chest);
							}
						}
					}
				}
				catch (Exception ex)
				{
					TShock.Log.ConsoleError(ex.ToString());
				}
			}
			else
			{
				//Probably could just have this code once and switch db conn as needed, but #lazy
				string query = $"SELECT * FROM Chests WHERE WorldID = {Main.worldID};";
				using (var reader = db.QueryReader(query))
				{
					while (reader.Read())
					{
						int x = reader.Get<int>("X");
						int y = reader.Get<int>("Y");
						string account = reader.Get<string>("Account");
						string rawitems = reader.Get<string>("Items");
						int refill = reader.Get<int>("RefillTime");
						UserAccount user = TShock.UserAccounts.GetUserAccountByName(account);
						InfChest chest = new InfChest(user == null ? -1 : user.ID, x, y, Main.worldID);
						if (refill > -1)
							chest.refill = refill;
						string[] items = rawitems.Split(',');
						for (int i = 0; i < 40; i++)
						{
							Item item = new Item();
							item.SetDefaults(int.Parse(items[3 * i]));
							item.stack = int.Parse(items[3 * i + 1]);
							item.prefix = byte.Parse(items[3 * i + 2]);
							chest.items[i] = item;
						}
						chests.Add(chest);
					}
				}
			}

			foreach (InfChest chest in chests)
			{
				AddChest(chest);
			}
			InfMain.lockChests = false;
			return chests.Count;
		}

		public static int TransferV2()
		{
			InfMain.lockChests = true;
			List<InfChest> chests = new List<InfChest>();

			if (TShock.Config.Settings.StorageType == "sqlite")
			{
				// We have to open a new db connection, as these tables are in different .sqlite files
				try
				{
					using (var v2conn = new SqliteConnection($"Data Source={Path.Combine(TShock.SavePath, "chests.sqlite")}"))
                    {
						string query = $"SELECT * FROM InfChests WHERE WorldID = {Main.worldID};";
						using (var reader = v2conn.QueryReader(query))
						{
							while (reader.Read())
							{
								int id = reader.Get<int>("ID");
								int userid = reader.Get<int>("UserID");
								int x = reader.Get<int>("X");
								int y = reader.Get<int>("Y");
								bool ispublic = reader.Get<int>("Public") == 1 ? true : false;
								List<int> users = string.IsNullOrEmpty(reader.Get<string>("Users")) ? new List<int>() : reader.Get<string>("Users").Split(',').ToList().ConvertAll(p => int.Parse(p));
								List<string> groups = string.IsNullOrEmpty(reader.Get<string>("Groups")) ? new List<string>() : reader.Get<string>("Groups").Split(',').ToList();
								int refill = reader.Get<int>("Refill");
                                                                InfChest chest = new InfChest(userid, x, y, Main.worldID)
                                                                {
                                                                        // Constructor pre-populates the item slots
                                                                        id = id,
                                                                        groups = groups,
                                                                        isPublic = ispublic,
                                                                        refill = refill,
                                                                        users = users
                                                                };
								chests.Add(chest);

							}
						}

						foreach (InfChest chest in chests)
						{
							query = $"SELECT * FROM ChestItems WHERE ChestID = {chest.id};";
							using (var reader = v2conn.QueryReader(query))
							{
								while (reader.Read())
								{
									int slot = reader.Get<int>("Slot");
									int type = reader.Get<int>("Type");
									int stack = reader.Get<int>("Stack");
									int prefix = reader.Get<int>("Prefix");
									Item item = new Item();
									item.SetDefaults(type);
									item.stack = stack;
									item.prefix = (byte)prefix;
									chest.items[slot] = item;
								}
							}

							AddChest(chest);
						}
					}
				}
				catch (Exception ex)
				{
					TShock.Log.ConsoleError(ex.ToString());
				}
			}
			else
			{
				string query = $"SELECT * FROM InfChests WHERE WorldID = {Main.worldID};";
				using (var reader = db.QueryReader(query))
				{
					while (reader.Read())
					{
						int id = reader.Get<int>("ID");
						int userid = reader.Get<int>("UserID");
						int x = reader.Get<int>("X");
						int y = reader.Get<int>("Y");
						bool ispublic = reader.Get<int>("Public") == 1 ? true : false;
						List<int> users = string.IsNullOrEmpty(reader.Get<string>("Users")) ? new List<int>() : reader.Get<string>("Users").Split(',').ToList().ConvertAll(p => int.Parse(p));
						List<string> groups = string.IsNullOrEmpty(reader.Get<string>("Groups")) ? new List<string>() : reader.Get<string>("Groups").Split(',').ToList();
						int refill = reader.Get<int>("Refill");
                                                InfChest chest = new InfChest(userid, x, y, Main.worldID)
                                                {
                                                        // Constructor pre-populates the item slots
                                                        id = id,
                                                        groups = groups,
                                                        isPublic = ispublic,
                                                        refill = refill,
                                                        users = users
                                                };
						chests.Add(chest);

					}
				}

				foreach (InfChest chest in chests)
				{
					query = $"SELECT * FROM ChestItems WHERE ChestID = {chest.id};";
					using (var reader = db.QueryReader(query))
					{
						while (reader.Read())
						{
							int slot = reader.Get<int>("Slot");
							int type = reader.Get<int>("Type");
							int stack = reader.Get<int>("Stack");
							int prefix = reader.Get<int>("Prefix");
							Item item = new Item();
							item.SetDefaults(type);
							item.stack = stack;
							item.prefix = (byte)prefix;
							chest.items[slot] = item;
						}
					}

					AddChest(chest);
				}
			}

			InfMain.lockChests = false;

			return chests.Count;
		}
	}
}
