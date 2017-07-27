using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace BarcodeConversion.App_Code
{
    public class Helper
    {
        // GET CONNECTION OBJECT. FUNCTION
        public static SqlConnection ConnectionObj
        {
            get
            {
                SqlConnection con = new SqlConnection(@"Data Source=GLORY-PC\SQLEXPRESS;Initial Catalog=ImagePRO;Integrated Security=True");
                return con;
            }
        }


        // GET USER ID VIA USERNAME. HELPER FUNCTION
        public static int getUserId(string user, SqlConnection con)
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