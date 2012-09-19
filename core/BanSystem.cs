﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Net;
using iconnect;

namespace core
{
    class BanSystem
    {
        private static List<Ban> list;
        private static String DataPath { get; set; }

        public static bool IsBanned(IClient client)
        {
            return list.Find(x => x.ExternalIP.Equals(client.ExternalIP) ||
                x.Guid.Equals(client.Guid)) != null;
        }

        public static void Eval(Action<IBan> action)
        {
            for (int i = (list.Count - 1); i > -1; i--)
                action(list[i]);
        }

        public static void AddBan(IClient client)
        {
            Ban ban = new Ban
            {
                Name = client.Name,
                Version = client.Version,
                Guid = client.Guid,
                ExternalIP = client.ExternalIP,
                LocalIP = client.LocalIP,
                Port = client.DataPort,
                Ident = NextIdent
            };

            list.Add(ban);

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=\"" + DataPath + "\""))
            {
                connection.Open();

                String query = @"insert into bans (name, version, guid, externalip, localip, port, ident) 
                                 values (@name, @version, @guid, @externalip, @localip, @port, @ident)";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.Add(new SQLiteParameter("@name", ban.Name));
                    command.Parameters.Add(new SQLiteParameter("@version", ban.Version));
                    command.Parameters.Add(new SQLiteParameter("@guid", ban.Guid.ToString()));
                    command.Parameters.Add(new SQLiteParameter("@externalip", ban.ExternalIP.ToString()));
                    command.Parameters.Add(new SQLiteParameter("@localip", ban.LocalIP.ToString()));
                    command.Parameters.Add(new SQLiteParameter("@port", (int)ban.Port));
                    command.Parameters.Add(new SQLiteParameter("@ident", (int)ban.Ident));
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void RemoveBan(ushort ident)
        {
            Ban ban = list.Find(x => x.Ident == ident);

            if (ban != null)
            {
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=\"" + DataPath + "\""))
                {
                    connection.Open();

                    String query = @"delete from bans
                                     where ident=@ident
                                     limit 1";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.Add(new SQLiteParameter("@ident", (int)ban.Ident));
                        command.ExecuteNonQuery();
                    }
                }

                list.RemoveAll(x => x.Ident == ident);
            }
        }

        public static void LoadBans()
        {
            DataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
               "\\sb0t\\" + AppDomain.CurrentDomain.FriendlyName + "\\Dat";

            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);

            DataPath += "\\banned.dat";

            if (!File.Exists(DataPath))
                CreateDatabase();

            list = new List<Ban>();

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=\"" + DataPath + "\""))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand("select * from bans", connection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                    while (reader.Read())
                        list.Add(new Ban
                        {
                            Name = (String)reader["name"],
                            Version = (String)reader["version"],
                            Guid = new Guid((String)reader["guid"]),
                            ExternalIP = IPAddress.Parse((String)reader["externalip"]),
                            LocalIP = IPAddress.Parse((String)reader["localip"]),
                            Port = (ushort)(int)reader["port"],
                            Ident = (ushort)(int)reader["ident"]
                        });
            }
        }

        private static void CreateDatabase()
        {
            SQLiteConnection.CreateFile(DataPath);

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=\"" + DataPath + "\""))
            {
                connection.Open();

                String query = @"create table bans
                                 (
                                     name text not null,
                                     version text not null,
                                     guid text not null,
                                     externalip text not null,
                                     localip text not null,
                                     port int not null,
                                     ident int not null
                                 )";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    command.ExecuteNonQuery();
            }
        }

        private static ushort NextIdent
        {
            get
            {
                ushort result = 0;

                for (ushort u = 0; u < ushort.MaxValue; u++)
                {
                    result = u;
                    int index = list.FindIndex(x => x.Ident == u);

                    if (index == -1)
                        break;
                }

                return result;
            }
        }
    }
}
