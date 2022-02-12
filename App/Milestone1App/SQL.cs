using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Documents;
using Npgsql;

namespace YelpApp
{
    public static class SQL
    {
        public static string buildConnectionString()
        {
            return "Host = localhost; Username = postgres; Database = milestone1db; password=wsu12";
        }

        public static bool executeQuery(string sqlstr, MainWindow form, Action<MainWindow, NpgsqlDataReader> myf)
        {
            using (var connection = new NpgsqlConnection(buildConnectionString()))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = sqlstr;
                    try
                    {
                        var reader = cmd.ExecuteReader();
                        if (!reader.HasRows) return false;
                        while (reader.Read())
                            myf(form, reader);
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        MessageBox.Show("SQL Error - " + ex.Message.ToString());
                    }
                    finally
                    { 
                        connection.Close();
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// Overload for an ORDER BY statement.
        /// </summary>
        /// <param name="column"> Column. </param>
        /// <param name="table"> Table. </param>
        /// <param name="orderby"> Orderby column. </param>
        /// <param name="conditions"> Conditions. </param>
        /// <returns> Sql query. </returns>
        public static string generateSelectQueryOrderBy(string column, string table, string orderby, params string[] conditions)
        {
            string query = generateSelectQuery(column, table, conditions);
            query += " ORDER BY " + orderby;
            return query;
        }

        /// <summary>
        /// Generates and SQL select query statement,
        /// based on column, table, and conditions.
        /// </summary>
        /// <param name="column"> column(s) to be selected. </param>
        /// <param name="table"> table(s) to select from. </param>
        /// <param name="conditions"> conditions, enter each condition string as its own parameter. </param>
        /// <returns> sql query statement. </returns>
        public static string generateSelectQuery(string column, string table, params string[] conditions)
        {
            string query = "SELECT " + column + " FROM " + table;
            query = addConditions(query, conditions);
            return query;
        }

        public static string generateInsertQuery(string table, params string[] columnvalues)
        {
            string query = "INSERT INTO " + table;
            List<string> columns = new List<string>();
            List<string> values = new List<string>();
            foreach (string columnvaluepair in columnvalues)
            {
                string[] arr = columnvaluepair.Split('|');
                columns.Add(arr[0]);
                values.Add(arr[1]);
            }
            query += "(";
            foreach (string column in columns)
            {
                query += column.TrimEnd(' ') + ",";
            }
            query = query.TrimEnd(',',' ');
            query += ") VALUES (";
            foreach (string value in values)
            {
                query += "'" + value.Trim(' ') + "',";
            }
            query = query.TrimEnd(',');
            query += ")";

            return query;
        }

        public static string generateUpdateQuery(string table, string column, string value, params string[] conditions)
        {
            string query = "UPDATE " + table + " SET " + column + " = " + value;
            query = addConditions(query, conditions);
            return query;
        }

        private static string addConditions(string query, params string[] conditions)
        {
            if (conditions.Length > 0)
            {
                query += " WHERE " + conditions[0];
                for (int i = 1; i < conditions.Length; i++)
                {
                    query += " AND " + conditions[i];
                }
            }
            return query;
        }
    }
}
