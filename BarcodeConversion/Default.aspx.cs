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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                label1Box.Focus();

                // Get your accessible jobs
                selectJob_Click(new object(), new EventArgs());
            }
            setDropdownColor();
        }


        // 'JOB ABBREVIATION' DROPDOWN: SET & DISPLAY CONTROLS OF SELECTED JOB. 
        protected void onJobSelect(object sender, EventArgs e)
        {
            try
            {
                // Set stage
                indexSavedMsg.Visible = false;
                generateIndexSection.Visible = false;
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
                if (this.selectJob.SelectedValue != "Select")
                {
                    // First, get selected job ID.
                    int jobID = getJobId(this.selectJob.SelectedValue);

                    if (jobID == 0)
                    {
                        string msg = "Error-02: Selected job not found. Contact system admin.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        selectJob.SelectedValue = "Select";
                        return;
                    }

                    // Then, check whether that job is configured, if so, display controls.
                    using (SqlConnection con = Helper.ConnectionObj)
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT JOB_ID, LABEL1, REGEX1, LABEL2, REGEX2, LABEL3, REGEX3," +
                                              "LABEL4, REGEX4, LABEL5, REGEX5 FROM JOB_CONFIG_INDEX WHERE JOB_ID = @jobID";
                            cmd.Parameters.AddWithValue("@jobID", jobID);

                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    indexCreationSection.Visible = true;

                                    // Set & display controls
                                    while (reader.Read())
                                    {
                                        string text1 = (string)reader.GetValue(1);
                                        if (text1 != string.Empty)
                                        {
                                            text1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text1.ToLower());
                                            LABEL1.Text = text1 + " :";
                                            LABEL1.Visible = true;
                                            label1Box.Visible = true;
                                            label1Box.Focus();
                                        }
                                        if (reader.GetValue(3) != DBNull.Value)
                                        {
                                            string text2 = (string)reader.GetValue(3);
                                            text2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text2.ToLower());
                                            LABEL2.Text = text2 + " :";
                                            LABEL2.Visible = true;
                                            label2Box.Visible = true;
                                        }
                                        if (reader.GetValue(5) != DBNull.Value)
                                        {
                                            string text3 = (string)reader.GetValue(5);
                                            text3 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text3.ToLower());
                                            LABEL3.Text = text3 + " :";
                                            LABEL3.Visible = true;
                                            label3Box.Visible = true;
                                        }
                                        if (reader.GetValue(7) != DBNull.Value)
                                        {
                                            string text4 = (string)reader.GetValue(7);
                                            text4 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text4.ToLower());
                                            LABEL4.Text = text4 + " :";
                                            LABEL4.Visible = true;
                                            label4Box.Visible = true;
                                        }
                                        if (reader.GetValue(9) != DBNull.Value)
                                        {
                                            string text5 = (string)reader.GetValue(9);
                                            text5 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text5.ToLower());
                                            LABEL5.Text = text5 + " :";
                                            LABEL5.Visible = true;
                                            label5Box.Visible = true;
                                        }
                                    }
                                    generateIndexSection.Visible = true;
                                }
                                else
                                {
                                    string msg = "The " + selectJob.SelectedValue + " job that you selected has not yet been configured by your system admin. Only red colored jobs can be processed.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    selectJob.SelectedValue = "Select";
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error-03: Issue occured while attempting to retrieve selected job's form controls. Contact system admin." +
                                    System.Environment.NewLine + ex.Message);
            }
        }      


        // 'GENERATE INDEX' CLICKED: GENERATE INDEX AND BARCODE FROM FORM DATA.
        private void generateBarcode()
        {
            try
            {
                if (!Page.IsValid) return;

                // First, get list of all entries.
                indexSetPrintedMsg.Visible = false;
                List<EntryContent> allEntriesList = new List<EntryContent>();
                allEntriesList = getEntries();
                ViewState["allEntriesList"] = allEntriesList;

                // If Index form not filled
                if (allEntriesList.Count == 0)
                {
                    string msg = "All fields are required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
                else
                {
                    string year = DateTime.Now.ToString("yy");
                    JulianCalendar jc = new JulianCalendar();
                    string julianDay = jc.GetDayOfYear(DateTime.Now).ToString();
                    string time = DateTime.Now.ToString("HHmmssfff");

                    // Making the Index string 
                    ViewState["allEntriesConcat"] = selectJob.SelectedValue.ToUpper() + year + julianDay + time;
                }
                string indexString = (string)ViewState["allEntriesConcat"];
                indexSavedMsg.Visible = false;
                generateIndexSection.Visible = true;

                // Convert index to barcode
                // imgBarcode.ImageUrl = string.Format("ShowCode39Barcode.ashx?code={0}&ShowText={1}&Thickness={2}",indexString,showTextValue, 1);
            }
            catch (Exception ex)
            {
                throw new Exception("Error-04: Issue occured while attempting to generate Index. Contact system admin." + 
                                    System.Environment.NewLine + ex.Message);
            }
        }



        // 'SAVE INDEX' CLICKED: SAVING INDEX INTO DB.
        protected void saveIndex_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                generateBarcode();
                // First, get current user id via name.
                string user = Environment.UserName;
                int opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    string msg = "Error-05: Couldn't identify this computer. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }

                // Then, get selected job id
                int jobID = getJobId(this.selectJob.SelectedValue);
                if (jobID == 0)
                {
                    string msg = "Error-06: Couldn't identify the selected job. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    selectJob.SelectedValue = "Select";
                    return;
                }

                // Saving
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO INDEX_DATA (JOB_ID, BARCODE, VALUE1, VALUE2, " +
                                            "VALUE3, VALUE4, VALUE5, OPERATOR_ID, CREATION_TIME, PRINTED) VALUES(@jobId, @barcodeIndex," +
                                            " @val1, @val2, @val3, @val4, @val5, @opId, @time, @printed)";
                        cmd.Parameters.AddWithValue("@jobID", jobID);
                        cmd.Parameters.AddWithValue("@barcodeIndex", ViewState["allEntriesConcat"]);
                        if (label1Box.Visible == true) { cmd.Parameters.AddWithValue("@val1", label1Box.Text); }
                        else { cmd.Parameters.AddWithValue("@val1", DBNull.Value); }
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
                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            indexSavedMsg.Visible = true;
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOperation", "FadeOut();", true);
                            clearFields();
                        }
                        else
                        {
                            string msg = "Error-07: Issue occured while attempting to save. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Violation of UNIQUE KEY"))
                {
                    throw new Exception("Error-08: The Index you are trying to save already exists! Click 'Generate Index' button to generate a new index.");
                }
                else
                {
                    throw new Exception("Error-09: Issue occured while attempting to save index. Contact system admin." +
                                        System.Environment.NewLine + ex.Message);
                }
            }
        }


        // 'SAVE & PRINT' CLICKED: SAVE THEN PRINT INDEX. 
        protected void saveAndPrint_Click(object sender, EventArgs e)
        {   
            try
            {
                // First, save index
                saveIndex_Click(new object(), new EventArgs());

                // Clear page
                formPanel.Visible = false;
                indexSavedMsg.Visible = false;
                Image imgBarcode = new Image();

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
                            "<td style='font-size:25px; font-weight:500;'>" + entry.labelText + "</td>" +
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
            catch (Exception ex)
            {
                throw new Exception("Error-10: Issue occured while attempting to setup the printing job. Contact system admin." +
                                     System.Environment.NewLine + ex.Message);
            }     
        }



        // SET INDEX AS PRINTED IN DB. 
        protected void setIndexAsPrinted()
        {
            try
            {
                if (!Page.IsValid) return;
                var counter = 0;
                string indexString = (string)ViewState["allEntriesConcat"];

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE INDEX_DATA SET PRINTED = @printed WHERE BARCODE = @barcodeIndex";
                        cmd.Parameters.AddWithValue("@printed", 1);
                        cmd.Parameters.AddWithValue("@barcodeIndex", indexString);
                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            counter++;
                            indexSetPrintedMsg.Visible = true;
                            indexSavedMsg.Visible = false;
                        }
                        else
                        {
                            string msg = "Error-11: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }

                        // Confirmation msg & back to unprinted indexes gridview
                        if (counter == 1)
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOperation", "FadeOut2();", true);
                        }
                        else
                        {
                            string msg = "Error-12: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error-13: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                throw new Exception(msg + System.Environment.NewLine + ex.Message);
            }
        }



        // SET INDEX AS PRINTED AFTER INDEX SHEET PRINTOUT 
        protected void setAsPrinted_Click(object sender, EventArgs e)
        {
            try
            {
                formPanel.Visible = true;
                setIndexAsPrinted();
            }
            catch (Exception ex)
            {
                string msg = "Error-14: Issue occured while attempting to set job as PRINTED. Contact system admin.";
                throw new Exception(msg + System.Environment.NewLine + ex.Message);
            }
        }



        // BACK TO BLANK FORM
        protected void backToForm_Click(object sender, EventArgs e)
        {
            formPanel.Visible = true;
        }



        // (HIDDEN) 'GENERATE JOBS' CLICKED: GET YOUR ACCESSIBLE JOBS.
        protected void selectJob_Click(Object sender, EventArgs e)
        {
            try
            {
                // First, get current user id via name.
                string user = Environment.UserName;
                List<int> jobIdList = new List<int>();
                noJobsFound.Visible = false;
                int jobID = 0;
                int opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    noJobsFound.Visible = true;
                    return;
                }

                // Then, get all job IDs accessible to current operator from OPERATOR_ACCESS.
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT JOB_ID FROM OPERATOR_ACCESS WHERE OPERATOR_ACCESS.OPERATOR_ID = @userId";
                        cmd.Parameters.AddWithValue("@userId", opID);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    jobID = (int)reader.GetValue(0);
                                    jobIdList.Add(jobID);
                                }
                            }
                            else
                            {   
                                // If no accessible jobs found, display message.
                                noJobsFound.Visible = true;
                                return;
                            }
                        }
                    }
                }

                // Now, for each job ID, get corresponding job abbreviation.
                if (jobIdList.Count > 0)
                {
                    foreach (var id in jobIdList)
                    {
                        using (SqlConnection con = Helper.ConnectionObj)
                        {
                            using (SqlCommand cmd = con.CreateCommand())
                            {
                                cmd.CommandText = "SELECT ABBREVIATION FROM JOB WHERE ID = @job";
                                cmd.Parameters.AddWithValue("@job", id);
                                con.Open();
                                var result = cmd.ExecuteScalar();
                                if (result != null)
                                {
                                    // Fill dropdown list
                                    string jobAbb = result.ToString();
                                    selectJob.Items.Add(jobAbb);
                                }
                                else
                                {
                                    string msg = "Error-15: Issue occured while attempting to retrieve jobs accessible to you. Contact system admin.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    string msg = "Error-16: Issue occured while attempting to retrieve jobs accessible to you. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg = "Error-17: Issue occured while attempting to retrieve jobs accessible to you. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + ex.Message + "');", true);
            }
        }



        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS.
        private void setDropdownColor()
        {
            try
            {
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ABBREVIATION " +
                                          "FROM JOB " +
                                          "INNER JOIN JOB_CONFIG_INDEX ON JOB.ID = JOB_CONFIG_INDEX.JOB_ID";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    foreach (ListItem item in selectJob.Items)
                                    {
                                        if (item.Value == (string)reader.GetValue(0))
                                        {
                                            item.Attributes.Add("style", "color:Red;");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error-18: Issue occured while attempting to color configured jobs in dropdown. Contact system admin.";
                throw new Exception(msg + System.Environment.NewLine + ex.Message);
            }
        }


        // GET JOB ID VIA SELECTED JOB ABBREV.
        private int getJobId(string jobAbb)
        {
            try 
            {
                int jobID = 0;
                using(SqlConnection con = Helper.ConnectionObj) 
                {
                    using (SqlCommand cmd = con.CreateCommand()) 
                    {
                        cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @abb";
                        cmd.Parameters.AddWithValue("@abb", jobAbb);
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            jobID = (int) result; 
                            return jobID;
                        }
                        else return jobID;
                    }
                }  
            } 
            catch(Exception ex) 
            {
                throw new Exception("Error-01: Issue occured while attempting to identify the selected Job. Contact system admin." +
                                    System.Environment.NewLine + ex.Message);
            }
        }



        // CLEAR TEXT FIELDS.
        private void clearFields()
        {
            try
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
            catch (Exception ex)
            {
                string msg = "Error-19: Issue occured while attempting to clear text fields. Contact system admin.";
                throw new Exception(msg + System.Environment.NewLine + ex.Message);
            }
        }



        // ALL ENTRIES.
        private List<EntryContent> getEntries()
        {
            try
            {
                List<EntryControl> controlList = new List<EntryControl>();
                List<EntryContent> contentList = new List<EntryContent>();
                EntryControl entry1 = new EntryControl(LABEL1, label1Box);
                controlList.Add(entry1);
                EntryControl entry2 = new EntryControl(LABEL2, label2Box);
                controlList.Add(entry2);
                EntryControl entry3 = new EntryControl(LABEL3, label3Box);
                controlList.Add(entry3);
                EntryControl entry4 = new EntryControl(LABEL4, label4Box);
                controlList.Add(entry4);
                EntryControl entry5 = new EntryControl(LABEL5, label5Box);
                controlList.Add(entry5);

                foreach (var control in controlList)
                {
                    if (control.textBox.Visible == true && control.textBox.Text == string.Empty)
                    {
                        // Return empty list
                        control.textBox.Focus();
                        contentList = new List<EntryContent>();
                        return contentList;
                    }
                    else if (control.textBox.Visible == true && control.textBox.Text != string.Empty)
                    {
                        EntryContent sampleEntry = new EntryContent(control.label.Text, control.textBox.Text);
                        contentList.Add(sampleEntry);
                    }
                }
                return contentList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error-03: Issue occured while retrieving entered entries. Contact system admin." +
                                    System.Environment.NewLine + ex.Message);
            }
        }    
    }
}

