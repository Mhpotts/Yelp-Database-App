using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YelpApp
{
    /// <summary>
    /// Interaction logic for Tips.xaml
    /// </summary>
    public partial class Tips : Window
    {
        MainWindow form;
        public Tips(MainWindow form)
        {
            InitializeComponent();
            addColumns2BusinessTipsGrid();
            addColumns2ReviewedGrid();
            this.form = form;
        }

        public void addColumns2BusinessTipsGrid()
        {
            businessTipsTable.ColumnHeaderHeight = 25;

            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "Date";
            col1.Binding = new Binding("tipdate");
            col1.Width = 120;
            col1.SortDirection = ListSortDirection.Descending;
            businessTipsTable.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = "User Name";
            col2.Binding = new Binding("UserName");
            col2.Width = 70;
            businessTipsTable.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Header = "Likes";
            col3.Binding = new Binding("tiplikes");
            col3.Width = 35;
            businessTipsTable.Columns.Add(col3);

            DataGridTextColumn col4 = new DataGridTextColumn();
            col4.Header = "Text";
            col4.Binding = new Binding("tiptext");
            col4.Width = 565;
            businessTipsTable.Columns.Add(col4);
        }

        public void addColumns2ReviewedGrid()
        {
            friendsTipsTable.ColumnHeaderHeight = 25;

            DataGridTextColumn col1 = new DataGridTextColumn();
            col1.Header = "User Name";
            col1.Binding = new Binding("UserName");
            col1.Width = 70;
            col1.SortDirection = ListSortDirection.Ascending;
            friendsTipsTable.Columns.Add(col1);

            DataGridTextColumn col2 = new DataGridTextColumn();
            col2.Header = "Date";
            col2.Binding = new Binding("tipdate");
            col2.Width = 120;
            friendsTipsTable.Columns.Add(col2);

            DataGridTextColumn col3 = new DataGridTextColumn();
            col3.Header = "Text";
            col3.Binding = new Binding("tiptext");
            col3.Width = 600;
            friendsTipsTable.Columns.Add(col3);
        }

        public void Populate(MainWindow form, NpgsqlDataReader R)
        {
            this.businessTipsTable.Items.Add(new Tip()
            {
                tipdate = R.GetString(0),
                UserName = R.GetString(1),
                tiplikes = R.GetValue(2).ToString(),
                tiptext = R.GetString(3),
                userid = R.GetString(4)
            });
            this.businessTipsTable.Items.SortDescriptions.Clear();
            var col = businessTipsTable.Columns[0];
            this.businessTipsTable.Items.SortDescriptions.Add(new SortDescription(col.SortMemberPath, ListSortDirection.Descending));
        }

        public void PopulateFriends(MainWindow form, NpgsqlDataReader R)
        {
            this.friendsTipsTable.Items.Add(new Tip()
            {
                UserName = R.GetString(0),
                tipdate = R.GetString(1),
                tiptext = R.GetString(2),
                userid = R.GetString(3)
            });
            this.businessTipsTable.Items.SortDescriptions.Clear();
            var col = businessTipsTable.Columns[0];
            this.businessTipsTable.Items.SortDescriptions.Add(new SortDescription(col.SortMemberPath, ListSortDirection.Descending));
        }

        private void tipButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(form.currentUserID) && form.currentBusiness != null)
            {
                DateTime current = DateTime.Now;
                string sqlstring = SQL.generateInsertQuery("tip", "userid | " + form.currentUserID, "businessid | " + form.currentBusiness.bid,
                    "tipdate | " + current.ToString(), "tiptext | " + newTipText.Text.Replace("\'", "`"), "tiplikes | 0");
                SQL.executeQuery(sqlstring, form, null);
                tipsUpdate();
            }
        }

        private void likeButton_Click(object sender, RoutedEventArgs e)
        {
            string sqlstring;
            if (friendsTipsTable.SelectedItem != null)
            {
                Tip tip = (Tip)friendsTipsTable.SelectedItem;
                sqlstring = SQL.generateUpdateQuery("tip", "tiplikes", "tiplikes + 1",
                    "userid = '" + form.currentUserID + "'", "businessid = '" + form.currentBusiness.bid + "'", "tipdate = '" + tip.tipdate + "'");
            } else if (businessTipsTable.SelectedItem != null)
            {
                Tip tip = (Tip)businessTipsTable.SelectedItem;
                sqlstring = "UPDATE tip SET tiplikes = tiplikes + 1 WHERE userid = '" +
                    tip.userid + "' AND businessid = '" + form.currentBusiness.bid + "' AND tipdate = '" + tip.tipdate + "'";
            } else return;
            SQL.executeQuery(sqlstring, form, null);
            tipsUpdate();
        }

        public void tipsUpdate()
        {
            friendsTipsTable.Items.Clear();
            businessTipsTable.Items.Clear();

            string sqlstr = SQL.generateSelectQuery("tipdate, username, tiplikes, tiptext, tip.userid", "tip, \"user\"",
                "tip.businessid = '" + form.currentBusiness.bid + "'", "\"user\".userid = tip.userid");
            SQL.executeQuery(sqlstr, this.form, this.Populate);

            if (!string.IsNullOrEmpty(form.currentUserID))
            {
                string friendtipstr = SQL.generateSelectQuery("username, tipdate, tiptext, tip.userid", "tip, friends, \"user\"",
                    "businessid = '" + form.currentBusiness.bid + "'", "friends.userid = '" + form.currentUserID + "'",
                    "tip.userid = friends.friendid", "\"user\".userid = tip.userid", "\"user\".userid = friends.friendid");
                SQL.executeQuery(friendtipstr, this.form, this.PopulateFriends);
            }
        }
    }
}
