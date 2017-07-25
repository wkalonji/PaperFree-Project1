using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Globalization;
using BarcodeConversion.App_Code;

namespace BarcodeConversion
{

    public partial class _Default : Page
    {
        SqlConnection con = new SqlConnection(@"Data Source=GLORY-PC\SQLEXPRESS;Initial Catalog=ImagePRO;Integrated Security=True");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                label1Box.Focus();

                // Get your assigned jobs
                selectJob_Click(new object(), new EventArgs());
            }
            con.Close();
            setDropdownColor(con);
        }



        // 'OnSelectedIndexChanged' CALLED: SET & DISPLAY CONTROLS OF SELECTED JOB. FUNCTION
        protected void onJobSelect(object sender, EventArgs e)
        {
            generateIndexSection.Visible = false;
            con.Open();

            // Set stage
            indexCreationSection.Visible = false;
            LABEL1.Visible = false;
            label1Box.Visible = false;
            label1Box.Text = string.Empty;
            LABEL2.Visible = false;
            label2Box.Visible = false;
            label2Box.Text = string.Empty;
            LABEL3.Visible = false;
            label3Box.Visible = false;
            label3Box.Text = string.Empty;
            LABEL4.Visible = false;
            label4Box.Visible = false;
            label4Box.Text = string.Empty;
            LABEL5.Visible = false;
            label5Box.Visible = false;
            label5Box.Text = string.Empty;
            
            // Make sure a job is selected
            if(this.selectJob.SelectedValue != "Select")
            {

                // First, get selected job ID.
                int jobID = getJobId(this.selectJob.SelectedValue, con);
                if(jobID == 0)
                {
                    string msg = "Error: Issues occured while retrieving job info. Please try again";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    selectJob.SelectedValue = "Select";
                    con.Close();
                    return;
                }
                
                // Then, check whether that job is configured, if so, display controls.
                SqlCommand cmd2 = new SqlCommand("SELECT JOB_ID, LABEL1, REGEX1, LABEL2, REGEX2, LABEL3, REGEX3," +
                    "LABEL4, REGEX4, LABEL5, REGEX5 FROM JOB_CONFIG_INDEX WHERE JOB_ID = @jobID", con);
                cmd2.Parameters.AddWithValue("@jobID", jobID);
                SqlDataReader reader2 = cmd2.ExecuteReader();
                if (reader2.HasRows)
                {
                    indexCreationSection.Visible = true;

                    // Set & display controls
                    while (reader2.Read())
                    {
                        string text1 = (string)reader2.GetValue(1);
                        if (text1 != string.Empty)
                        {
                            text1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text1.ToLower());
                            LABEL1.Text = text1 + ":";
                            LABEL1.Visible = true;
                            label1Box.Visible = true;
                            label1Box.Focus();
                        }
                        if(reader2.GetValue(3) != DBNull.Value)
                        {
                            string text2 = (string)reader2.GetValue(3);
                            text2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text2.ToLower());
                            LABEL2.Text = text2;
                            LABEL2.Visible = true;
                            label2Box.Visible = true;
                        }
                        if(reader2.GetValue(5) != DBNull.Value)
                        {
                            string text3 = (string)reader2.GetValue(5);
                            text3 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text3.ToLower());
                            LABEL3.Text = text3;
                            LABEL3.Visible = true;
                            label3Box.Visible = true;
                        }
                        if(reader2.GetValue(7) != DBNull.Value)
                        {
                            string text4 = (string)reader2.GetValue(7);
                            text4 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text4.ToLower());
                            LABEL4.Text = text4;
                            LABEL4.Visible = true;
                            label4Box.Visible = true;
                        }
                        if(reader2.GetValue(9) != DBNull.Value)
                        {
                            string text5 = (string)reader2.GetValue(9);
                            text5 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text5.ToLower());
                            LABEL5.Text = text5;
                            LABEL5.Visible = true;
                            label5Box.Visible = true;
                        }
                    }
                    reader2.Close();
                }
                else
                {   
                    string msg = "The "+ this.selectJob.SelectedValue + " Job that you selected has not yet been configured. Please contact your system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    selectJob.SelectedValue = "Select";
                    con.Close();
                    return;
                }
            }         
        }



        // 'GENERATE INDEX' CLICKED: GENERATE INDEX AND BARCODE FROM FORM DATA. FUNCTION
        protected void btnGenerateBarcode_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // First, get list of all entries.
            string allEntriesConcat = string.Empty;
            List<EntryContent> allEntriesList = new List<EntryContent>();
            allEntriesList = getEntries();
            ViewState["allEntriesList"] = allEntriesList;
            if(allEntriesList.Count == 0)
            {
                string msg = "All fields are required!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                return;
            }
            else
            {
                foreach(var entry in allEntriesList)
                {
                    allEntriesConcat += entry.text;
                }
                string today = DateTime.Today.ToString("yyyyMMdd");
                ViewState["allEntriesConcat"] = allEntriesConcat + today;
            }
            var showTextValue = chkShowText.Checked ? "1" : "0";
            string indexString = (string)ViewState["allEntriesConcat"];
            textToConvert.Text = indexString.ToUpper();
            indexSavedMsg.Visible = false;
            generateIndexSection.Visible = true;
            
            // Convert index to barcode
            imgBarcode.ImageUrl = string.Format("ShowCode39Barcode.ashx?code={0}&ShowText={1}&Thickness={2}",
                                                indexString,
                                                showTextValue,1);
            /* imgBarcode.ImageUrl = string.Format("ShowCode39Barcode.ashx?code={0}&ShowText={1}&Thickness={2}",
                                                 textBarcodeText,
                                                 showTextValue,
                                                 ddlBarcodeThickness.SelectedValue); */
        }



        // 'SAVE INDEX' CLICKED: SAVING INDEX INTO DB. FUNCTION
        protected void saveIndex_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            con.Open();

            // First, get current user id via name.
            string user = Environment.UserName;
            int opID = getUserId(user, con);
            if (opID == 0)
            {
                string msg = "Your name could not be found. Contact Tech Support";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                con.Close();
                return;
            }

            // Second, get selected job id
            int jobID = getJobId(this.selectJob.SelectedValue, con);
            if(jobID == 0)
            {
                string msg = "Error: Issues occured while retrieving job info. Please try again";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                selectJob.SelectedValue = "Select";
                con.Close();
                return;
            }

            // Now save the record into INDEX_DATA table
            SqlCommand cmd = new SqlCommand("INSERT INTO INDEX_DATA (JOB_ID, BARCODE, VALUE1, VALUE2, " +
                "VALUE3, VALUE4, VALUE5, OPERATOR_ID, CREATION_TIME, PRINTED) VALUES(@jobId, @barcodeIndex," +
                " @val1, @val2, @val3, @val4, @val5, @opId, @time, @printed)", con);

            cmd.Parameters.AddWithValue("@jobId", jobID);
            cmd.Parameters.AddWithValue("@barcodeIndex", ViewState["allEntriesConcat"]);
            if (label1Box.Visible == true) { cmd.Parameters.AddWithValue("@val1", label1Box.Text); }
            else { cmd.Parameters.AddWithValue("@val1", DBNull.Value);}
            if (label2Box.Visible == true) { cmd.Parameters.AddWithValue("@val2", label2Box.Text); }
            else { cmd.Parameters.AddWithValue("@val2", DBNull.Value); }
            if (label3Box.Visible == true) { cmd.Parameters.AddWithValue("@val3", label3Box.Text); }
            else { cmd.Parameters.AddWithValue("@val3", DBNull.Value); }
            if (label4Box.Visible == true) { cmd.Parameters.AddWithValue("@val4", label4Box.Text); }
            else { cmd.Parameters.AddWithValue("@val4", DBNull.Value); }
            if (label5Box.Visible == true) { cmd.Parameters.AddWithValue("@val5", label5Box.Text); }
            else { cmd.Parameters.AddWithValue("@val5", DBNull.Value); }
            cmd.Parameters.AddWithValue("@opId", opID);
            cmd.Parameters.AddWithValue("@time", DateTime.Now);
            cmd.Parameters.AddWithValue("@printed", 0);

            if (cmd.ExecuteNonQuery() == 1) {
                indexSavedMsg.Visible = true;
                ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOperation", "FadeOut();", true);
                clearFields();
                generateIndexSection.Visible = false;
            }
            else
            {
                string msg = "Index string NOT saved. Try again or contact Tech support.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
            }
            con.Close();
        }



        // 'SAVE & PRINT' CLICKED: SAVE THEN PRINT INDEX. FUNCTION
        protected void saveAndPrint_Click(object sender, EventArgs e)
        {   
            // First, save index
            saveIndex_Click(new object(), new EventArgs());
            
            // Clear page
            formPanel.Visible = false;
            indexSavedMsg.Visible = false;

            // Write Index sheet page content
            string indexString = (string)ViewState["allEntriesConcat"];
            imgBarcode.ImageUrl = string.Format("ShowCode39BarCode.ashx?code={0}&ShowText=1&Height=50", indexString.PadLeft(8, '0'));
           
            Response.Write(
                "<div id = 'pageToPrint' style='margin-top:-50px;'>" +
                    "<div>" +
                        "<div style='font-size:25px; font-weight:500;'>" +
                            "<img src='" + imgBarcode.ImageUrl + "' height='160px' width='500px' style='margin-top:0px; '> " +
                        "</div>" +
                        "<div style='font-size:25px; font-weight:500; text-align:right;' >" +
                            "<img src='" + imgBarcode.ImageUrl + "' height='160px' width='500px' style='margin-top:250px; margin-right:-180px;' class='rotate'> " +
                        "</div>" +
                    "</div>" +

                    "<table style='margin-top:250px; margin-bottom:580px; margin-left:40px;'>" +
                        "<tr>" +
                            "<td style='font-size:25px; font-weight:500;'> Index String: </td>" +
                            "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + indexString.ToUpper() + "</td>" +
                        "</tr>"
             );
            List<EntryContent> allEntriesList = new List<EntryContent>();
            allEntriesList = (List<EntryContent>)ViewState["allEntriesList"];

            foreach (var entry in allEntriesList)
            {
                Response.Write(
                    "<tr>" +
                        "<td style='font-size:25px; font-weight:500;'>" + entry.labelText + ": </h2></td>" +
                        "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + entry.text.ToUpper() + "</td>" +
                    "</tr>" 
                );
            }
            Response.Write(
                        "<tr>" +
                            "<td style='font-size:25px; font-weight:500;'>Date Created: </td>" +
                            "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + DateTime.Now + "</td>" +
                        "</tr>" +
                    "</table >" +
                "</div>"
            );

            // Finally, Print Index sheet.
            ClientScript.RegisterStartupScript(this.GetType(), "PrintOperation", "printing();", true);           
        }



        // SET INDEX AS PRINTED IN DB. HELPER FUNCTION
        protected void setIndexAsPrinted_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            var counter = 0;
            string indexString = (string)ViewState["allEntriesConcat"];
            con.Open();
            SqlCommand cmd = new SqlCommand("UPDATE INDEX_DATA SET PRINTED = @printed WHERE BARCODE = @barcodeIndex", con);
            cmd.Parameters.AddWithValue("@printed", 1);
            cmd.Parameters.AddWithValue("@barcodeIndex", indexString);
            int some = cmd.ExecuteNonQuery();
            if (cmd.ExecuteNonQuery() == 1)
            {
                counter++;
                indexSetPrintedMsg.Visible = true;
                indexSavedMsg.Visible = false;
            }
            else
            {
                string msg = "Index saved, but there was an unexpected error setting it to PRINTED. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
            }
            con.Close();
            // Confirmation msg & back to unprinted indexes gridview
            if (counter == 1)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOperation", "FadeOut2();", true);
            }
            else
            {
                string msg = "Error: Index did not set as PRINTED";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
            }
        }



        // SET INDEX AS PRINTED AFTER INDEX SHEET PRINTOUT. FUNCTION
        protected void setAsPrinted_Click(object sender, EventArgs e)
        {
            formPanel.Visible = true;
            setIndexAsPrinted_Click(new object(), new EventArgs());
        }

        // BACK TO BLANK FORM. FUNCTION
        protected void backToForm_Click(object sender, EventArgs e)
        {
            formPanel.Visible = true;
        }


        //----HELPER FUNCTION -------------------------------------------------------------------------------------------------


        // GET YOUR ASSIGNED JOBS. HELPER FUNCTION
        protected void selectJob_Click(Object sender, EventArgs e)
        {
            // First, get current user id via name.
            string user = Environment.UserName;
            int jobID = 0;
            List<int> jobIdList = new List<int>();
            noJobsFound.Visible = false;


            con.Open();
            int opID = getUserId(user, con);
            if (opID == 0)
            {
                string msg = "Your name could not be found, thus not allowed to use this product. Contact Tech Support";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                con.Close();
                return;
            }

            // Then, get all job IDs accessible to current user from OPERATOR_ACCESS.
            SqlCommand cmd2 = new SqlCommand("SELECT JOB_ID FROM OPERATOR_ACCESS WHERE OPERATOR_ACCESS.OPERATOR_ID = @userId", con);
            cmd2.Parameters.AddWithValue("@userId", opID);
            SqlDataReader reader2 = cmd2.ExecuteReader();
            if (reader2.HasRows)
            {
                while (reader2.Read())
                {
                    jobID = (int)reader2.GetValue(0);
                    jobIdList.Add(jobID);
                }
                reader2.Close();
            }
            else
            {
                noJobsFound.Visible = true; 
                return;
            }

            // Now, for each job ID, get corresponding job abbreviation.
            if (jobIdList.Count > 0)
            {
                foreach (var id in jobIdList)
                {
                    SqlCommand cmd3 = new SqlCommand("SELECT ABBREVIATION FROM JOB WHERE ID = @job", con);
                    cmd3.Parameters.AddWithValue("@job", id);
                    SqlDataReader reader3 = cmd3.ExecuteReader();
                    if (reader3.HasRows)
                    {
                        while (reader3.Read())
                        {
                            string jobAbb = (string)reader3.GetValue(0);
                            selectJob.Items.Add(jobAbb);
                            selectJob.AutoPostBack = true;

                        }
                        reader3.Close();
                    }
                    else
                    {
                        string msg = "Some went wront while getting job abbreviations from job IDs.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        return;
                    }
                }
            }
            else
            {
                string msg = "For some reason, jobIdList did not populate";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                return;
            }
            con.Close();
        }



        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS. HELPER FUNCTION
        private void setDropdownColor(SqlConnection con)
        {
            con.Open();
            SqlCommand cmd4 = new SqlCommand("SELECT ABBREVIATION " +
                                             "FROM JOB " +
                                             "INNER JOIN JOB_CONFIG_INDEX ON JOB.ID = JOB_CONFIG_INDEX.JOB_ID", con);
            SqlDataReader reader4 = cmd4.ExecuteReader();
            if (reader4.HasRows)
            {
                while (reader4.Read())
                {
                    foreach (ListItem item in selectJob.Items)
                    {
                        if (item.Value == (string)reader4.GetValue(0))
                        {
                            item.Attributes.Add("style", "color:Red;");
                        }
                    }
                }
                reader4.Close();
                con.Close();
            }
            else
            {
                string msg = "Couldn't set background color for your configured jobs";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                con.Close();
                return;
            }
        }



        // GET USER ID VIA USERNAME. HELPER FUNCTION
        private int getUserId(string user, SqlConnection con)
        {
            int opID = 0;
            SqlCommand cmd = new SqlCommand("SELECT ID FROM OPERATOR WHERE NAME = @username", con);
            cmd.Parameters.AddWithValue("@username", user);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    opID = (int)reader.GetValue(0);
                }
                reader.Close();
                return opID;
            }
            else
            {
                return opID;
            }
        }



        // GET JOB ID VIA SELECTED JOB ABBREV. HELPER FUNCITON
        private int getJobId(string jobAbb, SqlConnection con)
        {
            int jobID = 0;
            SqlCommand cmd = new SqlCommand("SELECT ID FROM JOB WHERE ABBREVIATION = @abb", con);
            cmd.Parameters.AddWithValue("@abb", this.selectJob.SelectedValue);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    jobID = (int)reader.GetValue(0);
                }
                reader.Close();
                return jobID ;
            }
            else
            {
                return jobID;
            }
        }

        // CLEAR TEXT FIELDS. HELPER FUNCTION
        private void clearFields()
        {
            List<TextBox> textBoxList = new List<TextBox>();
            textBoxList.Add(label1Box);
            textBoxList.Add(label2Box);
            textBoxList.Add(label3Box);
            textBoxList.Add(label4Box);
            textBoxList.Add(label5Box);

            foreach (var textBox in textBoxList)
            {
                if (textBox.Visible == true) textBox.Text = string.Empty;
            }
            foreach (var textBox in textBoxList)
            {
                if (textBox.Visible == true)
                {
                    textBox.Focus();
                    return;
                }
            }
        }



        // ALL ENTRIES. HELPER FUNCTION
        private List<EntryContent> getEntries()
        {
            List<EntryControl> controlList = new List<EntryControl>();
            List<EntryContent> contentList = new List<EntryContent>();
            EntryControl entry1 = new EntryControl(LABEL1,label1Box);
            controlList.Add(entry1);
            EntryControl entry2 = new EntryControl(LABEL2,label2Box);
            controlList.Add(entry2);
            EntryControl entry3 = new EntryControl(LABEL3,label3Box);
            controlList.Add(entry3);
            EntryControl entry4 = new EntryControl(LABEL4,label4Box);
            controlList.Add(entry4);
            EntryControl entry5 = new EntryControl(LABEL5, label5Box);
            controlList.Add(entry5);

            foreach (var control in controlList)
            {
                if(control.textBox.Visible == true && control.textBox.Text == string.Empty)
                {
                    // Return empty list
                    control.textBox.Focus();
                    contentList = new List<EntryContent>();
                    return contentList;
                }
                else if(control.textBox.Visible == true && control.textBox.Text != string.Empty)
                {
                    EntryContent sampleEntry = new EntryContent(control.label.Text, control.textBox.Text);
                    contentList.Add(sampleEntry);
                }
            }
            return contentList;
        }
    }
}