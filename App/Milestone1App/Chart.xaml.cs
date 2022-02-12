using System;
using System.Collections.Generic;
using System.Windows;
using Npgsql;

namespace YelpApp
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class Chart : Window
    {
        MainWindow form;
        List<KeyValuePair<string, int>> myChartData;

        public Chart(MainWindow form)
        {
            InitializeComponent();
            myChartData = new List<KeyValuePair<string, int>>();
            this.form = form;
        }

        public void PopulateCheckins(MainWindow form, NpgsqlDataReader R)
        {
            List<string> months = new List<string>(){ "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            for (int i = 0; i < myChartData.Count; i++)
            {
                KeyValuePair<string, int> pair = myChartData[i];
                if (pair.Key == months[int.Parse(R.GetString(0)) - 1])
                {
                    myChartData[i] = new KeyValuePair<string, int>(pair.Key, pair.Value + R.GetInt32(1));
                    myChart.DataContext = myChartData;
                    return;
                }
            }
            myChartData.Add(new KeyValuePair<string, int>(months[int.Parse(R.GetString(0)) - 1], R.GetInt32(1)));
            myChart.DataContext = myChartData;
        }

        private void addCheckInButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            string sqlstring = SQL.generateInsertQuery("checkin", "businessid | " + form.currentBusiness.bid, "year | " + dateTime.Year.ToString(),
                "month | " + dateTime.Month.ToString(), "day | " + dateTime.Day.ToString(), "time | " + dateTime.TimeOfDay.ToString().Substring(0,8));
            SQL.executeQuery(sqlstring, null, null);
            checkinUpdate();
        }

        private void checkinUpdate()
        {
            this.Close();
            Chart chart = new Chart(form);
            string sqlstr = SQL.generateSelectQueryOrderBy("month, COUNT(checkin)", "checkin", "month", "businessid = '" + form.currentBusiness.bid + "' GROUP BY month");
            SQL.executeQuery(sqlstr, form, chart.PopulateCheckins);
            chart.Show();

        }
    }
}
