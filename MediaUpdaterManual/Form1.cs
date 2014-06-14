using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;


namespace MediaUpdaterManual
{
    public partial class Form1 : Form
    {

        private string localdbName = "MediaServer";
        private string localdbUser = "webuser";
        private string localdbPW = "f1shs0up";
        private string localdbHost = "192.168.1.6";
        public Form1()
        {
            InitializeComponent();
        }

        /* DB Cols 14/6/14
         * [movieID]
         * [imdbID]
         * [imdbTitle]
         * [imdbYear]
         * [imdbGenre]
         * [imdbRating]
         * [imdbRuntime]
         * [imdbPlot]
         * [moviedbImage]
         * [moviedbID]
         * [fileLocation]
         * [watched]
         * */
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void log(String str, bool newLine = true)
        {
            if (output.InvokeRequired)
            {
                MethodInvoker invoker = () => log(str, newLine);
                Invoke(invoker);
            }
            else
            {
                if (newLine)
                {
                    output.AppendText(str + "\r\n");
                }
                else
                {
                    output.AppendText(str);
                }
            }


        }

        private void goToIMDB(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.imdb.com");
        }

        private void goToMovieDB(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://api.themoviedb.org/3/find/xxxxxx?external_source=imdb_id&api_key=4bccdaf9961a8734cc9bd742bff4b862");
        }

        /******************* INSERT TAB CODE START *******************/

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(insertToDB); //Run the work in a new thread to stop the form getting blocked
            t.Start();
        }

        private void insertToDB()
        {
            if (!confirmCheck.Checked)
            {
                log("Confirmation not ticked!");
                return;
            }

            log("Confirmation ticked, proceeding...");
            string query = "INSERT INTO Movies ([imdbID], [imdbTitle], [imdbYear], [imdbGenre], [imdbRating], " +
                    "[imdbRuntime], [imdbPlot], [moviedbImage], [moviedbID], [fileLocation], [watched]) VALUES (@iID, @iTitle, @iYear, @iGenre, @iRating, @iRunTime, @iPlot, @mdbImage, @mdbID, @fileLoc, @watched);";


            SqlCommand sqlquery = new SqlCommand(query); //Insert the query params into the query. This protects against injection attack.
            sqlquery.Parameters.AddWithValue("@iID", textBox1.Text);
            sqlquery.Parameters.AddWithValue("@iTitle", textBox2.Text);
            sqlquery.Parameters.AddWithValue("@iYear", textBox3.Text);
            sqlquery.Parameters.AddWithValue("@iGenre", textBox4.Text);
            sqlquery.Parameters.AddWithValue("@iRating", textBox5.Text);
            sqlquery.Parameters.AddWithValue("@iRunTime", textBox6.Text);
            sqlquery.Parameters.AddWithValue("@iPlot", textBox7.Text);
            sqlquery.Parameters.AddWithValue("@mdbImage", textBox8.Text);
            sqlquery.Parameters.AddWithValue("@mdbID", textBox9.Text);
            sqlquery.Parameters.AddWithValue("@fileLoc", textBox10.Text);


            if (watchedCheck.Checked)
                sqlquery.Parameters.AddWithValue("@watched", "1");
            else
                sqlquery.Parameters.AddWithValue("@watched", "0");

            string logquery = sqlquery.CommandText;

            foreach (SqlParameter p in sqlquery.Parameters)
            {
                logquery = logquery.Replace(p.ParameterName, p.Value.ToString());
            }
            log("Query: " + logquery);
            log("Opening connection:" + "User ID=" + localdbUser + ";" +
                                       "Password=" + localdbPW + ";" +
                                       "Server=tcp:" + localdbHost + ",1433;" +
                                       "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                       "Database=" + localdbName + "; " +
                                       "connection timeout=30");

            using (SqlConnection connection = new SqlConnection("User ID=" + localdbUser + ";" +
                                       "Password=" + localdbPW + ";" +
                                       "Server=tcp:" + localdbHost + ",1433;" +
                                       "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                       "Database=" + localdbName + "; " +
                                       "connection timeout=30"))
            {
                try
                {
                    log("Inserting");
                    sqlquery.Connection = connection;
                    connection.Open();
                    sqlquery.ExecuteNonQuery();
                    connection.Close();
                    log("Done!");
                    clearInsertForm();
                }

                catch (Exception ex)
                {
                    connection.Close();
                    log("Failed with exception: " + ex.Message);
                }
            }
        }

        private void clearInsertForm()
        {
            if (output.InvokeRequired)
            {
                MethodInvoker invoker = () => clearInsertForm();
                Invoke(invoker);
            }
            else
            {
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                textBox4.Text = "";
                textBox5.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
                textBox8.Text = "";
                textBox9.Text = "";
                textBox10.Text = "M:\\\\";

                watchedCheck.Checked = false;
                confirmCheck.Checked = false;
            }
        }

        /******************* INSERT TAB CODE END *******************/


        /******************* UPDATE TAB CODE START *******************/

        private void button2_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(populateUpdateTab); //Run the work in a new thread to stop the form getting blocked
            t.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(updateDB); //Run the work in a new thread to stop the form getting blocked
            t.Start();
        }

        private void populateUpdateTab()
        {
            if (textBox21.Text.Length > 0)
            {

                string query = "SELECT * FROM movies WHERE movieID=" + textBox21.Text;
                SqlCommand sqlquery = new SqlCommand(query);
                log("Query: " + query);
                log("Opening connection:" + "User ID=" + localdbUser + ";" +
                                           "Password=" + localdbPW + ";" +
                                           "Server=tcp:" + localdbHost + ",1433;" +
                                           "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                           "Database=" + localdbName + "; " +
                                           "connection timeout=30");

                using (SqlConnection connection = new SqlConnection("User ID=" + localdbUser + ";" +
                                           "Password=" + localdbPW + ";" +
                                           "Server=tcp:" + localdbHost + ",1433;" +
                                           "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                           "Database=" + localdbName + "; " +
                                           "connection timeout=30"))
                {
                    try
                    {
                        log("Getting Movie info");
                        sqlquery.Connection = connection;
                        connection.Open();
                        SqlDataReader sdr = sqlquery.ExecuteReader();

                        while (sdr.Read())
                        {
                            textBox11.Text = sdr["imdbID"].ToString();
                            textBox12.Text = sdr["imdbTitle"].ToString();
                            textBox13.Text = sdr["imdbYear"].ToString();
                            textBox14.Text = sdr["imdbGenre"].ToString();
                            textBox15.Text = sdr["imdbRating"].ToString();
                            textBox16.Text = sdr["imdbRuntime"].ToString();
                            textBox17.Text = sdr["imdbPlot"].ToString();
                            textBox18.Text = sdr["moviedbImage"].ToString();
                            textBox19.Text = sdr["moviedbID"].ToString();
                            textBox20.Text = sdr["fileLocation"].ToString();
                            if (sdr["watched"].ToString() == "1")
                                checkBox2.Checked = true;
                            else
                                checkBox2.Checked = false;


                        }

                        connection.Close();
                        log("Done!");
                    }

                    catch (Exception ex)
                    {
                        connection.Close();
                        log("Failed with exception: " + ex.Message);
                    }
                }

            }
            else
            {
                log("No movieID specified, cannot populate fields.");
            }

        }

        private void updateDB()
        {

            if (!checkBox1.Checked)
            {
                log("Confirmation not ticked!");
                return;
            }
            if (textBox21.Text.Length == 0)
            {
                log("No movieID specified!");
                return;
            }
            log("Confirmation ticked, proceeding...");
            string query = "UPDATE Movies SET" +
                                "[imdbID] = @iID," +
                                "[imdbTitle] = @iTitle," +
                                "[imdbYear] = @iYear," +
                                "[imdbGenre] = @iGenre," +
                                "[imdbRating] = @iRating," +
                                "[imdbRuntime] = @iRunTime," +
                                "[imdbPlot] = @iPlot," +
                                "[moviedbImage] = @mdbImage," +
                                "[moviedbID] = @mdbID," +
                                "[fileLocation] = @fileLoc," +
                                "[watched] = @watched " +
                                "WHERE [movieID] = @mID;";


            SqlCommand sqlquery = new SqlCommand(query); //Insert the query params into the query. This protects against injection attack.
            sqlquery.Parameters.AddWithValue("@iID", textBox11.Text);
            sqlquery.Parameters.AddWithValue("@iTitle", textBox12.Text);
            sqlquery.Parameters.AddWithValue("@iYear", textBox13.Text);
            sqlquery.Parameters.AddWithValue("@iGenre", textBox14.Text);
            sqlquery.Parameters.AddWithValue("@iRating", textBox15.Text);
            sqlquery.Parameters.AddWithValue("@iRunTime", textBox16.Text);
            sqlquery.Parameters.AddWithValue("@iPlot", textBox17.Text);
            sqlquery.Parameters.AddWithValue("@mdbImage", textBox18.Text);
            sqlquery.Parameters.AddWithValue("@mdbID", textBox19.Text);
            sqlquery.Parameters.AddWithValue("@fileLoc", textBox20.Text);
            sqlquery.Parameters.AddWithValue("@mID", textBox21.Text);

            if (checkBox2.Checked)
                sqlquery.Parameters.AddWithValue("@watched", "1");
            else
                sqlquery.Parameters.AddWithValue("@watched", "0");

            string logquery = sqlquery.CommandText;

            foreach (SqlParameter p in sqlquery.Parameters)
            {
                logquery = logquery.Replace(p.ParameterName, p.Value.ToString());
            }
            log("Query: " + logquery);
            log("Opening connection:" + "User ID=" + localdbUser + ";" +
                                       "Password=" + localdbPW + ";" +
                                       "Server=tcp:" + localdbHost + ",1433;" +
                                       "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                       "Database=" + localdbName + "; " +
                                       "connection timeout=30");

            using (SqlConnection connection = new SqlConnection("User ID=" + localdbUser + ";" +
                                       "Password=" + localdbPW + ";" +
                                       "Server=tcp:" + localdbHost + ",1433;" +
                                       "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                       "Database=" + localdbName + "; " +
                                       "connection timeout=30"))
            {
                try
                {
                    log("Updating");
                    sqlquery.Connection = connection;
                    connection.Open();
                    sqlquery.ExecuteNonQuery();
                    connection.Close();
                    log("Done!");
                    clearUpdateForm();
                }

                catch (Exception ex)
                {
                    connection.Close();
                    log("Failed with exception: " + ex.Message);
                }
            }
        }


        private void clearUpdateForm()
        {
            textBox11.Text = "";
            textBox12.Text = "";
            textBox13.Text = "";
            textBox14.Text = "";
            textBox15.Text = "";
            textBox16.Text = "";
            textBox17.Text = "";
            textBox18.Text = "";
            textBox19.Text = "";
            textBox20.Text = "";
            textBox21.Text = "";


            checkBox1.Checked = false;
            checkBox2.Checked = false;
        }

        /******************* UPDATE TAB CODE END *******************/

        /******************* DELETE TAB CODE START *******************/
        private void button5_Click(object sender, EventArgs e)
        {

            Thread t = new Thread(populateDeleteTab); //Run the work in a new thread to stop the form getting blocked
            t.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(deleteFromDB); //Run the work in a new thread to stop the form getting blocked
            t.Start();
        }

        private void populateDeleteTab()
        {
            if (textBox22.Text.Length > 0)
            {

                string query = "SELECT * FROM movies WHERE movieID=" + textBox22.Text;
                SqlCommand sqlquery = new SqlCommand(query);
                log("Query: " + query);
                log("Opening connection:" + "User ID=" + localdbUser + ";" +
                                           "Password=" + localdbPW + ";" +
                                           "Server=tcp:" + localdbHost + ",1433;" +
                                           "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                           "Database=" + localdbName + "; " +
                                           "connection timeout=30");

                using (SqlConnection connection = new SqlConnection("User ID=" + localdbUser + ";" +
                                           "Password=" + localdbPW + ";" +
                                           "Server=tcp:" + localdbHost + ",1433;" +
                                           "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                           "Database=" + localdbName + "; " +
                                           "connection timeout=30"))
                {
                    try
                    {
                        log("Getting Movie info");
                        sqlquery.Connection = connection;
                        connection.Open();
                        SqlDataReader sdr = sqlquery.ExecuteReader();

                        while (sdr.Read())
                        {
                            textBox23.Text = sdr["imdbID"].ToString();
                            textBox24.Text = sdr["imdbTitle"].ToString();
                            textBox25.Text = sdr["imdbYear"].ToString();
                            textBox26.Text = sdr["imdbGenre"].ToString();
                            textBox27.Text = sdr["imdbRating"].ToString();
                            textBox28.Text = sdr["imdbRuntime"].ToString();
                            textBox29.Text = sdr["imdbPlot"].ToString();
                            textBox30.Text = sdr["moviedbImage"].ToString();
                            textBox31.Text = sdr["moviedbID"].ToString();
                            textBox32.Text = sdr["fileLocation"].ToString();
                            if (sdr["watched"].ToString() == "1")
                                checkBox4.Checked = true;
                            else
                                checkBox4.Checked = false;


                        }

                        connection.Close();
                        log("Done!");
                    }

                    catch (Exception ex)
                    {
                        connection.Close();
                        log("Failed with exception: " + ex.Message);
                    }
                }

            }
            else
            {
                log("No movieID specified, cannot populate fields.");
            }

        }

        private void deleteFromDB()
        {

            if (!checkBox3.Checked)
            {
                log("Confirmation not ticked!");
                return;
            }
            if (textBox22.Text.Length == 0)
            {
                log("No movieID specified!");
                return;
            }
            log("Confirmation ticked, proceeding...");
            string query = "DELETE FROM Movies WHERE [movieID] = @mID;";


            SqlCommand sqlquery = new SqlCommand(query); //Insert the query params into the query. This protects against injection attack.
            sqlquery.Parameters.AddWithValue("@mID", textBox22.Text);

            string logquery = sqlquery.CommandText;
            foreach (SqlParameter p in sqlquery.Parameters)
            {
                logquery = logquery.Replace(p.ParameterName, p.Value.ToString());
            }
            log("Query: " + logquery);
            log("Opening connection:" + "User ID=" + localdbUser + ";" +
                                       "Password=" + localdbPW + ";" +
                                       "Server=tcp:" + localdbHost + ",1433;" +
                                       "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                       "Database=" + localdbName + "; " +
                                       "connection timeout=30");

            using (SqlConnection connection = new SqlConnection("User ID=" + localdbUser + ";" +
                                       "Password=" + localdbPW + ";" +
                                       "Server=tcp:" + localdbHost + ",1433;" +
                                       "Trusted_Connection=false;" + //This has to be false, otherwise connection will attempt to use Windows account credentials.
                                       "Database=" + localdbName + "; " +
                                       "connection timeout=30"))
            {
                try
                {
                    log("Deleting");
                    sqlquery.Connection = connection;
                    connection.Open();
                    sqlquery.ExecuteNonQuery();
                    connection.Close();
                    log("Done!");
                    clearDeleteForm();
                }

                catch (Exception ex)
                {
                    connection.Close();
                    log("Failed with exception: " + ex.Message);
                }
            }
        }

        private void clearDeleteForm()
        {
            textBox23.Text = "";
            textBox24.Text = "";
            textBox25.Text = "";
            textBox26.Text = "";
            textBox27.Text = "";
            textBox28.Text = "";
            textBox29.Text = "";
            textBox30.Text = "";
            textBox31.Text = "";
            textBox32.Text = "";
            textBox22.Text = "";


            checkBox4.Checked = false;
            checkBox3.Checked = false;
        }



    }
}
