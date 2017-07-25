using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Security.Principal;

namespace BarcodeConversion
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // SHOW 'SETTINGS' BUTTON IF ADMIN. IF NEW, SAVE USER.
            bool isAdmin = false;
            try
            {
                string user = Environment.UserName;
                if (user != null)
                {   
                    // If user exists, get Admin status
                    SqlConnection con = new SqlConnection(@"Data Source=GLORY-PC\SQLEXPRESS;Initial Catalog=ImagePRO;Integrated Security=True");
                    con.Open();

                    SqlCommand cmd = new SqlCommand("SELECT ADMIN FROM OPERATOR WHERE NAME = @user", con);
                    cmd.Parameters.AddWithValue("@user", user);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            isAdmin = (bool)reader.GetValue(0);
                        }
                        reader.Close();
                    }
                    else
                    {    
                        // If user doesn't exist, register user and set Admin status to operator.
                        string msg;
                        SqlCommand cmd2 = new SqlCommand("INSERT INTO OPERATOR (NAME, ADMIN) VALUES(@user,@admin)", con);
                        cmd2.Parameters.AddWithValue("@user", user);
                        cmd2.Parameters.AddWithValue("@admin", 0);

                        if (cmd2.ExecuteNonQuery() == 1)
                        {
                            //TBD
                            msg = "New user saved!";
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('"+msg+"')", true);
                        }
                        else
                        {
                            //TBD
                            msg = "Failed to save new user!";
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "')", true);
                        }
                    }                 
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                //string msg = "You've not been found into our system. Contact the system admin.";
                Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "\nYou've not been found into our system. Contact your System Admin." + "');", true);
            }
            if (isAdmin) settings.Visible = true;

        }

    }
}