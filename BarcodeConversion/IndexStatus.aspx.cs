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
            if (c != null && (c.ID == "reset" || c.ID == "whoFilter" || c.ID == "whenFilter" || c.ID == "whatFilter")) indexeStatusGridView.PageIndex = 0;
        }



        // 'RESET' CLICKED: RESET FILTER TO DEFAULT VALUES. FUNCTION
        protected void reset_Click(object sender, EventArgs e)
        {
            whoFilter.SelectedValue = "meOnly";
            whenFilter.SelectedValue = "allTime";
            whatFilter.SelectedValue = "allSheets";
            getIndexes("meOnly", "allTime", "allSheets");
            indexeStatusGridView.Visible = true;
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
                description.Visible = false;
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
                    string msg = "You could not be found in our system. Try again or contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
                string cmdString =  "SELECT NAME, JOB_ID, BARCODE, CREATION_TIME, PRINTED " +
                                    "FROM INDEX_DATA " +
                                    "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID = OPERATOR.ID WHERE ";

                if(who == "meOnly")
                {
                    if(when == "allTime")
                    {
                        timePanel.Visible = false;
                        if (what == "allSheets")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId", con);
                            description.Text = "Your Indexes For All Time, Printed or Not.";
                        }
                        else if (what == "printed")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID =@opId AND PRINTED=1", con);
                            description.Text = "Your Printed Indexes For All Time.";
                        }
                        else if (what == "notPrinted")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=0", con);
                            description.Text = "Your Unprinted Indexes For All Time.";
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
                            description.Text = "Your Indexes Between "+start+" and "+end+", Printed or Not.";
                        }
                        else if (what == "printed")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                            description.Text = "Your Printed Indexes Between " + start + " and " + end + ".";
                        }
                        else if (what == "notPrinted")
                        {
                            cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                            description.Text = "Your Unprinted Indexes Between " + start + " and " + end + ".";
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
                            description.Text = "All Operators' Indexes For All Time, Printed or Not.";
                        }
                        else if (what == "printed")
                        {
                            cmd = new SqlCommand(cmdString + "PRINTED=1", con);
                            description.Text = "All Operators' Printed Indexes For All Time.";
                        }
                        else if (what == "notPrinted")
                        {
                            cmd = new SqlCommand(cmdString + "PRINTED=0", con);
                            description.Text = "All Operators' Unprinted Indexes For All Time.";
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
                            description.Text = "All Operators' Indexes Between " + start + " and " + end + ", Printed or Not.";
                        }
                        else if (what == "printed")
                        {
                            cmd = new SqlCommand(cmdString + "PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                            description.Text = "All Operators' Printed Indexes Between " + start + " and " + end + ".";
                        }
                        else if (what == "notPrinted")
                        {
                            cmd = new SqlCommand(cmdString + "PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                            description.Text = "All Operators' Unprinted Indexes Between " + start + " and " + end + ".";
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
                    indexeStatusGridView.DataSource = ds.Tables[0];
                    indexeStatusGridView.DataBind();
                }
                con.Close();

                // Handling of whether any index was returned from DB
                if (indexeStatusGridView.Rows.Count == 0)
                {
                    string noIndex = "No index records found.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + noIndex + "');", true);
                    description.Text = "No indexes found within the specified dates.";
                }
                else
                {
                    description.Visible = true;
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
        


        // PREVENT LINE BREAKS IN GRIDVIEW
        protected void rowDataBound(object sender, GridViewRowEventArgs e)
        {
            for (int i = 0; i < e.Row.Cells.Count; i++)
            {
                e.Row.Cells[i].Attributes.Add("style", "white-space: nowrap;");
            }
        }

    }
}