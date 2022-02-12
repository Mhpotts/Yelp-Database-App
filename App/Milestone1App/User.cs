using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Npgsql;

namespace YelpApp
{
    public class User
    {
        MainWindow form = Application.Current.Windows[0] as MainWindow;
        public string UserID { get; set; }
        public double AverageStars { get; set; }
        public int Cool { get; set; }
        public int Funny { get; set; }
        public int Useful { get; set; }
        public  string YelpingSince { get; set; }
        public string UserName { get; set; }
        public int TipCount { get; set; }
        public int TotalLikes { get; set; }
        public int UserLatitude { get; set; }
        public int UserLongitude { get; set; }

        public static  void addColumns2FriendGrid(MainWindow form)
        {
            form.friendGrid.ColumnHeaderHeight = 25;

            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "Name";
            col1.Binding = new Binding("UserName");
            col1.Width = 100;
            form.friendGrid.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = "TotalLikes";
            col2.Binding = new Binding("TotalLikes");
            col2.Width = 100;
            form.friendGrid.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Header = "Avg Stars";
            col3.Binding = new Binding("AverageStars");
            col3.Width = 100;
            form.friendGrid.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = "Yelping Since";
            col4.Binding = new Binding("YelpingSince");
            col4.Width = 165;
            form.friendGrid.Columns.Add(col4);
        }

        internal static void addFriendTipsInfo(MainWindow form, NpgsqlDataReader R)
        {
            form.tipFriendGrid.Items.Add(new Tip()
            {
                UserName = R.GetString(0),
                name = R.GetString(1),
                city = R.GetString(2),
                tiptext = R.GetString(3),
                tipdate = R.GetString(4)
            });
        }

        internal static void addFriendInfo(MainWindow form, NpgsqlDataReader R)
        {
            form.friendGrid.Items.Add(new User()
            {
                UserName = R.GetString(0),
                TotalLikes = R.GetInt32(1),
                AverageStars = R.GetDouble(2),
                YelpingSince = R.GetString(3)
            });
        }

        public static void addUserIDs(MainWindow form, NpgsqlDataReader R)
        {
            form.userIDList.Items.Add(R.GetString(0));
        }

        public static void addUserInfo(MainWindow form, NpgsqlDataReader R)
        {
            form.userName.Text = R.GetString(1);
            form.starsValue.Text = R.GetValue(2).ToString();
            form.fansValue.Text = R.GetValue(4).ToString();
            form.yelping_Since.Text = R.GetString(8);
            form.funnyValue.Text = R.GetValue(6).ToString();
            form.coolValue.Text = R.GetValue(7).ToString();
            form.usefulValue.Text = R.GetValue(5).ToString();
            form.totalTipLikes.Text = R.GetValue(11).ToString();
            form.tipCount.Text = R.GetValue(12).ToString();
            form.latitudeVal.Text = R.GetValue(10).ToString();
            form.longitudeVal.Text = R.GetValue(9).ToString();
        }

        public void AddTip()
        {

        }

        public void AddCheckIn()
        {

        }
        public void UpdateTipCount()
        {

        }

        public void UpdateTotalLikes()
        {

        }
    }
}
