using BarcodeConversion.App_Code;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BarcodeConversion
{
    public partial class IndexStatus : System.Web.UI.Page
    {
        SqlConnection con = Helper.ConnectionObj;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                getIndexes("meOnly", "allTime", "allSheets");
                indexeStatusGridView.Visible = true;
            }
            // Make date fields entries persist
            from.Attributes.Add("readonly", "readonly");
            to.Attributes.Add("readonly", "readonly");
            // Reset gridview page
            Control c = Helper.GetPostBackControl(this.Page);
            if (c != null && (c.ID == "reset" || c.ID == "whoFilter" || c.ID == "whenFilter" ||
                c.ID == "whatFilter" || c.ID == "recordsPerPage")) indexeStatusGridView.PageIndex = 0;
        }



        // 'RESET' CLICKED: RESET FILTER TO DEFAULT VALUES. FUNCTION
        protected void reset_Click(object sender, EventArgs e)
        {
            whoFilter.SelectedValue = "meOnly";
            whenFilter.SelectedValue = "allTime";
            whatFilter.SelectedValue = "allSheets";
            getIndexes("meOnly", "allTime", "allSheets");
            indexeStatusGridView.Visible = true;
            sortOrder.Text = "Sorted By : CREATION_TIME ASC (Default)";
        }


        // 'WHO' & 'WHAT' FILTER CHANGED.
        protected void onSelectedChange(object sender, EventArgs e)
        {
            getIndexes(whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
        }

        // 'WHEN' FILTER CHANGED.
        protected void onSelectWhen(object sender, EventArgs e)
        {

            if (whenFilter.SelectedValue == "allTime")
            {
                timePanel.Visible = false;
                getIndexes(whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
                indexeStatusGridView.Visible = true;
            }
            else
            {
                timePanel.Visible = true;
                gridHeader.Visible = false;
                indexeStatusGridView.Visible = false;
            }
        }



        // 'SUBMIT' CLICKED: DATE FIELDS ENTERED.
        protected void submit_Click(object sender, EventArgs e)
        {
            getIndexes(whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
        }


        
        // HANDLE NEXT PAGE CLICK. FUNCTION
        protected void pageChange_Click(object sender, GridViewPageEventArgs e)
        {
            indexeStatusGridView.PageIndex = e.NewPageIndex;
            getIndexes(whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
        }



        // GET FILTERED INDEXES. HELPER FUNCTION
        protected void getIndexes(string who, string when, string what)
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
                opID = Helper.getUserId(user, con);
                if (opID == 0)
                {
                    description.Text = "No indexes found with the specified filter entries.";
                    recordsPerPageLabel.Visible = false;
                    recordsPerPage.Visible = false;
                    sortOrder.Visible = false;
                    return;
                }
                string cmdString =  "SELECT NAME, JOB_ID, BARCODE, CREATION_TIME, PRINTED " +
                                    "FROM INDEX_DATA " +
                                    "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID=OPERATOR.ID WHERE ";

                if(who == "meOnly")
                {
                    if(when == "allTime")
                    {
                        timePanel.Visible = false;
                        if (what == "allSheets")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId", con);
                            description.Text = "Your Indexes for all Time.";
                        }
                        else if (what == "printed")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID =@opId AND PRINTED=1", con);
                            description.Text = "Your Printed Indexes for all Time.";
                        }
                        else if (what == "notPrinted")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=0", con);
                            description.Text = "Your Unprinted Indexes for all Time.";
                        }
                    }
                    else if(when == "pickRange")
                    {
                        DateTime start = DateTime.Parse(Request.Form[from.UniqueID]);
                        DateTime end = DateTime.Parse(Request.Form[to.UniqueID]);
                        if (start == default(DateTime))
                        {
                            string msg = "Please pick a start date.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return;
                        }
                        if(end == default(DateTime))
                        {
                            string msg = "Please pick an end date.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return;
                        }
                        
                        if (what == "allSheets")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND (CREATION_TIME BETWEEN @start AND @end)  ", con);
                            description.Text = "Your Indexes from "+start+" to "+end+".";
                        }
                        else if (what == "printed")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                            description.Text = "Your Printed Indexes from " + start + " to " + end + ".";
                        }
                        else if (what == "notPrinted")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                            description.Text = "Your Unprinted Indexes from " + start + " to " + end + ".";
                        }
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);
                    }
                    cmd.Parameters.AddWithValue("@opId", opID);
                }
                else if(who == "everyone")
                {
                    if (when == "allTime")
                    {
                        timePanel.Visible = false;
                        if (what == "allSheets")
                        {
                            string cmdStringShort = cmdString;
                            cmd = new SqlCommand(cmdStringShort.Substring(0, cmdString.Length - 7), con);
                            description.Text = "Operators' Indexes for all Time.";
                        }
                        else if (what == "printed")
                        {
                            cmd = new SqlCommand(cmdString + "PRINTED=1", con);
                            description.Text = "Operators' Printed Indexes for all Time.";
                        }
                        else if (what == "notPrinted")
                        {
                            cmd = new SqlCommand(cmdString + "PRINTED=0", con);
                            description.Text = "Operators' Unprinted Indexes for all Time.";
                        }
                    }
                    else if (when == "pickRange")
                    {
                        DateTime start = DateTime.Parse(Request.Form[from.UniqueID]);
                        DateTime end = DateTime.Parse(Request.Form[to.UniqueID]);
                        if (start == default(DateTime))
                        {
                            string msg = "Please pick a start date.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return;
                        }
                        if (end == default(DateTime))
                        {
                            string msg = "Please pick an end date.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return;
                        }

                        if (what == "allSheets")
                        {
                            cmd = new SqlCommand(cmdString + "CREATION_TIME BETWEEN @start AND @end", con);
                            description.Text = "Operators' Indexes from " + start + " to " + end + ".";
                        }
                        else if (what == "printed")
                        {
                            cmd = new SqlCommand(cmdString + "PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                            description.Text = "Operators' Printed Indexes From " + start + " to " + end + ".";
                        }
                        else if (what == "notPrinted")
                        {
                            cmd = new SqlCommand(cmdString + "PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                            description.Text = "Operators' Unprinted Indexes from " + start + " to " + end + ".";
                        }
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);
                    }
                }
                da = new SqlDataAdapter(cmd);
                ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    //Persist the table in the Session object.
                    Session["TaskTable"] = ds.Tables[0];

                    indexeStatusGridView.DataSource = ds.Tables[0];
                    indexeStatusGridView.DataBind();
                }
                con.Close();

                // Handling of whether any index was returned from DB
                if (indexeStatusGridView.Rows.Count == 0)
                {
                    description.Text = "No indexes found with the specified filter entries.";
                    gridHeader.Visible = true;
                    recordsPerPageLabel.Visible = false;
                    recordsPerPage.Visible = false;
                    sortOrder.Visible = false;
                }
                else
                {
                    gridHeader.Visible = true;
                    recordsPerPageLabel.Visible = true;
                    recordsPerPage.Visible = true;
                    sortOrder.Visible = true;
                    indexeStatusGridView.Visible = true;
                }
                
            }
            catch (SqlException ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Date fields required! " + ex.Message + "');", true);
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




        // RECORDS PER PAGE
        protected void onSelectedRecordsPerPage(object sender, EventArgs e)
        {
            indexeStatusGridView.PageSize = Int32.Parse(recordsPerPage.SelectedValue);
            getIndexes(whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
            sortOrder.Text = "Sorted By : CREATION_TIME ASC (Default)";
        }



        // PREVENT LINE BREAKS IN GRIDVIEW
        protected void rowDataBound(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < e.Row.Cells.Count; i++)
            {
                e.Row.Cells[i].Attributes.Add("style", "white-space: nowrap;");
            }

            // GIVE CUSTOM COLUMN NAMES
            if (e.Row.RowType == DataControlRowType.Header)
            {
                //e.Row.Cells[1].Text = "OPERATOR";
                //e.Row.Cells[2].Text = "JOB_ID";
                //e.Row.Cells[3].Text = "INDEX";
            }
        }


        
        // SORT ANY GRIDVIEW COLUMN. 
        protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            //Retrieve the table from the session object.
            DataTable dt = Session["TaskTable"] as DataTable;

            if (dt != null)
            {
                //Sort the data.
                dt.DefaultView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
                sortOrder.Text = "Sorted By : " + dt.DefaultView.Sort;
                indexeStatusGridView.DataSource = Session["TaskTable"];
                indexeStatusGridView.DataBind();
            }
        }


        // GET SORTING ORDER
        private string GetSortDirection(string column)
        {
            // By default, set the sort direction to ascending.
            string sortDirection = "ASC";

            // Retrieve the last column that was sorted.
            string sortExpression = ViewState["SortExpression"] as string;

            if (sortExpression != null)
            {
                // Check if the same column is being sorted.
                // Otherwise, the default value can be returned.
                if (sortExpression == column)
                {
                    string lastDirection = ViewState["SortDirection"] as string;
                    if ((lastDirection != null) && (lastDirection == "ASC"))
                    {
                        sortDirection = "DESC";
                    }
                }
            }

            // Save new values in ViewState.
            ViewState["SortDirection"] = sortDirection;
            ViewState["SortExpression"] = column;

            return sortDirection;
        }

    }
}