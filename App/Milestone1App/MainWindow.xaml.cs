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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Core;
using Microsoft.Maps.MapControl.WPF.Design;
using Npgsql;

namespace YelpApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string currentUserID;
        public Business currentBusiness;
        public string currentBID;

        public MainWindow()
        {
            InitializeComponent();
            Business.addState(this);
            Business.addColumns2BusinessGrid(this);
            User.addColumns2FriendGrid(this);
            Business.addColumnToRecentVisits(this);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   USER INFORMATION   //////////////////////////////////////////////////////////////////////////////////////////
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                userIDList.Items.Clear();
                string sqlStr = SQL.generateSelectQuery("userid", "\"user\"", "username = '" + Enter_Name.Text + "'");
                SQL.executeQuery(sqlStr, this, User.addUserIDs);
            }
        }

        private void userIDList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            userName.Clear();
            starsValue.Clear();
            fansValue.Clear();
            yelping_Since.Clear();
            funnyValue.Clear();
            coolValue.Clear();
            usefulValue.Clear();
            tipCount.Clear();
            totalTipLikes.Clear();
            friendGrid.Items.Clear();
            tipFriendGrid.Items.Clear();
            latitudeVal.Clear();
            longitudeVal.Clear();

            if (userIDList.SelectedItem != null)
            {
                string userstr = SQL.generateSelectQuery("*","\"user\"", "userid = '" + userIDList.SelectedItem.ToString() + "'");
                SQL.executeQuery(userstr, this, User.addUserInfo);
                currentUserID = userIDList.SelectedItem.ToString();

                string userfriendstr = SQL.generateSelectQuery("username, usertotallikes, useraveragestars, yelpingsince", "\"user\", friends", "friends.userid = '" + currentUserID + "'", 
                    "friends.friendid = \"user\".userid");
                SQL.executeQuery(userfriendstr, this, User.addFriendInfo);

                string friendtipstr = SQL.generateSelectQueryOrderBy("username, businessname, city, tiptext, tipdate", "\"user\", friends, tip, business",
                    "tipdate DESC", "tipdate = (" + SQL.generateSelectQuery("MAX(tipdate)", "tip", "\"user\".userid = tip.userid")  + ")","friends.userid = '" + currentUserID + "'", "\"user\".userid = friends.friendid", "tip.userid = friends.friendid", 
                    "business.businessid = tip.businessid");
                SQL.executeQuery(friendtipstr, this, User.addFriendTipsInfo);
            }
        }

        private void MapButton_Click(object sender, RoutedEventArgs e)
        {
            BingMap map = new BingMap(this);
            map.Show();
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   BUSINESS SEARCH    //////////////////////////////////////////////////////////////////////////////////////////
        private void stateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cityList.Items.Clear();
            if (stateList.SelectedIndex > -1)
            {
                string sqlStr = SQL.generateSelectQueryOrderBy("distinct city", "business", "city",
                    "state = '" + stateList.SelectedItem.ToString() + "'");
                SQL.executeQuery(sqlStr, this, Business.addCity);
            }
        }

        private void cityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            zipcodeList.Items.Clear();
            if (cityList.SelectedIndex > -1)
            {
                string sqlStr = SQL.generateSelectQuery("distinct postalcode", "business",
                    "state = '" + stateList.SelectedItem.ToString() + "'", "city = '" + cityList.SelectedItem.ToString() + "'");
                SQL.executeQuery(sqlStr, this, Business.addZip);
            }
        }

        private void zipcodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BusinessCategoryList.Items.Clear();
            if (zipcodeList.SelectedIndex > -1)
            {
                string sqlStr = SQL.generateSelectQueryOrderBy("distinct categoryname", "business, category", "categoryname",
                    "state = '" + stateList.SelectedItem.ToString() + "'", "city = '" + cityList.SelectedItem.ToString() + "'",
                    "postalcode = '" + zipcodeList.SelectedItem.ToString() + "'", "category.businessid =  business.businessid");
                SQL.executeQuery(sqlStr, this, Business.addBusCategory);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (BusinessCategoryList.SelectedIndex > -1)
            {
                currentCategories.Items.Remove(currentCategories.SelectedItem);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (BusinessCategoryList.SelectedIndex > -1)
            {
                currentCategories.Items.Add(BusinessCategoryList.SelectedItem);
            }
        }

        private void searchBusinessesButton_Click(object sender, RoutedEventArgs e)
        {
            businessGrid.Items.Clear();
            string sqlStr;
            if (BusinessCategoryList.Items.Count > 0)
            {
                if (currentCategories.Items.Count > 0)
                {
                    sqlStr = SQL.generateSelectQueryOrderBy("*", "business", "city",
                        "state = '" + stateList.SelectedItem.ToString() + "'", "city = '" + cityList.SelectedItem.ToString() + "'", 
                        "postalcode = '" + zipcodeList.SelectedItem.ToString() + "'",
                        "ARRAY( " + SQL.generateSelectQuery("categoryname", "category", "business.businessid = category.businessid") +
                        ")::text[] @> ARRAY['" + string.Join(",", currentCategories.Items.Cast<string>().ToArray()).Replace(",", "','") + "']");
                }
                else
                {
                    sqlStr = SQL.generateSelectQueryOrderBy("*", "business", "city",
                        "state = '" + stateList.SelectedItem.ToString() + "'", "city = '" + cityList.SelectedItem.ToString() + "'",
                        "postalcode = '" + zipcodeList.SelectedItem.ToString() + "'");
                }
                SQL.executeQuery(sqlStr, this, Business.addGridRow);
                numberOfBusinesses.Text = businessGrid.Items.Count.ToString();
                Business.PopulateDistances(this);
            }
        }

        private void businessGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (businessGrid.SelectedIndex > -1)
            {
                Business B = businessGrid.Items[businessGrid.SelectedIndex] as Business;
                //B.bid = businessGrid.Items
                if ((B.bid != null) && (B.bid.ToString().CompareTo("") != 0))
                {
                    B.loadBusinessDetails();
                    B.loadAttributeDetails();
                    currentBusiness = B;
                }
            }
        }

        private void showCheckinsButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentBusiness != null)
            {
                Chart chartWindow = new Chart(this);
                string sqlstr = SQL.generateSelectQueryOrderBy("month, COUNT(checkin)", "checkin", "month", "businessid = '" + currentBusiness.bid + "' GROUP BY month");
                SQL.executeQuery(sqlstr, this, chartWindow.PopulateCheckins);
                chartWindow.Show();
            }
        }

        private void showtipsButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentBusiness != null)
            {
                Tips tipwindow = new Tips(this);
                string sqlstr = SQL.generateSelectQuery("tipdate, username, tiplikes, tiptext, tip.userid", "tip, \"user\"",
                    "tip.businessid = '" + currentBusiness.bid + "'", "\"user\".userid = tip.userid");
                SQL.executeQuery(sqlstr, this, tipwindow.Populate);

                if (!string.IsNullOrEmpty(currentUserID))
                {
                    string friendtipstr = SQL.generateSelectQuery("username, tipdate, tiptext, tip.userid", "tip, friends, \"user\"",
                        "businessid = '" + currentBusiness.bid + "'", "friends.userid = '" + currentUserID + "'",
                        "tip.userid = friends.friendid", "\"user\".userid = tip.userid", "\"user\".userid = friends.friendid");
                    SQL.executeQuery(friendtipstr, this, tipwindow.PopulateFriends);
                }
                tipwindow.Show();
            }
        }

        private void tipFriendGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void nameSort_Selected(object sender, RoutedEventArgs e)
        {
            this.businessGrid.Items.SortDescriptions.Clear();
            var col = businessGrid.Columns[0];
            this.businessGrid.Items.SortDescriptions.Add(new SortDescription(col.SortMemberPath, ListSortDirection.Ascending));
        }

        private void rateSort_Selected(object sender, RoutedEventArgs e)
        {
            this.businessGrid.Items.SortDescriptions.Clear();
            var col = businessGrid.Columns[5];
            this.businessGrid.Items.SortDescriptions.Add(new SortDescription(col.SortMemberPath, ListSortDirection.Descending));
        }

        private void tipsSort_Selected(object sender, RoutedEventArgs e)
        {
            this.businessGrid.Items.SortDescriptions.Clear();
            var col = businessGrid.Columns[6];
            this.businessGrid.Items.SortDescriptions.Add(new SortDescription(col.SortMemberPath, ListSortDirection.Descending));
        }

        private void checkinsSort_Selected(object sender, RoutedEventArgs e)
        {
            this.businessGrid.Items.SortDescriptions.Clear();
            var col = businessGrid.Columns[7];
            this.businessGrid.Items.SortDescriptions.Add(new SortDescription(col.SortMemberPath, ListSortDirection.Descending));
        }

        private void nearSort_Selected(object sender, RoutedEventArgs e)
        {
            this.businessGrid.Items.SortDescriptions.Clear();
            var col = businessGrid.Columns[4];
            this.businessGrid.Items.SortDescriptions.Add(new SortDescription(col.SortMemberPath, ListSortDirection.Descending));
        }

        private void Edit_Button_Click(object sender, RoutedEventArgs e)
        {

            latitudeVal.IsEnabled = true;
            longitudeVal.IsEnabled = true;
        }

        private void updateLocation_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(latitudeVal.Text) && 
                double.TryParse(latitudeVal.Text, out double result1) && 
                double.TryParse(longitudeVal.Text, out double result2))
            {
                string latitudeupdate = SQL.generateUpdateQuery("\"user\"", "userlatitude", latitudeVal.Text, "userid = '" + currentUserID + "'");
                SQL.executeQuery(latitudeupdate, this, null);
                string longitudeupdate = SQL.generateUpdateQuery("\"user\"", "userlongitude", longitudeVal.Text, "userid = '" + currentUserID + "'");
                SQL.executeQuery(longitudeupdate, this, null);
                latitudeVal.IsEnabled = false;
                longitudeVal.IsEnabled = false;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   BUSINESS VIEW    ////////////////////////////////////////////////////////////////////////////////////////////
        
        private void businessName_keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                businessIDList.Items.Clear();
                string sqlStr = SQL.generateSelectQuery("businessid", "\"business\"", "businessname = '" + Enter_BusinessName.Text.Replace('\'', '`') + "'");
                SQL.executeQuery(sqlStr, this, Business.addBID);
            }
        }

        private void businessIDList_SelctionChanged(object sender, SelectionChangedEventArgs e)
        {
            businessNameBox.Clear();
            busStarsBox.Clear();
            busTipsBox.Clear();
            busCheckInsBox.Clear();
            busHoursBox.Clear();
            busAddressBox.Clear();
            busCityBox.Clear();
            busStateBox.Clear();
            busPostalCodeBox.Clear();
            latestTipsGrid.Items.Clear();
            recentVisitorsGrid.Items.Clear();

            if (businessIDList.SelectedItem != null)
            {
                string busStr = SQL.generateSelectQuery("*", "\"business\"", "businessid = '" + businessIDList.SelectedItem.ToString() + "'");
                SQL.executeQuery(busStr, this, Business.addBusinessInfo);
                currentBID = businessIDList.SelectedItem.ToString();

                string month;
                if (DateTime.Now.Month < 10)
                {
                    month = "0" + DateTime.Now.Month;
                }
                else
                {
                    month = "" + DateTime.Now.Month;
                }

                string recentCheckin = SQL.generateSelectQueryOrderBy("year, month, day, time", "checkin", "year", "checkin.businessid = '" + currentBID + "'", "checkin.month = '" + month + "'");
                SQL.executeQuery(recentCheckin, this, Business.AddRecentVisits);

                string latestTips = SQL.generateSelectQuery("username, tiptext, tiplikes, tipdate", "\"user\", tip", "tip.userid = \"user\".userid", "tip.businessid = '"
                    + currentBID + "'");
                SQL.executeQuery(latestTips, this, Business.addBusinessTipsInfo);
            }
        }
    }
}