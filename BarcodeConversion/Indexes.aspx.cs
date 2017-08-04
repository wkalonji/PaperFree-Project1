using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using BarcodeConversion.App_Code;
using System.Globalization;

namespace BarcodeConversion
{
    public partial class About : Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            // Get unprinted indexes whenever page loads.
            if (!IsPostBack)
            {
                getUnprintedIndexes_Click(new object(), new EventArgs());
            }
            Control c = Helper.GetPostBackControl(this.Page);
            if (c != null && (c.ID == "getUnprintedIndexes" || c.ID == "recordsPerPage")) indexesGridView.PageIndex = 0;
        }



        // 'RESET' CLICKED: GET UNPRINTED INDEXES. 
        protected void getUnprintedIndexes_Click(object sender, EventArgs e)
        {
            try
            {
                Page.Validate();
                if (!Page.IsValid) return;
                string user = Environment.UserName;
                int opID = 0;

                // Retrieve operator ID
                opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    string msg = " Error-20: Could not identify this computer. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }

                //Get unprinted indexes from DB
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText =   "SELECT BARCODE, ABBREVIATION, CREATION_TIME " +
                                            "FROM INDEX_DATA " +
                                            "INNER JOIN JOB ON INDEX_DATA.JOB_ID = JOB.ID " +
                                            "WHERE OPERATOR_ID=@opId AND PRINTED=0";
                        cmd.Parameters.AddWithValue("@opId", opID);
                        con.Open();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            using (DataSet ds = new DataSet())
                            {
                                da.Fill(ds);
                                if (ds.Tables.Count > 0)
                                {
                                    //Persist the table in the Session object.
                                    Session["Table"] = ds.Tables[0];
                                    sortOrder.Text = "Sorted By : CREATION_TIME ASC (Default)";
                                    indexesGridView.DataSource = ds.Tables[0];
                                    indexesGridView.DataBind();
                                }
                            }
                        }
                    }
                }

                // Handling of whether any index was returned from DB
                if (indexesGridView.Rows.Count == 0)
                {
                    indexesGridView.Visible = false;
                    getBarcodeBtn.Visible = false;
                    printBarcodeBtn.Visible = false;
                    deleteBtn.Visible = false;
                    recordsPerPage.Visible = false;
                    recordsPerPageLabel.Visible = false;
                    sortOrder.Visible = false;
                    description.Text = "You have no records of unprinted indexes";
                }
                else
                {
                    getBarcodeBtn.Visible = true;
                    printBarcodeBtn.Visible = true;
                    deleteBtn.Visible = true;
                    recordsPerPage.Visible = true;
                    recordsPerPageLabel.Visible = true;
                    sortOrder.Visible = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error-21: Issue occured while attempting to reset page. Contact system admin." + System.Environment.NewLine + ex.Message);
            }
        }



        // 'DELETE INDEXES' CLICKED: DELETE CHECKED INDEXES. FUNCTION 
        protected void deleteIndexes_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            SqlConnection con = Helper.ConnectionObj;
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
                int opID = Helper.getUserId(user);
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
                            try {
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
                            catch(Exception ex) {
                                counter += 1;
                                string msg = "Error: There was a problem deleting selected Index Number " + counter + ".";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                Console.WriteLine(ex.Message);
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
            try
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
            catch (Exception ex)
            {
                throw new Exception("Error-22: Issue occured while attempting to handle checkboxes. Contact system admin." + System.Environment.NewLine + ex.Message);
            }
        }




        // 'SHOW BARCODE' CLICKED: GET BARCODES FOR SELECTED INDEXES. 
        protected void getBarcode_Click(object sender, EventArgs e)
        {
            try
            {
                bool boxChecked = false;
                foreach (GridViewRow row in indexesGridView.Rows)
                {
                    var imgBarCode = row.FindControl("imgBarCode") as System.Web.UI.WebControls.Image;
                    CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;

                    if (chxBox.Checked)
                    {
                        boxChecked = true;
                        indexesGridView.HeaderRow.Cells[2].Text = "&nbsp;&nbsp;&nbsp;BARCODE IMAGE";

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
            catch (Exception ex)
            {
                throw new Exception("Error-22: Issue occured while attempting to handle checkboxes. Contact system admin." + System.Environment.NewLine + ex.Message);
            }
        }


        // 'PRINT BARCODE' CLICKED: PRINT INDEX BARCODES SHEETS FOR SELECTED INDEXES. 
        protected void printBarcode_Click(object sender, EventArgs e)
        {
            try
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
                    var imgBarCode = row.FindControl("imgBarCode") as Image;
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
                            using (SqlConnection con = Helper.ConnectionObj)
                            {
                                using (SqlCommand cmd = con.CreateCommand())
                                {
                                    cmd.CommandText =   "SELECT LABEL1, VALUE1, LABEL2, VALUE2, LABEL3, VALUE3," +
                                                        " LABEL4, VALUE4, LABEL5, VALUE5 FROM JOB_CONFIG_INDEX" +
                                                        " INNER JOIN INDEX_DATA ON JOB_CONFIG_INDEX.JOB_ID = INDEX_DATA.JOB_ID" +
                                                        " WHERE INDEX_DATA.BARCODE = @indexString";
                                    cmd.Parameters.AddWithValue("@indexString", indexString);
                                    con.Open();
                                    using (SqlDataReader reader = cmd.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            int i = 0;
                                            while (reader.Read())
                                            {
                                                int count = reader.FieldCount;
                                                while (i < count - 1)
                                                {
                                                    if (reader.GetValue(i) != DBNull.Value && reader.GetValue(i + 1) != DBNull.Value)
                                                    {
                                                        EntryContent content = new EntryContent((string)reader.GetValue(i), (string)reader.GetValue(i + 1));
                                                        allEntriesList.Add(content);
                                                    }
                                                    i = i + 2;
                                                }
                                            }
                                            ViewState["allEntriesList"] = allEntriesList;
                                        }
                                        else
                                        {
                                            // Handle this
                                        }
                                    }
                                }
                            }

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
                                        "<td style='font-size:25px; font-weight:500;'>" + label + ":" + "</td>" +
                                        "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + entry.text.ToUpper() + "</td>" +
                                    "</tr>"
                                );
                            }
                            Response.Write(
                                        "<tr>" +
                                            "<td style='font-size:25px; font-weight:500;'>Date Created: </td>" +
                                            "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + DateTime.Now + "</td>" +
                                        "</tr>" +
                                    "</table>" +
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
            }
            catch (Exception ex)
            {
                throw new Exception("Error-23: Issue occured while attempting to setup for printing. Contact system admin." 
                                        + System.Environment.NewLine + ex.Message);
            }
           

            try
            {   
                // Print generated Index sheets wepages, clear & get unprinted indexes again.
                ClientScript.RegisterStartupScript(this.GetType(), "PrintOperation", "printing();", true);
                //unprintedIndexesPanel.Visible = true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error-24: Issue occured while attempting to print. Contact system admin." + System.Environment.NewLine + ex.Message);
            }
        }



        // SET INDEX AS PRINTED IN DB. FUNCTION
        protected void setIndexAsPrinted_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsValid) return;
                var counter = 0;
                using (SqlConnection con = Helper.ConnectionObj) 
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE INDEX_DATA SET Printed = 1 WHERE BARCODE = @barcodeIndex";
                        foreach (GridViewRow row in indexesGridView.Rows)
                        {
                            CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;

                            if (chxBox.Checked)
                            {
                                if (row.RowType == DataControlRowType.DataRow)
                                {
                                    var indexString = row.Cells[3].Text;
                                    cmd.Parameters.AddWithValue("@barcodeIndex", indexString);
                                    con.Open();
                                    if (cmd.ExecuteNonQuery() == 1)
                                    {
                                        counter++;
                                    }
                                    else
                                    {
                                        string msg = "Error-25: Couldn't update Index to PRINTED. Contact system admin.";
                                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    }
                                }
                            }
                        }
                    }
                }

                // Confirmation msg & back to unprinted indexes gridview
                string jobDone;
                if (counter == 1)
                {
                    jobDone = counter + " index record was updated and set as PRINTED.";
                }
                else
                {
                    jobDone = counter + " index records were updated and set as PRINTED.";
                }
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + jobDone + "');", true);
                getUnprintedIndexes_Click(new object(), new EventArgs());
            }
            catch (Exception ex)
            {
                throw new Exception("Error-26: Issue occured while attempting to set Index to PRINTED. Contact system admin." 
                                    + System.Environment.NewLine + ex.Message);
            }
        }



        // RECORDS PER PAGE
        protected void onSelectedRecordsPerPage(object sender, EventArgs e)
        {
            try
            {
                indexesGridView.PageSize = Int32.Parse(recordsPerPage.SelectedValue);
                getUnprintedIndexes_Click(new object(), new EventArgs());
                sortOrder.Text = "Sorted By : CREATION_TIME ASC (Default)";
            }
            catch (Exception ex)
            {
                throw new Exception("Error-27: Issue occured while attempting to process request of change in records per page. Contact system admin."
                                     + System.Environment.NewLine + ex.Message);
            }
        }



        // SET PRINTED INDEXES AS PRINTED IN DB. HERLPER FUNCTION
        protected void setAsPrinted_Click(object sender, EventArgs e)
        {
            try
            {
                unprintedIndexesPanel.Visible = true;
                setIndexAsPrinted_Click(new object(), new EventArgs());
            }
            catch (Exception ex)
            {
                throw new Exception("Error-28: Issue occured while attempting to set Index to PRINTED. Contact system admin." 
                                    + System.Environment.NewLine + ex.Message);
            }
        }

        protected void getUnprinted_Click(object sender, EventArgs e)
        {
            try
            {
                unprintedIndexesPanel.Visible = true;
                getUnprintedIndexes_Click(new object(), new EventArgs());
            }
            catch (Exception ex)
            {
                throw new Exception("Error-29: Issue occured while attempting to get unprinted indexes. Contact system admin." 
                                     + System.Environment.NewLine + ex.Message);
            }
        }



        // HANDLE NEXT PAGE CLICK. FUNCTION
        protected void pageChange_Click(object sender, GridViewPageEventArgs e)
        {   
            try
            {
                indexesGridView.PageIndex = e.NewPageIndex;
                getUnprintedIndexes_Click(new object(), new EventArgs());
            }
            catch (Exception ex)
            {
                throw new Exception("Error-30: Issue occured while attempting to change table page" + System.Environment.NewLine + ex.Message);
            }
        }


        // PREVENT LINE BREAKS IN GRIDVIEW
        protected void rowDataBound(object sender, GridViewRowEventArgs e)
        {   
            try 
            {
                for (int i = 0; i < e.Row.Cells.Count; i++)
                {
                    e.Row.Cells[i].Attributes.Add("style", "white-space: nowrap;");
                }

                // GIVE CUSTOM COLUMN NAMES
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    //e.Row.Cells[3].Text = "INDEX";
                    //e.Row.Cells[4].Text = "JOB";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error-31: Issue occured while attempting to prevent line breaks in table. Contact system admin." 
                                    + System.Environment.NewLine + ex.Message);
            }
        }  


        // SORT ANY GRIDVIEW COLUMN. 
        protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
        {   
            try 
            {
                //Retrieve the table from the session object.
                DataTable dt = Session["Table"] as DataTable;

                if (dt != null)
                {
                    //Sort the data.
                    dt.DefaultView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
                    sortOrder.Text = "Sorted By : " + dt.DefaultView.Sort;
                    indexesGridView.DataSource = Session["Table"];
                    indexesGridView.DataBind();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error-32: Issue occured while attempting to sort column. Contact system admin." 
                                    + System.Environment.NewLine + ex.Message);
            }
        }


        // GET SORTING ORDER
        private string GetSortDirection(string column)
        {
            try 
            {
                // By default, set the sort direction to ascending.
                string sortDirection = "ASC";

                // Retrieve the last column that was sorted.
                string sortExpression = ViewState["SortExpres"] as string;

                if (sortExpression != null)
                {
                    // Check if the same column is being sorted. Otherwise, the default value can be returned.
                    if (sortExpression == column)
                    {
                        string lastDirection = ViewState["SortDirect"] as string;
                        if ((lastDirection != null) && (lastDirection == "ASC"))
                        {
                            sortDirection = "DESC";
                        }
                    }
                }

                // Save new values in ViewState.
                ViewState["SortDirect"] = sortDirection;
                ViewState["SortExpres"] = column;
                return sortDirection;
            }
            catch (Exception ex)
            {
                throw new Exception("Error-33: Issue occured while attempting to sort column. Contact system admin." 
                                    + System.Environment.NewLine + ex.Message);
            }
        }
    }
}
