using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Milestone1App;
using Npgsql;

namespace YelpApp
{
    public class Business
    {
        MainWindow form = Application.Current.Windows[0] as MainWindow;

        public string bid { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string distance { get; set; }
        public string stars { get; set; }
        public string tips { get; set; }
        public string checkins { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string postalcode { get; set; }
        public bool isopen { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }


        public static void addColumns2BusinessGrid(MainWindow form)
        {
            form.businessGrid.ColumnHeaderHeight = 50;

            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "BusinessName";
            col1.Binding = new Binding("name");
            col1.Width = 130;
            form.businessGrid.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("address");
            col2.Header = "Address";
            col2.Width = 150;
            form.businessGrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("city");
            col3.Header = "City";
            col3.Width = 90;
            form.businessGrid.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Binding = new Binding("state");
            col4.Header = "State";
            col4.Width = 45;
            form.businessGrid.Columns.Add(col4);

            DataGridTextColumn col5 = new DataGridTextColumn();
            col5.Binding = new Binding("distance");
            col5.Header = "Distance\n(miles)";
            col5.Width = 70;
            form.businessGrid.Columns.Add(col5);

            DataGridTextColumn col6 = new DataGridTextColumn();
            col6.Binding = new Binding("stars");
            col6.Header = "Stars";
            col6.Width = 60;
            form.businessGrid.Columns.Add(col6);

            DataGridTextColumn col7 = new DataGridTextColumn();
            col7.Binding = new Binding("tips");
            col7.Header = "# of Tips";
            col7.Width = 65;
            form.businessGrid.Columns.Add(col7);

            DataGridTextColumn col8 = new DataGridTextColumn();
            col8.Binding = new Binding("checkins");
            col8.Header = "Total\nCheckins";
            col8.Width = 70;
            form.businessGrid.Columns.Add(col8);
        }

        public static void addState(MainWindow form)
        {
            string stateString = SQL.generateSelectQueryOrderBy("distinct state", "business", "state");
            SQL.executeQuery(stateString, form, addStateHelper);
        }

        private static void addStateHelper(MainWindow form, NpgsqlDataReader R)
        {
            form.stateList.Items.Add(R.GetString(0));
        }

        public static void addCity(MainWindow form, NpgsqlDataReader R)
        {
            form.cityList.Items.Add(R.GetString(0));
        }

        public static void addZip(MainWindow form, NpgsqlDataReader R)
        {
            form.zipcodeList.Items.Add(R.GetString(0));
        }

        public static void addBusCategory(MainWindow form, NpgsqlDataReader R)
        {
            form.BusinessCategoryList.Items.Add(R.GetString(0));
        }

        public static void addGridRow(MainWindow form, NpgsqlDataReader R)
        {
            Business b = new Business()
            {
                bid = R.GetString(0),
                isopen = R.GetBoolean(2),
                stars = R.GetValue(3).ToString(),
                tips = R.GetValue(4).ToString(),
                checkins = R.GetValue(5).ToString(),
                name = R.GetString(6),
                city = R.GetString(7),
                address = R.GetString(8),
                state = R.GetString(9),
                postalcode = R.GetString(10),
                latitude = R.GetDouble(11),
                longitude = R.GetDouble(12)
            };

            form.businessGrid.Items.Add(b);
            if (!string.IsNullOrEmpty(form.latitudeVal.Text) && !string.IsNullOrEmpty(form.longitudeVal.Text))
            {
                InsertDistance(form, b.bid, form.latitudeVal.Text, form.longitudeVal.Text, b.latitude.ToString(), b.longitude.ToString());
            }
        }

        public void loadBusinessDetails()
        {
            form.currentBusinessTextBox.Text = name;
            form.currentAddressTextBox.Text = address;
            String currentday = DateTime.Now.DayOfWeek.ToString();
            string sqlstr = SQL.generateSelectQuery("day, timeframe", "hours", "day = '" + currentday + "'", "businessid = '" + bid + "'");
            if (!SQL.executeQuery(sqlstr, form, setBusinessHours))
            {
                setBusinessHours();
            }
        }

        public void loadAttributeDetails()
        {
            string sqlstr = "SELECT distinct attributename, categoryname FROM attribute, category WHERE attribute.businessid = '" + bid + "' AND category.businessid = '" + bid + "'";
            SQL.executeQuery(sqlstr, form, setAttributeTable);
        }

        private void setAttributeTable(MainWindow form, NpgsqlDataReader R)
        {
            form.categoryListBox.Items.Add(R.GetString(0));
            //form.categoryListBox.Items.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));
        }

        private void setBusinessHours()
        {
            form.todayOpensTextBox.Text = "Closed";
        }

        private void setBusinessHours(MainWindow form, NpgsqlDataReader R)
        {
            string[] times = R.GetString(1).Split('-');
            for (int i = 0; i < times.Length; i++)
            {
                if (times[i].EndsWith(":0"))
                {
                    times[i] = times[i] + "0";
                }
            }
            form.todayOpensTextBox.Text = R.GetString(0) + ": Open from "  + times[0] + " until " + times[1];
        }

        public static void InsertDistance(MainWindow form, string bid, string ulat, string ulng, string blat, string blng)
        {
            string insertstring = SQL.generateInsertQuery("distance", "userid |" + form.userIDList.SelectedItem, "businessid |" + bid,
                "ulat |" + ulat, "ulong |" + ulng, "blat |" + blat, "blong |" + blng);
            SQL.executeQuery(insertstring, form, null);
        }

        public static void PopulateDistances(MainWindow form)
        {
            if (!string.IsNullOrEmpty(form.latitudeVal.Text) && !string.IsNullOrEmpty(form.longitudeVal.Text))
            {
                string ulat = form.latitudeVal.Text;
                string ulng = form.longitudeVal.Text;
                foreach (Business b in form.businessGrid.Items)
                {
                    string distancestring = SQL.generateSelectQuery("blat, blong, getdistance(" + ulat + "," + ulng + "," + b.latitude + "," + b.longitude + ")", "distance",
                        "blat= '" + b.latitude + "'", "blong = '" + b.longitude + "'");
                    SQL.executeQuery(distancestring, form, AddDistance);
                }
            }
        }

        private static void AddDistance(MainWindow form, NpgsqlDataReader R)
        {
            foreach (Business b in form.businessGrid.Items)
            {
                if (b.latitude == R.GetDouble(0) && b.longitude == R.GetDouble(1))
                {
                    b.distance = "" + R.GetDouble(2);
                }
            }
        }

        public static void addBID(MainWindow form, NpgsqlDataReader R)
        {
            form.businessIDList.Items.Add(R.GetString(0));
        }

        public static void addBusinessInfo(MainWindow form, NpgsqlDataReader R)
        {
            string isOpen;

            if (R.GetBoolean(2) == true)
            {
                isOpen = "Open";
            }
            else
            {
                isOpen = "Closed";
            }
            
            form.businessNameBox.Text = R.GetString(6);
            form.busStarsBox.Text = R.GetValue(3).ToString();
            form.busTipsBox.Text = R.GetValue(4).ToString();
            form.busCheckInsBox.Text = R.GetValue(5).ToString();
            form.busHoursBox.Text = isOpen; 
            form.busAddressBox.Text = R.GetString(8);
            form.busCityBox.Text = R.GetString(7);
            form.busStateBox.Text = R.GetString(9);
            form.busPostalCodeBox.Text = R.GetString(10);
        }

        public static void addBusinessTipsInfo(MainWindow form, NpgsqlDataReader R)
        {
            form.latestTipsGrid.Items.Add(new Tip() 
            {
                UserName = R.GetString(0),
                tiplikes = R.GetInt32(2).ToString(),
                tiptext = R.GetString(1),
                tipdate = R.GetString(3)
            });
        }

        public static void AddRecentVisits(MainWindow form, NpgsqlDataReader R)
        {
            form.recentVisitorsGrid.Items.Add(new CheckIn()
            {
                year = R.GetString(0),
                month = R.GetString(1),
                day = R.GetString(2),
                time = R.GetString(3)
            });
            ;

        }

        public static void addColumnToRecentVisits(MainWindow form)
        {
            form.recentVisitorsGrid.ColumnHeaderHeight = 50;

            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "Year";
            col1.Binding = new Binding("year");
            col1.Width = 100;
            form.recentVisitorsGrid.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Binding = new Binding("month");
            col2.Header = "Month";
            col2.Width = 100;
            form.recentVisitorsGrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Binding = new Binding("day");
            col3.Header = "Day";
            col3.Width = 100;
            form.recentVisitorsGrid.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Binding = new Binding("time");
            col4.Header = "Time";
            col4.Width = 100;
            form.recentVisitorsGrid.Columns.Add(col4);

            
        }
    }
}
