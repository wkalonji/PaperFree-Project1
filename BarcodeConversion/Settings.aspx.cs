﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Data;
using BarcodeConversion.App_Code;

namespace BarcodeConversion
{
    public partial class Contact : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) jobAbb.Focus();
            setDropdownColor();
            getDropdownJobItems();
            success.Visible = false;
        }




        // 'CHOOSE YOUR ACTION' SELECTED
        protected void actionChange(object sender, EventArgs e)
        {

            if (selectAction.SelectedValue == "edit")
            {
                // Fill dropdown list with jobs.
                selectJobList.Items.Clear();
                selectJobList.Items.Add("Select");
                getDropdownJobItems();
                jobAbb.Visible = false;
                createJobBtn.Visible = false;
                deleteJobBtn.Visible = false;
                selectJobList.Visible = true;
                editJobBtn.Visible = true;
                jobNameLabel.Visible = true;
                jobName.Visible = true;
                jobName.Attributes["placeholder"] = " Optional";
                jobActiveLabel.Visible = true;
                jobActiveBtn.Visible = true;
            }
            else if (selectAction.SelectedValue == "create")
            {
                jobSectionDefault();
            }
            else
            {
                editJobBtn.Visible = false;
                jobAbb.Visible = false;
                createJobBtn.Visible = false;
                jobNameLabel.Visible = false;
                jobName.Visible = false;
                jobActiveLabel.Visible = false;
                jobActiveBtn.Visible = false;
                jobAssignedToLabel.Visible = false;
                jobAssignedTo.Visible = false;
                selectJobList.Visible = true;
                deleteJobBtn.Visible = true;
                getDropdownJobItems();
            }
        }




        // 'CREATE' CLICKED: CREATE NEW JOB. FUNCTION
        protected void createJob_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            SqlConnection con = Helper.ConnectionObj;
            con.Open();

            if (this.jobAbb.Text == string.Empty)
            {
                string msg = "Job abbreviation is required!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                jobAbb.Text = string.Empty;
                jobAbb.Focus();
                con.Close();
                return;
            }
            else if (this.jobName.Text == string.Empty)
            {
                string warning2 = "Job name is required!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + warning2 + "');", true);
                jobName.Text = string.Empty;
                jobName.Focus();
                con.Close();
                return;
            }
            else
            {
                try
                {
                    // Save created job
                    SqlCommand cmd = new SqlCommand("INSERT INTO JOB (ABBREVIATION, NAME, ACTIVE) VALUES(@abbr, @name, @active)", con);
                    cmd.Parameters.AddWithValue("@abbr", this.jobAbb.Text);
                    cmd.Parameters.AddWithValue("@name", this.jobName.Text);
                    cmd.Parameters.AddWithValue("@active", this.jobActiveBtn.SelectedValue);
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        // Assign job to assignee
                        if (jobAssignedTo.Text != string.Empty)
                        {
                            string assignee = jobAssignedTo.Text;
                            string abbr = jobAbb.Text;
                            bool answer = AssignJob(assignee, abbr, con); // calling assignJob function
                            if (answer == true)
                            {
                                string msg = "New job successfully saved & assigned!";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                jobFormClear();
                            }
                            else
                            {
                                string msg = "New job successfully saved, but not assigned! Contact tech support.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                jobFormClear();
                            }
                        }
                        else
                        {
                            success.Text = "New Job Created!";
                            success.Visible = true;
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                            jobFormClear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = "This Job may already exists. Duplicates are not allowed.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    con.Close();
                    jobFormClear();
                    return;
                }
            }
            con.Close();
        }



        // 'EDIT' CLICKED: EDIT JOB NAME OR ACTIVE STATUS. FUNCTION
        protected void editJob_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            SqlConnection con = Helper.ConnectionObj;
            con.Open();
            
            // Edit job
            SqlCommand cmd = new SqlCommand();
            if(this.selectJobList.SelectedValue == "Select")
            {
                string msg = "Please select a Job.";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                jobFormClear();
                return;
            }
            if (this.jobName.Text == string.Empty)
            {
                cmd = new SqlCommand("UPDATE JOB SET ACTIVE = @active WHERE ABBREVIATION = @abbr", con);
                cmd.Parameters.AddWithValue("@active", this.jobActiveBtn.SelectedValue);
                cmd.Parameters.AddWithValue("@abbr", this.selectJobList.SelectedValue);
            }
            else
            {
                cmd = new SqlCommand("UPDATE JOB SET NAME = @job, ACTIVE = @active WHERE ABBREVIATION = @abbr", con);
                cmd.Parameters.AddWithValue("@job", this.jobName.Text);
                cmd.Parameters.AddWithValue("@active", this.jobActiveBtn.SelectedValue);
                cmd.Parameters.AddWithValue("@abbr", this.selectJobList.SelectedValue);
            }
            try
            {
                if (cmd.ExecuteNonQuery() == 1)
                {
                    // Assign job to assignee
                    if (jobAssignedTo.Visible = true && jobAssignedTo.Text != string.Empty)
                    {
                        string assignee = jobAssignedTo.Text;
                        string abbr = this.selectJobList.SelectedValue;
                        bool answer = AssignJob(assignee, abbr, con); // calling assignJob function
                        if (answer == true)
                        {
                            success.Text = "Job successfully updated & assigned!";
                            success.Visible = true;
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                            jobFormClear();
                        }
                        else
                        {
                            string msg = "Job updated successfully, but not assigned! Contact System Admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            jobFormClear();
                        }
                    }
                    else
                    {
                        success.Text = "Job updated successfully!";
                        success.Visible = true;
                        ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                        jobFormClear();
                    }
                    con.Close();
                    getDropdownJobItems();
                    getActiveJobs();
                    getUnassignedJobs(null);
                }
                else
                {
                    string msg = "Job not updated. Try again or contact System Admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    con.Close();
                    jobFormClear();
                    return;
                }
                con.Close();
            }
            catch(Exception ex)
            {
                string msg = "Error.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
            }
            
        }



        // 'DELETE' CLICKED: DELETE JOB. FUNCTION
        protected void deleteJob_Click(object sender, EventArgs e)
        {
            int jobID = 0;
            SqlConnection con = Helper.ConnectionObj;
            con.Open();
            if (selectJobList.SelectedValue != "Select")
            {
                // First, get ID of specified job.
                SqlCommand cmd = new SqlCommand("SELECT ID FROM JOB WHERE ABBREVIATION = @abb", con);               
                cmd.Parameters.AddWithValue("@abb", this.selectJobList.SelectedValue);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        jobID = (int)reader.GetValue(0);
                    }
                    reader.Close();
                }
                else
                {
                    string msg = "Specified job does not exist, thus cannot be deleted!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    jobFormClear();
                    jobAssignedToLabel.Visible = false;
                    jobAssignedTo.Visible = false;
                    con.Close();
                    return;
                }

                if(jobID > 0)
                {
                    // TBD: Got to delete the config 1st
                    // Then, delete job record in OPERATOR_ACCESS
                    SqlCommand cmd2 = new SqlCommand("DELETE FROM OPERATOR_ACCESS WHERE JOB_ID = @jobID", con);
                    cmd2.Parameters.AddWithValue("@jobID", jobID);
                    if (cmd2.ExecuteNonQuery() == 1)
                    {
                        // Finally, delete job record in JOB table
                        SqlCommand cmd3 = new SqlCommand("DELETE FROM JOB WHERE ABBREVIATION = @abb", con);
                        cmd3.Parameters.AddWithValue("@abb", this.selectJobList.SelectedValue);
                        if (cmd3.ExecuteNonQuery() == 1)
                        { 
                            success.Text = "Job successfully deleted!";
                            success.Visible = true;
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                            jobFormClear();
                            jobAssignedToLabel.Visible = false;
                            jobAssignedTo.Visible = false;
                            con.Close();
                            return;
                        }
                        else
                        {
                            string msg = "Error: There was an error in deleting this job in JOB.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            jobFormClear();
                            jobAssignedToLabel.Visible = false;
                            jobAssignedTo.Visible = false;
                            con.Close();
                            return;
                        }
                    }
                    else
                    {
                        // If no record in OPERATOR_ACCESS, just delete it in JOB
                        SqlCommand cmd3 = new SqlCommand("DELETE FROM JOB WHERE ABBREVIATION = @abb", con);
                        cmd3.Parameters.AddWithValue("@abb", this.selectJobList.SelectedValue);
                        if (cmd3.ExecuteNonQuery() == 1)
                        {
                            success.Text = "Job successfully deleted!";
                            success.Visible = true;
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                            jobFormClear();
                            jobAssignedToLabel.Visible = false;
                            jobAssignedTo.Visible = false;
                            con.Close();
                            return;
                        }
                        else
                        {
                            string msg = "Error: There was an error in deleting this job in JOB.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            jobFormClear();
                            jobAssignedToLabel.Visible = false;
                            jobAssignedTo.Visible = false;
                            con.Close();
                            return;
                        }
                    }
                }
            }
            con.Close();
            getDropdownJobItems();
            jobAssignedToLabel.Visible = false;
            jobAssignedTo.Visible = false;
        }

    

        // SET OPERATOR ADMIN PERMISSIONS. FUNCTION
        protected void setPermissions_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                if (user.Text != null)
                {
                    // If user exists, set Admin status
                    SqlConnection con = Helper.ConnectionObj;
                    con.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE OPERATOR SET ADMIN = @admin WHERE NAME = @user", con);
                    cmd.Parameters.AddWithValue("@admin", this.permissions.SelectedValue);
                    cmd.Parameters.AddWithValue("@user", this.user.Text);
                    if (cmd.ExecuteNonQuery() == 1)
                    {
                        string msg = "Operator permissions set!";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);     
                        permissionsFormClear();
                        con.Close();
                    }
                    else
                    {
                        // If user doesn't exist, register user and set Admin status.
                        string msg;
                        SqlCommand cmd2 = new SqlCommand("INSERT INTO OPERATOR (NAME, ADMIN) VALUES(@user,@admin)", con);
                        cmd2.Parameters.AddWithValue("@user", this.user.Text);
                        cmd2.Parameters.AddWithValue("@admin", this.permissions.SelectedValue);

                        if (cmd2.ExecuteNonQuery() == 1)
                        {
                            msg = "Operator permissions set!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            con.Close();
                            permissionsFormClear();
                        }
                        else
                        {
                            msg = "Failed to set operator permissions!";
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "')", true);
                            con.Close();
                            permissionsFormClear();
                            return;
                        }
                    }
                }
                else
                {
                    string msg = "Operator field is required!";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "')", true);
                    permissionsFormClear();
                }
            }
            catch (Exception ex)
            {
                Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message +"');", true);
            }
        }



        // CHECKBOX THAT SETS ALL THE OTHERS. HELPER FUNCTION
        protected void selectAll_changed(object sender, EventArgs e)
        {
            CheckBox ChkBoxHeader = (CheckBox)jobAccessGridView.HeaderRow.FindControl("selectAll");
            foreach (GridViewRow row in jobAccessGridView.Rows)
            {
                CheckBox ChkBoxRows = (CheckBox)row.FindControl("cbSelect");
                if (ChkBoxHeader.Checked == true)
                {
                    ChkBoxRows.Checked = true;
                }
                else
                {
                    ChkBoxRows.Checked = false;
                }
            }
        }



        // CLEAR JOB FORM. HELPER FUNCTION
        private void jobFormClear()
        {
            jobAbb.Text = string.Empty;
            selectJobList.SelectedValue = "Select";
            jobName.Text = string.Empty;
            jobActiveBtn.SelectedValue = "1";
            jobAssignedTo.Text = string.Empty;
            jobAssignedToLabel.Visible = true;
            jobAssignedTo.Visible = true;
            jobAbb.Focus();
        }



        // CLEAR PERMISSIONS FORM. HELPER FUNCTION
        private void permissionsFormClear()
        {
            user.Text = string.Empty;
            permissions.SelectedValue = "0";
            user.Focus();
        }



        // HIDE/COLLAPSE JOB SECTION. FUNCTION
        protected void newJobShow_Click(object sender, EventArgs e)
        {
            if (jobSection.Visible == false)
            {
                jobSection.Visible = true;
                line.Visible = true;
                jobSectionDefault();
            }
            else
            {
                jobSection.Visible = false;
                if (newUserSection.Visible == false) line.Visible = false;
            }
        }



        // 'PERMISSION SECTION' CLICKED: HIDE/SHOW USER PERMISSION SECTION
        protected void permissionsShow_Click(object sender, EventArgs e)
        {
            if(newUserSection.Visible == false)
            {
                newUserSection.Visible = true;
                line.Visible = true;
            }
            else
            {
                newUserSection.Visible = false;
                if (jobSection.Visible == false) line.Visible = false;
            }          
        }



        // 'JOB ACCESS SECTION' CLICKED: HIDE/SHOW JOB-ACCESS SECTION & PULL InnaccessibleED JOBS
        protected void assignShow_Click(object sender, EventArgs e)
        {
            if (assignPanel.Visible == false)
            {
                Page.Validate();
                assignPanel.Visible = true;
                assignee.Text = string.Empty;
                assignee.Focus();
                // Get all unassigned jobs
                getUnassignedJobs(null);
            }
            else
            {
                assignPanel.Visible = false;
            }
        }



        
        // 'ACCESSIBLE' CLICKED: GET OP'S ACCESSIBLE JOBS. FUNCTION
        protected void assignedJob_Click(object sender, EventArgs e)
        {
            SqlConnection con = Helper.ConnectionObj;
            con.Open();
            if(assignee.Text == string.Empty)
            {
                string msg = "Operator field is required!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                assignee.Focus();
                con.Close();
                return; 
            }
            else
            {
                // If operator field not empty, get operator ID, then jobs
                int opID = 0;
                SqlCommand cmd = new SqlCommand("SELECT ID FROM OPERATOR WHERE NAME = @name", con);
                cmd.Parameters.AddWithValue("@name", assignee.Text);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows) // If operator exists
                {
                    while (reader.Read())
                    {
                        opID = (int)reader.GetValue(0);
                    }
                    reader.Close();
                    jobsLabel.Text = "Operator's Currently Accessible Jobs";
                    // Get operator assigned jobs
                    jobAccessBtn.Visible = false;
                    SqlCommand cmd2 = null;
                    SqlDataAdapter da = null;
                    DataSet ds = null;
                    try
                    {
                        //if (!Page.IsValid) return;

                        cmd2 = new SqlCommand("SELECT ABBREVIATION " +
                                             "FROM JOB " +
                                             "INNER JOIN OPERATOR_ACCESS ON JOB.ID = OPERATOR_ACCESS.JOB_ID " +
                                             "WHERE ACTIVE = 1 AND OPERATOR_ACCESS.OPERATOR_ID = @ID", con);
                        cmd2.Parameters.AddWithValue("@ID", opID);
                        da = new SqlDataAdapter(cmd2);
                        ds = new DataSet();
                        da.Fill(ds);
                        if (ds.Tables.Count > 0)
                        {
                            jobAccessGridView.DataSource = ds.Tables[0];
                            //indexesGridView.AllowPaging = true;
                            jobAccessGridView.DataBind();
                            jobAccessGridView.Visible = true;
                            assignee.Focus();
                        }
                        con.Close();

                        // Handling of whether any JOB was returned from DB
                        if (jobAccessGridView.Rows.Count == 0)
                        {
                            jobAccessGridView.Visible = false;
                            jobAccessBtn.Visible = false;
                            deleteAssignedBtn.Visible = false;
                            jobsLabel.Text = "No Accessible Jobs Found.";
                            assignee.Focus();
                        }
                        else
                        {
                            jobAccessBtn.Visible = false;
                            deleteAssignedBtn.Visible = true;
                            assignee.Focus();
                        }
                    }
                    catch (SqlException ex)
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
                    }
                    catch (Exception ex)
                    {
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
                    }
                    finally
                    {
                        if (da != null)
                        {
                            da.Dispose();
                        }
                        if (cmd != null)
                        {
                            cmd.Dispose();
                        }
                        if (con != null)
                        {
                            con.Dispose();
                        }
                    }
                }
                else
                {
                    reader.Close();
                    string msg = "Operator name could not be found!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                }
            }
            
        }

        


        // 'UNASSIGNED' CLICKED: GET ALL JOBS. FUNCTION
        protected void unassignedJob_Click(object sender, EventArgs e)
        {
            getUnassignedJobs(sender);
            assignee.Focus();
        }



        // 'DENY' CLICKED: REMOVE OPERATOR ACCESS TO JOBS. FUNCTION
        protected void deleteAssigned_Click(object sender, EventArgs e)
        {
            SqlConnection con = Helper.ConnectionObj;
            con.Open();
            if (assignee.Text == string.Empty)
            {
                string msg = "Operator field is required!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                assignee.Focus();
                con.Close();
                return;
            }
            else
            {
                int opID = 0;
                int jobID = 0;
                int count = 0;
                // If operator field not empty, get Operator ID
                SqlCommand cmd = new SqlCommand("SELECT ID FROM OPERATOR WHERE NAME = @name", con);
                cmd.Parameters.AddWithValue("@name", assignee.Text);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows) // If operator exists
                {
                    while (reader.Read())
                    {
                        opID = (int)reader.GetValue(0);
                    }
                    reader.Close();
                }
                else
                {
                    string msg = "Operator entered could not be found!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                    con.Close();
                    assignedJob_Click(new object(), new EventArgs());
                    return;
                }

                // For each selected job, remove access.
                foreach (GridViewRow row in jobAccessGridView.Rows)
                {
                    CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;
                    if (chxBox.Checked)
                    {
                        // First, get ID of selectd job
                        SqlCommand cmd2 = new SqlCommand("SELECT ID FROM JOB WHERE ABBREVIATION = @abbr", con);
                        cmd2.Parameters.AddWithValue("@abbr", row.Cells[2].Text);
                        SqlDataReader reader2 = cmd2.ExecuteReader();
                        if (reader2.HasRows) // If job exists
                        {
                            while (reader2.Read())
                            {
                                jobID = (int)reader2.GetValue(0);
                            }
                            reader2.Close();
                        }
                        else
                        {
                            string msg = "Assigned job "+ row.Cells[2].Text + " could not be found!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignee.Focus();
                            con.Close();
                            assignedJob_Click(new object(), new EventArgs());
                            return;
                        }
                        // Then, remove job from OPERATOR_ACCESS
                        SqlCommand cmd3 = new SqlCommand("DELETE FROM OPERATOR_ACCESS WHERE OPERATOR_ACCESS.JOB_ID = @job AND OPERATOR_ACCESS.OPERATOR_ID = @op", con);
                        cmd3.Parameters.AddWithValue("@job", jobID);
                        cmd3.Parameters.AddWithValue("@op", opID);
                        if (cmd3.ExecuteNonQuery() == 1)
                        {
                            count++;
                        }
                        else
                        {
                            string msg = "Job: " + row.Cells[2].Text + " could not be removed! Please try again or contact Tech Support";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            con.Close();
                            assignedJob_Click(new object(), new EventArgs());
                            assignee.Focus();
                            return;
                        }
                    }
                }
                if (count == 0)
                {
                    string msg = "Please select at least one job.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    con.Close();
                    assignedJob_Click(new object(), new EventArgs());
                }
                else
                {
                    string msg = count + " Job(s) Unassigned!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                    con.Close();
                    assignedJob_Click(new object(), new EventArgs());
                }
            }
        }




        // 'GRANT' CLICKED: ASSIGN OPERATORS JOB-ACCESSES. FUNCTION
        protected void jobAccess_Click(object sender, EventArgs e)
        {
            SqlConnection con = Helper.ConnectionObj;
            con.Open();

            string assigneeName = assignee.Text;
            if (assigneeName == string.Empty)
            {
                string msg = "Operator field is required!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                assignee.Focus();
                return;
            }
            else
            {
                if (jobAccessGridView.Rows.Count > 0)
                {
                    int count = 0;
                    // For each checked job, assign job to assignee
                    foreach (GridViewRow row in jobAccessGridView.Rows)
                    {
                        CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;
                        if (chxBox.Checked)
                        {
                            string abbr = row.Cells[2].Text;
                            bool answer = AssignJob(assigneeName, abbr, con); // calling assignJob function
                            if (answer == true)
                            {
                                count++;
                            }
                            else
                            {
                                //assignee.Focus();
                                //getUnassignedJobs();

                            }
                        }
                    }
                    if (count == 0)
                    {
                        string msg = "Please make sure that at least 1 job is selected.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        return;
                    }
                    else
                    {
                        string msg = count + " job(s) successfully assigned.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        getUnassignedJobs(null);
                        assignee.Focus();
                        return;
                    }
                }
                else
                {
                    string msg = "No Unassigned job record could be found.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }

            }
        }


        // 'JOB INDEX CONFIGURATION SECTION' CLICKED: SHOW & INDEX FORM CONTROLS. FUNCTION
        protected void jobIndexEditingShow_Click(object sender, EventArgs e)
        {
            // Fill Job Abb. dropdown list with jobs.
            selectJob.Items.Clear();
            selectJob.Items.Add("Select");
            if (jobIndexEditingPanel.Visible == false)
            {
                jobIndexEditingPanel.Visible = true;
                getActiveJobs();
            }
            else
            {
                jobIndexEditingPanel.Visible = false;
            }
            
        }



        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS. FUNCTION
        protected void onJobSelect(object sender, EventArgs e)
        {
            //setDropdownColor();
        }



        // 'ACTIVE' SELECTED: SET 'ASSIGN TO' VISIBLE OR NOT
        protected void onActiveSelect(object sender, EventArgs e)
        {
            if(jobActiveBtn.Visible && jobActiveBtn.SelectedValue == "1")
            {
                jobAssignedToLabel.Visible = true;
                jobAssignedTo.Visible = true;
            }
            else
            {
                jobAssignedToLabel.Visible = false;
                jobAssignedTo.Visible = false;
            }

        }


        // 'SET' CLICKED: SET INDEX FORM RULES. FUNCTION
        protected void setRules_Click(object sender, EventArgs e)
        {
            SqlConnection con = Helper.ConnectionObj;
            con.Open();

            int jobID = 0;

            // Make sure a job is selected & LABEL1 is filled.
            if (this.selectJob.SelectedValue == "Select")
            {
                string msg = "Please select a specific job!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                jobAbb.Text = string.Empty;
                jobAbb.Focus();
                return;
            }
            else if (this.label1.Text == string.Empty)
            {
                string msg = "LABEL1 is required!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                jobAbb.Text = string.Empty;
                jobAbb.Focus();
                return;
            }
            else
            {   // First, get job ID of selected job
                SqlCommand cmd = new SqlCommand("SELECT ID FROM JOB WHERE ABBREVIATION = @jobAbb", con);
                cmd.Parameters.AddWithValue("@jobAbb", selectJob.SelectedValue);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        jobID = (int)reader.GetValue(0);
                    }
                    reader.Close();
                }
                else
                {
                    string msg = "Job selected could not be found.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    con.Close();
                    selectJob.SelectedValue = "Select";
                    return;
                }

                // Then, use that job ID to set job rules into JOB_CONFIG_INDEX 
                try
                {
                    SqlCommand cmd2 = new SqlCommand("INSERT INTO JOB_CONFIG_INDEX" +
                    "(JOB_ID, LABEL1, REGEX1, LABEL2, REGEX2, LABEL3, REGEX3, LABEL4, REGEX4, LABEL5, REGEX5) " +
                    "VALUES(@jobID, @label1, @regex1, @label2, @regex2, @label3, @regex3, @label4, @regex4, @label5, @regex5)", con);
                    cmd2.Parameters.AddWithValue("@jobID", jobID);
                    cmd2.Parameters.AddWithValue("@label1", label1.Text);
                    if (regex1.Text == string.Empty) cmd2.Parameters.AddWithValue("@regex1", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@regex1", regex1.Text);
                    if (label2.Text == string.Empty) cmd2.Parameters.AddWithValue("@label2", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@label2", label2.Text);
                    if (regex2.Text == string.Empty) cmd2.Parameters.AddWithValue("@regex2", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@regex2", regex2.Text);
                    if (label3.Text == string.Empty) cmd2.Parameters.AddWithValue("@label3", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@label3", label3.Text);
                    if (regex3.Text == string.Empty) cmd2.Parameters.AddWithValue("@regex3", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@regex3", regex3.Text);
                    if (label4.Text == string.Empty) cmd2.Parameters.AddWithValue("@label4", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@label4", label4.Text);
                    if (regex4.Text == string.Empty) cmd2.Parameters.AddWithValue("@regex4", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@regex4", regex4.Text);
                    if (label5.Text == string.Empty) cmd2.Parameters.AddWithValue("@label5", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@label5", label5.Text);
                    if (regex5.Text == string.Empty) cmd2.Parameters.AddWithValue("@regex5", DBNull.Value);
                    else cmd2.Parameters.AddWithValue("@regex5", regex5.Text);

                    if (cmd2.ExecuteNonQuery() == 1)
                    {
                        string msg = selectJob.SelectedValue + " Job config successfully set.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        con.Close();
                        setDropdownColor();
                        clearRules();
                        return;
                    }
                    else
                    {
                        string msg = "Config could not be set. Please contact system admin.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        clearRules();
                        con.Close();
                    }
                }
                catch(SqlException ex) {
                    clearRules();
                    con.Close();
                    string msg = " The job selected has already been configured. If you want to reconfigure, please Unset then Set again!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                }
                

            }
        }



        // 'UNSET' CLICKED: UNSET INPUT-CONTROLS RULES. FUNCTION.
        protected void unsetRules_Click(object sender, EventArgs e)
        {
            SqlConnection con = Helper.ConnectionObj;
            con.Open();
            int jobID = 0;

            // Make sure a job is selected & LABEL1 is filled.
            if (this.selectJob.SelectedValue == "Select")
            {
                string msg = "Please select a specific job!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                jobAbb.Text = string.Empty;
                jobAbb.Focus();
                return;
            }
            else
            {   // First, get job ID of selected job
                SqlCommand cmd = new SqlCommand("SELECT ID FROM JOB WHERE ABBREVIATION = @jobAbb", con);
                cmd.Parameters.AddWithValue("@jobAbb", selectJob.SelectedValue);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        jobID = (int)reader.GetValue(0);
                    }
                    reader.Close();
                }
                else
                {
                    string msg = "Job selected could not be found.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    con.Close();
                    selectJob.SelectedValue = "Select";
                    return;
                }

                // Then, use that job ID to unset job rules into JOB_CONFIG_INDEX 
                try
                {
                    SqlCommand cmd2 = new SqlCommand("DELETE FROM JOB_CONFIG_INDEX WHERE JOB_ID=@jobID", con);
                    cmd2.Parameters.AddWithValue("@jobID", jobID);

                    if (cmd2.ExecuteNonQuery() == 1)
                    {
                        string msg = selectJob.SelectedValue + " Job config successfully unset.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        con.Close();
                        setDropdownColor();
                        clearRules();
                        return;
                    }
                    else
                    {
                        string msg = "Config could not be unset. Please contact system admin.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        clearRules();
                        con.Close();
                    }
                }
                catch (SqlException ex)
                {
                    clearRules();
                    con.Close();
                    string msg = "Configuration rules for the selected job has already been unset. If you want to reconfigure, Make sure it is selected, then Set!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                }
            }
        }


            //--- HELPER FUNCTIONS ------------------------------------------------------------------------------------------------

            // COLLAPSE ALL SECTIONS. HELPER FUNCTION 
            protected void collapseAll_Click(object sender, EventArgs e)
            {
                if (collapseAll.Text == "Collapse All")
                {
                    collapseAll.Text = "Hide All";
                    jobSection.Visible = true;
                    newUserSection.Visible = true;
                    getUnassignedJobs(null);
                    assignPanel.Visible = true;
                    jobIndexEditingPanel.Visible = true;
                    getDropdownJobItems();
                    getActiveJobs();
                    jobSectionDefault();
                    line.Visible = true;
                }
                else
                {
                    collapseAll.Text = "Collapse All";
                    jobSection.Visible = false;
                    newUserSection.Visible = false;
                    assignPanel.Visible = false;
                    jobIndexEditingPanel.Visible = false;
                    line.Visible = false;
                }
            }



        // GET ALL UNASSIGNED JOBS OR OPERATOR'S INACCESSIBLE JOBS. HELPER FUNCTION
        private void getUnassignedJobs(Object sender)
        {
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            DataSet ds = null;
            string assigneeName=string.Empty;
            int opID = 0;
            SqlConnection con = Helper.ConnectionObj;
            Button button = (Button)sender;
            string buttonId = string.Empty;
            if(button != null) buttonId = button.ID;
            con.Open();

            if(button != null && buttonId == "inaccessibleBtn")
            {
                if (assignee.Text != string.Empty) assigneeName = assignee.Text;
                else
                {
                    string msg = "Operator field required.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    con.Close();
                    return;
                }

                // First check if the assignee exists. If so, get ID.
                SqlCommand cmd2 = new SqlCommand("SELECT ID FROM OPERATOR WHERE NAME = @assignedTo", con);
                cmd2.Parameters.AddWithValue("@assignedTo", assigneeName);

                try
                {
                    SqlDataReader reader = cmd2.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            opID = (int)reader.GetValue(0);
                        }
                        reader.Close();
                    }
                    else
                    {
                        string msg = "Operator entered could not be found.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        con.Close();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    string msg = "Error: An error occured. Contact system admin .";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    con.Close();
                    return;
                }
            }


            try
            {
                //if (!Page.IsValid) return;
                if(button != null && buttonId == "inaccessibleBtn")
                {
                    cmd = new SqlCommand("SELECT ABBREVIATION " +
                                         "FROM JOB " +
                                         "WHERE ACTIVE = 1 AND ID NOT IN (SELECT JOB_ID FROM OPERATOR_ACCESS WHERE OPERATOR_ID=@opId)", con);
                    cmd.Parameters.AddWithValue("@opId", opID);
                }
                else
                {
                    cmd = new SqlCommand("SELECT ABBREVIATION " +
                                         "FROM JOB " +
                                         "LEFT JOIN OPERATOR_ACCESS ON JOB.ID = OPERATOR_ACCESS.JOB_ID " +
                                         "WHERE JOB.ACTIVE = 1 AND OPERATOR_ACCESS.JOB_ID IS NULL", con);
                }

                da = new SqlDataAdapter(cmd);
                ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    jobAccessGridView.DataSource = ds.Tables[0];
                    jobAccessGridView.DataBind();
                    jobAccessGridView.Visible = true;
                }
                con.Close();

                // Handling of whether any JOB was returned from DB
                if (jobAccessGridView.Rows.Count == 0)
                {
                    jobAccessGridView.Visible = false;
                    jobAccessBtn.Visible = false;
                    deleteAssignedBtn.Visible = false;
                    if (buttonId == "inaccessibleBtn") jobsLabel.Text = "No Inaccessible Jobs Found.";
                    else jobsLabel.Text = "No Unassigned Jobs Found.";
                    jobsLabel.Visible = true;
                }
                else
                {
                    if (buttonId == "inaccessibleBtn") jobsLabel.Text = "Operator's Currently Inaccessible Jobs.";
                    else jobsLabel.Text = "Jobs Currently Inaccessible to Anyone.";
                    jobsLabel.Visible = true;
                    jobAccessBtn.Visible = true;
                    deleteAssignedBtn.Visible = false;
                }
            }
            catch (SqlException ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
            }
            finally
            {
                if (da != null)
                {
                    da.Dispose();
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (con != null)
                {
                    con.Dispose();
                }
            }
        }



        // Get dropdown list job items
        private void getDropdownJobItems()
        {
            selectJobList.Items.Clear();
            selectJobList.Items.Add("Select");
            SqlConnection con = Helper.ConnectionObj;
            SqlCommand cmd = new SqlCommand("SELECT ABBREVIATION, ACTIVE FROM JOB", con);
            con.Open();

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string jobAbb = (string)reader.GetValue(0);
                    bool active = (bool)reader.GetValue(1);
                    selectJobList.Items.Add(jobAbb);
                    if(active)
                    {
                        // Red config job items from 'JOB' section
                        foreach (ListItem item in selectJobList.Items)
                        {
                            if (item.Value == jobAbb)
                            {
                                item.Attributes.Add("style", "color:Red;");
                            }
                        }
                    }
                    //selectJobList.AutoPostBack = true;
                }
                reader.Close();
            }
            else
            {
                string msg = "Some went wront while getting job abbreviations from JOB";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                con.Close();
                return;
            }

            // Get configured jobs, then Set color in dropdown list.
            con.Close();
        }




        // Get dropdown list ACTIVE job items
        private void getActiveJobs()
        {
            selectJob.Items.Clear();
            selectJob.Items.Add("Select");
            SqlConnection con = Helper.ConnectionObj;
            SqlCommand cmd = new SqlCommand("SELECT ABBREVIATION FROM JOB WHERE ACTIVE = 1", con);
            con.Open();
            
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string jobAbb = (string)reader.GetValue(0);
                    selectJob.Items.Add(jobAbb);
                    //selectJob.AutoPostBack = true;
                }
                reader.Close();
            }
            else
            {
                string msg = "Some went wront while getting active job abbreviations from JOB";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                con.Close();
                return;
            }

            // Get configured jobs, then Set color in dropdown list.
            con.Close();
            setDropdownColor();
        }


        // ASSIGN JOB TO OPERATOR. HELPER FUNCTION
        private bool AssignJob(string assignee, string abbr, SqlConnection con)
        {
            int opID = 0;
            int jobID = 0;
            if (con.State == ConnectionState.Closed) con.Open();

            // First check if the assignee exists. If so, get ID.
            SqlCommand cmd2 = new SqlCommand("SELECT ID FROM OPERATOR WHERE NAME = @assignedTo", con);
            cmd2.Parameters.AddWithValue("@assignedTo", assignee);
            SqlDataReader reader = cmd2.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    opID = (int)reader.GetValue(0);
                }
                reader.Close();
            }
            else
            {
                string msg = "Job(s) not assigned. Specified operator could not be found!";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                con.Close();
                return false;
            }

            // Get job ID
            SqlCommand cmd3 = new SqlCommand("SELECT ID FROM JOB WHERE ABBREVIATION = @abbr", con);
            cmd3.Parameters.AddWithValue("@abbr", abbr);
            SqlDataReader reader2 = cmd3.ExecuteReader();
            if (reader2.HasRows)
            {
                while (reader2.Read())
                {
                    jobID = (int)reader2.GetValue(0);
                }
                reader2.Close();
            }
            else
            {
                string msg = "Job(s) not assigned. The specified Job abbreviation could not be found.";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "')", true);
                con.Close();
                return false;
            }

            // Save assignee ID & new job ID in DB
            if (opID > 0 && jobID > 0)
            {
                SqlCommand cmd4 = new SqlCommand("INSERT INTO OPERATOR_ACCESS (OPERATOR_ID, JOB_ID) VALUES(@opId, @jobID)", con);
                cmd4.Parameters.AddWithValue("@opID", opID);
                cmd4.Parameters.AddWithValue("@jobID", jobID);
                if (cmd4.ExecuteNonQuery() == 1)
                {
                    con.Close();
                    return true;
                }
                else
                {
                    string msg = "Job(s) NOT saved nor assigned. Try again or contact Tech support.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    con.Close();
                    return false;
                }
            }
            return false;
        }



        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS. HELPER FUNCTION
        private void setDropdownColor()
        {
            SqlConnection con = Helper.ConnectionObj;
            con.Open();
            SqlCommand cmd4 = new SqlCommand("SELECT ABBREVIATION " +
                                             "FROM JOB " +
                                             "INNER JOIN JOB_CONFIG_INDEX ON JOB.ID = JOB_CONFIG_INDEX.JOB_ID " +
                                             "WHERE JOB.ACTIVE = 1", con);
            SqlDataReader reader4 = cmd4.ExecuteReader();
            if (reader4.HasRows)
            {
                while (reader4.Read())
                {
                    // Red config job items from 'JOB INDEX CONFIG' section
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
            con.Close();
        }



        // HANDLE NEXT PAGE CLICK. FUNCTION
        protected void pageChange_Click(object sender, GridViewPageEventArgs e)
        {
            jobAccessGridView.PageIndex = e.NewPageIndex;
            if (jobsLabel.Text == "Operator's Currently Accessible Jobs")
            {
                assignedJob_Click(new object(), new EventArgs());
            }
            else
            {
                getUnassignedJobs(null);
            }
        }




        // CLEAR RULES. HELPER FUNCTION
        private void clearRules()
        {
            selectJob.SelectedValue = "Select";
            label1.Text = string.Empty;
            label1.Focus();
            regex1.Text = string.Empty;
            label2.Text = string.Empty;
            regex2.Text = string.Empty;
            label3.Text = string.Empty;
            regex3.Text = string.Empty;
            label4.Text = string.Empty;
            regex4.Text = string.Empty;
            label5.Text = string.Empty;
            regex5.Text = string.Empty;
        }


        // JOB SECTION DEFAULTS. HELPER FUNCTION
        private void jobSectionDefault()
        {
            selectAction.SelectedValue = "create";
            selectJobList.Visible = false;
            deleteJobBtn.Visible = false;
            editJobBtn.Visible = false;

            jobAbb.Visible = true;
            createJobBtn.Visible = true;
            jobNameLabel.Visible = true;
            jobName.Visible = true;
            jobName.Attributes["placeholder"] = " Required";
            jobActiveLabel.Visible = true;
            jobActiveBtn.Visible = true;
            jobActiveBtn.SelectedValue = "1";
            jobAssignedToLabel.Visible = true;
            jobAssignedTo.Visible = true;
        }

    }
}