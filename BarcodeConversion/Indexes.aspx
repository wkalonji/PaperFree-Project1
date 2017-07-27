
<%@ Page Title="Indexes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Indexes.aspx.cs" Inherits="BarcodeConversion.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

     <script>
         // FADEOUT INDEX-SAVED MSG. FUNCTION
         //function FadeOut() {
         //    $("span[id$='indexSavedMsg']").fadeOut(3000);
         //}
        // function FadeOut2() {
          //   $("span[id$='indexSetPrintedMsg']").fadeOut(3000);
         //}
        // PRINTING INDEX SHEETS. FUNCTION
        function printing() {
            window.print();
        }

        // PRINT WINDOW LISTNER. FUNCTION: ALLOW STUFF BE DONE RIGHT BFR or AFTER PRINT PREVIEW WINDOW.
        (function () {
            var beforePrint = function () {
                // Do something before printing dialogue box appears
            };
            // After printing dialogue box disappears, back to unprinted indexes gridview
            var afterPrint = function () {
                var answer = confirm("IMPORTANT!\n\nAre you satisfied?\n" +
                    "Click OK If you did print and are satisfied with the Index Sheets.\n" +
                    "Click CANCEL if you did not print or are not satisfied with the Index Sheets.");
                if (answer == true) {
                   // alert("SECOND");
                    document.getElementById("pageToPrint").style.display = "none";
                    document.getElementById('<%=setAsPrinted.ClientID%>').click();                                    
                } else {
                    document.getElementById("pageToPrint").style.display = "none";
                    document.getElementById('<%=getUnprinted.ClientID%>').click(); 
                }
            };

            if (window.matchMedia) {
                var mediaQueryList = window.matchMedia('print');
                mediaQueryList.addListener(function (mql) {
                    if (mql.matches) {
                        beforePrint();
                    } else {
                        afterPrint();
                    }
                });
            }
            window.onbeforeprint = beforePrint;
            window.onafterprint = afterPrint;
        }());
    </script>


    <asp:Panel ID="unprintedIndexesPanel" runat="server">
        <h2 style="margin-top:30px;margin-bottom:20px">View And Print Index Sheets</h2>   

        <div>           
            <table class = table style="margin-top:25px">
                <tr><td colspan="2" style="padding-bottom:40px;"><asp:Button ID="getUnprintedIndexes" Visible="true" runat="server" Text="Reset" onclick="getUnprintedIndexes_Click" /></td></tr>
                <tr>
                    <td>
                        <h3 style="color:blue; display:inline"><asp:Label ID="description" Text="Your Unprinted Indexes" Visible="True" runat="server"></asp:Label></h3>
                    </td>
                    <td style="text-align:right; vertical-align:central; padding-bottom:5px;">
                        <asp:Button ID="deleteBtn" Visible="false" runat="server" Text="Delete Indexes" 
                            OnClientClick="return confirm('Selected Indexes will be permanently deleted. Delete anyway?');" 
                            OnClick="deleteIndexes_Click" />
                    </td>
                </tr>                  
            </table>
        
            <asp:GridView ID="indexesGridView" runat="server" style="margin-top:20px" CssClass="mydatagrid" PagerStyle-CssClass="pager"
                        PageSize="20" HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="true" OnPageIndexChanging="pageChange_Click" OnRowDataBound="rowDataBound" > 
                <columns>
               
                    <asp:templatefield HeaderText="Select">
                        <HeaderTemplate>
                            <asp:checkbox ID="selectAll" AutoPostBack="true" OnCheckedChanged="selectAll_changed" runat="server"></asp:checkbox>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:checkbox ID="cbSelect"  runat="server"></asp:checkbox>
                        </ItemTemplate>
                    </asp:templatefield>

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
        <div >
            <table class = table style="margin-top:25px">
                <tr>
                    <td><asp:Button ID="getBarcodeBtn" Visible="false" runat="server" Text="Show Barcodes" onclick="getBarcode_Click" /></td>
                    <td style="text-align:right"><asp:Button ID="printBarcodeBtn" Visible="false" runat="server" Text="Print Barcodes" onclick="printBarcode_Click"/></td>
                </tr>                  
            </table>
        </div>   
    </asp:Panel>
    <div style="display:none; margin-top:15px;">
        <asp:Button ID="setAsPrinted" runat="server" Text="ShowPanel" onclick="setAsPrinted_Click"/>
    </div>
     <div style="display:none; margin-top:15px;">
        <asp:Button ID="getUnprinted" runat="server" Text="ShowPanel" onclick="getUnprinted_Click"/>
    </div>
  
</asp:Content>
