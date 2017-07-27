<%@ Page Title="Index Status" Language="C#"  MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IndexStatus.aspx.cs" Inherits="BarcodeConversion.IndexStatus" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    
<script src="Scripts/jquery.dynDateTime.min.js" type="text/javascript"></script>
<script src="Scripts/calendar-en.min.js" type="text/javascript"></script>
<link href="Content/calendar-blue.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
    $(document).ready(function () {
        $("#<%=from.ClientID %>").dynDateTime({
            showsTime: true,
            ifFormat: "%Y/%m/%d %H:%M",
            daFormat: "%l;%M %p, %e %m, %Y",
            align: "BR",
            electric: false,
            singleClick: false,
            displayArea: ".siblings('.dtcDisplayArea')",
            button: ".next()"
        });
        $("#<%=to.ClientID %>").dynDateTime({
            showsTime: true,
            ifFormat: "%Y/%m/%d %H:%M",
            daFormat: "%l;%M %p, %e %m, %Y",
            align: "BR",
            electric: false,
            singleClick: false,
            displayArea: ".siblings('.dtcDisplayArea')",
            button: ".next()"
        });
    });
</script>


     <asp:Panel ID="indexStatusPanel" runat="server">
        <h2 style="margin-top:30px;margin-bottom:20px">View Index Status</h2>   

        <div>           
            <table class = "table">
                <tr> 
                    <td style="padding-bottom:15px;"><asp:Button ID="reset" runat="server" Text="Reset" onclick="reset_Click" /></td>
                </tr>
                <tr>
                    <td><asp:Label ID="filterLabel" runat="server"><b>Filter:</b></asp:Label></td>
                    <td > 
                        <asp:DropDownList ID="whoFilter" OnSelectedIndexChanged="onSelectedChange" runat="server" AutoPostBack="true">
                            <asp:ListItem Value="meOnly">Show Your Sheets Only</asp:ListItem>
                            <asp:ListItem Value="everyone">Show Sheets for all Operators</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                     <td> 
                        <asp:DropDownList ID="whenFilter" OnSelectedIndexChanged="onSelectWhen" runat="server" AutoPostBack="true">
                            <asp:ListItem Value="allTime">For All Time</asp:ListItem>
                            <asp:ListItem Value="pickRange">Select Date/Time Range</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                    <td> 
                        <asp:DropDownList ID="whatFilter" OnSelectedIndexChanged="onSelectedChange" runat="server" AutoPostBack="true">
                            <asp:ListItem Value="allSheets">All Sheets</asp:ListItem>
                            <asp:ListItem Value="printed">Printed Only</asp:ListItem>
                            <asp:ListItem Value="notPrinted">Not Printed</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
            </table> 
            <asp:Panel ID="timePanel" Visible="false" runat="server">
                <table class = "table" style="width:548px;">
                    <tr>
                        <td><asp:label runat="server">From:&nbsp;&nbsp;&nbsp;</asp:label>
                            <asp:TextBox ID="from" runat="server" ></asp:TextBox>
                            <img style="margin-left:2px;" src="Content/calender.png" /> 
                        </td>
                        <td style="padding-left:15px;"><asp:label runat="server">To:&nbsp;&nbsp;</asp:label>
                            <asp:TextBox ID="to" runat="server"></asp:TextBox>
                            <img style="margin-left:2px;" src="Content/calender.png" /> 
                        </td>
                        <td style="padding-left:15px;">
                            <asp:Button ID="dates" Text="Submit" runat="server" onclick="submit_Click" />
                        </td>
                    </tr>
                </table>
            </asp:Panel>
            <h4 style="margin-top:35px; color:blue"><asp:Label ID="description" Text="" Visible="false" runat="server"></asp:Label></h4>
            <asp:GridView ID="indexeStatusGridView" runat="server" style="margin-top:15px" CssClass="mydatagrid" PagerStyle-CssClass="pager"
                        PageSize="20" HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="true" OnPageIndexChanging="pageChange_Click" OnRowDataBound="rowDataBound" > 
                <columns>
                    <asp:templatefield HeaderText ="N&#176;" ShowHeader="true">
                        <ItemTemplate >
                            <%# Container.DataItemIndex + 1 %>
                        </ItemTemplate>
                    </asp:templatefield>

                    <asp:TemplateField HeaderText="" ShowHeader="false">
                        <ItemTemplate>
                            <asp:Image runat="server" ID="imgBarCode" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </columns>       
            </asp:GridView>
        
        </div>   
    </asp:Panel>
</asp:Content>
