﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.IO;
using BarcodeConversion.App_Code;
using System.Globalization;

namespace BarcodeConversion
{
    public partial class About : Page
    {

        SqlConnection con = new SqlConnection(@"Data Source=GLORY-PC\SQLEXPRESS;Initial Catalog=ImagePRO;Integrated Security=True");
        protected void Page_Load(object sender, EventArgs e)
        {
            // Get unprinted indexes whenever page loads.
            if (!IsPostBack)
            {
                getUnprintedIndexes_Click(new object(), new EventArgs());
            }
            Control c = GetPostBackControl(this.Page);
            if (c != null && c.ID == "getUnprintedIndexes") indexesGridView.PageIndex = 0;
        }

        // 'RESET' CLICKED: GET UNPRINTED INDEXES. FUCNTION
        protected void getUnprintedIndexes_Click(object sender, EventArgs e)
        {
            Page.Validate();
            string user = Environment.UserName;
            int opID = 0;
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            DataSet ds = null;
            try
            {   
                //Get unprinted indexes from DB
                if (!Page.IsValid) return;
                con.Open();
                opID = getUserId(user, con);
                if (opID == 0)
                {
                    string msg = "You could not be found in our system. Try again or contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
                cmd = new SqlCommand("SELECT BARCODE, JOB_ID, CREATION_TIME FROM INDEX_DATA WHERE OPERATOR_ID=@opId AND PRINTED=0", con);
                cmd.Parameters.AddWithValue("@opId", opID);
                da = new SqlDataAdapter(cmd);
                ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    indexesGridView.DataSource = ds.Tables[0];
                    indexesGridView.DataBind();
                }
                con.Close();

                // Handling of whether any index was returned from DB
                if(indexesGridView.Rows.Count == 0)
                {
                    indexesGridView.Visible = false;
                    getBarcodeBtn.Visible = false;
                    printBarcodeBtn.Visible = false;
                    deleteBtn.Visible = false;
                    string noIndex = "There are no more records of unprinted inxdexes.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + noIndex + "');", true);
                }
                else
                {   
                    getBarcodeBtn.Visible = true;
                    printBarcodeBtn.Visible = true;
                    deleteBtn.Visible = true;
                }

            }
            catch (SqlException ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
                Console.WriteLine(ex.Message);

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



        // 'DELETE INDEXES' CLICKED: DELETE CHECKED INDEXES. FUNCTION
        protected void deleteIndexes_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            var check = 0; 
            var counter = 0;
            string jobDone;

            // Counting the number of selected checkboxes
            foreach (GridViewRow row in indexesGridView.Rows)
            {
                CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;
                if (chxBox.Checked)
                {
                    check++;
                }

            }

            if (check == 0)
            {
                // Warning if no Index was selected
                string warning = "No Index was selected. Please select at least Index to delete.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + warning + "');", true);
            }
            else
            {
                // Deletion of selected index records
                con.Open();

                // First, get current user ID
                string user = Environment.UserName;
                int opID = getUserId(user, con);
                if (opID == 0)
                {
                    string msg = "You could not be found in our system. Try again or contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }

                // Then, delete unprinted barcode indexes of current user
                foreach (GridViewRow row in indexesGridView.Rows)
                {
                    CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;

                    if (chxBox.Checked)
                    {
                        if (row.RowType == DataControlRowType.DataRow)
                        {
                            var indexString = row.Cells[3].Text;
                            SqlCommand cmd = new SqlCommand("DELETE FROM INDEX_DATA WHERE OPERATOR_ID=@opId AND BARCODE = @barcodeIndex", con);
                            cmd.Parameters.AddWithValue("@opId", opID);
                            cmd.Parameters.AddWithValue("@barcodeIndex", indexString);
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                counter++;
                            }
                            else
                            {
                                counter += 1;
                                string msg = "Error: There was a problem deleting selected Index Number " + counter + ".";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            }
                        }
                    }
                }
                if (counter == 1)
                {
                    jobDone = counter + " Index record was deleted.";
                }
                else
                {
                    jobDone = counter + " Index records were deleted.";
                }
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + jobDone + "');", true);
                con.Close();
                getUnprintedIndexes_Click(new object(), new EventArgs());
            }         
        }



        // 'MASTER CHECKBOX' SELECTED: CHECKBOX THAT SETS ALL THE OTHERS. FUNCTION
        protected void selectAll_changed(object sender, EventArgs e)
        {
            CheckBox ChkBoxHeader = (CheckBox)indexesGridView.HeaderRow.FindControl("selectAll");
            foreach (GridViewRow row in indexesGridView.Rows)
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




        // 'SHOW BARCODE' CLICKED: GET BARCODES FOR SELECTED INDEXES. FUNCTION
        protected void getBarcode_Click(object sender, EventArgs e)
        {
            bool boxChecked = false;
            foreach (GridViewRow row in indexesGridView.Rows)
            {
                var imgBarCode = row.FindControl("imgBarCode") as System.Web.UI.WebControls.Image;
                CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;

                if (chxBox.Checked)
                {
                    boxChecked = true;
                    indexesGridView.HeaderRow.Cells[2].Text = "&nbsp;&nbsp;&nbsp;Barcode";

                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        var indexBarcode = row.Cells[3].Text;
                        imgBarCode.ImageUrl = string.Format("ShowCode39BarCode.ashx?code={0}&ShowText=0&Height=50", indexBarcode.PadLeft(8, '0'));
                    }
                }
                else
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                       // var indexString = row.Cells[3].Text;
                        imgBarCode.ImageUrl = "";
                    }
                }
            }
            if (boxChecked == false)
            {
                string msg = "To view barcode, please select at least one index";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
            }
        }



        // 'PRINT BARCODE' CLICKED: PRINT INDEX BARCODES SHEETS FOR SELECTED INDEXES. FUNCTION
        protected void printBarcode_Click(object sender, EventArgs e)
        {
            // Hide all current html.
            unprintedIndexesPanel.Visible = false;
            bool boxChecked = false;
            if (!Page.IsValid) return;

            // Creating index barcode webpage
            Response.Write("<div id = 'pageToPrint' style = 'margin-top:-50px;'>");
            foreach (GridViewRow row in indexesGridView.Rows)
            {
                var indexString = row.Cells[3].Text;
                var imgBarCode = row.FindControl("imgBarCode") as System.Web.UI.WebControls.Image;
                CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;
                List<EntryContent> allEntriesList = new List<EntryContent>();

                if (chxBox.Checked)
                {   
                    boxChecked = true;
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        // Get barcode Image
                        string urlString = @"ShowCode39BarCode.ashx?code={0}&ShowText=1&Height=50";
                        imgBarCode.ImageUrl = string.Format(urlString, indexString.PadLeft(8, '0'));

                        // Get operator's entries
                        con.Open();
                        SqlCommand cmd = new SqlCommand("SELECT LABEL1, VALUE1, LABEL2, VALUE2, LABEL3, VALUE3," +
                            " LABEL4, VALUE4, LABEL5, VALUE5 FROM JOB_CONFIG_INDEX" +
                            " INNER JOIN INDEX_DATA ON JOB_CONFIG_INDEX.JOB_ID = INDEX_DATA.JOB_ID" +
                            " WHERE INDEX_DATA.BARCODE = @indexString", con);
                        cmd.Parameters.AddWithValue("@indexString", indexString);
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            int i = 0;
                            while (reader.Read())
                            {
                                int count  = reader.FieldCount;
                                while (i < count-1)
                                {
                                    if (reader.GetValue(i) != DBNull.Value && reader.GetValue(i + 1) != DBNull.Value) {
                                        EntryContent content = new EntryContent((string)reader.GetValue(i), (string)reader.GetValue(i + 1));
                                        allEntriesList.Add(content);
                                    }
                                    i = i + 2;
                                }
                            }
                            reader.Close();
                            ViewState["allEntriesList"] = allEntriesList;
                        }
                        else
                        {
                            // Handle this.
                        }
                        con.Close();

                        // Write to index page
                        Response.Write(
                               "<div>" +
                                   "<div style='font-size:25px; font-weight:500;'>" +
                                       "<img src='" + imgBarCode.ImageUrl + "' height='160px' width='500px' style='margin-top:0px; '> " +
                                   "</div>" +
                                   "<div style='font-size:25px; font-weight:500; text-align:right;' >" +
                                       "<img src='" + imgBarCode.ImageUrl + "' height='160px' width='500px' style='margin-top:250px; margin-right:-180px;' class='rotate'> " +
                                   "</div>" +
                               "</div>" +

                               "<table style='margin-top:250px; margin-bottom:580px; margin-left:40px;'>" +
                                   "<tr>" +
                                       "<td style='font-size:25px; font-weight:500;'> Index String: </td>" +
                                       "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + indexString.ToUpper() + "</td>" +
                                   "</tr>"
                        );

                        foreach (var entry in allEntriesList)
                        {   
                            string label = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(entry.labelText.ToLower());
                            Response.Write(
                                "<tr>" +
                                    "<td style='font-size:25px; font-weight:500;'>" + label + ": </h2></td>" +
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
                    }
                    else
                    {
                        if (row.RowType == DataControlRowType.DataRow)
                        {
                            var indexBarcode = row.Cells[3].Text;
                            imgBarCode.ImageUrl = "";
                        }
                    }
                }
            }
            Response.Write("</div>");

            // Handling of whether any checkbox was checked
            if (boxChecked == false)
            {
                string msg = "To print barcode, please select at least one index";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                unprintedIndexesPanel.Visible = true;
                return;
            }

            try
            {   
                // Print generated Index sheets wepages, clear & get unprinted indexes again.
                ClientScript.RegisterStartupScript(this.GetType(), "PrintOperation", "printing();", true);
                //unprintedIndexesPanel.Visible = true;
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
            }
        }



        // SETTING INDEX AS PRINTED IN DB. FUNCTION
        protected void setIndexAsPrinted_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            var counter = 0;
            
            con.Open();

            foreach (GridViewRow row in indexesGridView.Rows)
            {
                CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;

                if (chxBox.Checked)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        var indexString = row.Cells[3].Text;                        
                        SqlCommand cmd = new SqlCommand("UPDATE INDEX_DATA SET Printed = 1 WHERE BARCODE = @barcodeIndex", con);
                        cmd.Parameters.AddWithValue("@barcodeIndex", indexString);
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            counter++;
                        }
                        else
                        {
                            string msg = "There was an unexpected error. Make sure there are no duplicate indexes on records. Please Try again. If issue persists, contact your tech support.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }
                    }
                }             
            }
            // Confirmation msg & back to unprinted indexes gridview
            string jobDone;
            if(counter == 1)
            {
                jobDone = counter + " Index record was updated and set as PRINTED.";
            }
            else
            {
                jobDone = counter + " Indexes records were updated and set as PRINTED.";
            }
            
            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + jobDone + "');", true);
            getUnprintedIndexes_Click(new object(), new EventArgs());
        }

        // SET PRINTED INDEXES AS PRINTED IN DB. HERLPER FUNCTION
        protected void setAsPrinted_Click(object sender, EventArgs e)
        {
            unprintedIndexesPanel.Visible = true;
            setIndexAsPrinted_Click(new object(), new EventArgs());
        }

        protected void getUnprinted_Click(object sender, EventArgs e)
        {
            unprintedIndexesPanel.Visible = true;
            getUnprintedIndexes_Click(new object(), new EventArgs());
        }

        // HANDLE NEXT PAGE CLICK. FUNCTION
        protected void pageChange_Click(object sender, GridViewPageEventArgs e)
        {
            indexesGridView.PageIndex = e.NewPageIndex;
            getUnprintedIndexes_Click(new object(), new EventArgs());
        }


        // GET USER ID VIA USERNAME. HELPER FUNCTION
        private int getUserId(string user, SqlConnection con)
        {
            int opID = 0;
            SqlCommand cmd = new SqlCommand("SELECT ID FROM OPERATOR WHERE NAME = @username", con);
            cmd.Parameters.AddWithValue("@username", user);
            SqlDataReader reader = cmd.ExecuteReader();
            if(reader.HasRows)
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



        // PREVENT LINE BREAKS IN GRIDVIEW
        protected void rowDataBound(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < e.Row.Cells.Count; i++)
            {
                e.Row.Cells[i].Attributes.Add("style", "white-space: nowrap;");
            }
        }


        // GET CONTROL THAT FIRED POSTBACK. HELPER FUNCTION.
        public static Control GetPostBackControl(Page page)
        {
            Control control = null;

            string ctrlname = page.Request.Params.Get("__EVENTTARGET");
            if (ctrlname != null && ctrlname != string.Empty)
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }
            }
            return control;
        }
    }
}