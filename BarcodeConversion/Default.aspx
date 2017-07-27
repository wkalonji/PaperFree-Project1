﻿<%@ Page Title="Create Indexes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BarcodeConversion._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script>
        // FADEOUT INDEX-SAVED MSG. FUNCTION
        function FadeOut() {
            $("span[id$='indexSetPrintedMsg']").hide();
            $("span[id$='indexSavedMsg']").fadeOut(3000);
        } 
        function FadeOut2() {
            $("span[id$='indexSavedMsg']").hide();
            $("span[id$='indexSetPrintedMsg']").fadeOut(4000);
        }
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
                var answer = confirm("IMPORTANT!\n\n" +
                    "Click OK If you did print and are satisfied with the Index Sheets.\n" +
                    "Click CANCEL if you did not print or are not satisfied with the Index Sheets.");
                if (answer == true) {
                    // Set the just printed index as PRINTED
                    document.getElementById("pageToPrint").style.display = "none";
                    document.getElementById('<%=setAsPrinted.ClientID%>').click();
                } else {
                    document.getElementById("pageToPrint").style.display = "none";
                    document.getElementById('<%=backToForm.ClientID%>').click(); 
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

    <asp:Panel ID="formPanel" runat="server">
        <asp:Panel ID="formPanelJobSelection" runat="server">
            <div><h2 style="margin-top:35px">Job Selection</h2></div>
            <asp:Button ID="selectJobBtn" Visible="false" runat="server" Text="Generate Jobs" onclick="selectJob_Click" />

            <table class = table>
                <tr> <th colspan="2">Please Select a Job below </th></tr>
                <tr>
                    <td style="width: 186px"><asp:Label ID="selectJobLabel" runat="server">Job Abbreviation:</asp:Label></td>
                    <td> 
                        <asp:DropDownList ID="selectJob" OnSelectedIndexChanged="onJobSelect" runat="server">
                            <asp:ListItem Value="Select">Select</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
            </table> 
            <asp:Panel ID="noJobsFound" Visible="false" runat="server"><h3> You do not currently have any job assigned to you.</h3> </asp:Panel>
        </asp:Panel>
        
        <asp:panel ID="indexCreationSection" Visible="false" runat="server" style="width:auto; margin:auto">           
            <h2 style="margin-top:35px">Index Creation</h2>

            <table class = table>
                <tr> <th colspan="3">Please fill information below </th></tr>
                <tr>
                    <td><asp:Label ID="LABEL1" Text="LABEL1" Visible="false" runat="server"></asp:Label></td>
                    <td><asp:TextBox ID="label1Box" Visible="false" placeholder=" Required" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><asp:Label ID="LABEL2" Text="LABEL2"  Visible="false" runat="server"></asp:Label></td>
                    <td><asp:TextBox ID="label2Box" Visible="false" placeholder=" Required" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><asp:Label ID="LABEL3" Text="LABEL3"  Visible="false" runat="server"></asp:Label></td>
                    <td><asp:TextBox ID="label3Box" Visible="false" placeholder=" Required" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><asp:Label ID="LABEL4" Text="LABEL4"  Visible="false" runat="server"></asp:Label></td>
                    <td><asp:TextBox ID="label4Box" Visible="false" placeholder=" Required" runat="server"></asp:TextBox></td>
                </tr>
                <tr>
                    <td><asp:Label ID="LABEL5" Text="LABEL5"  Visible="false" runat="server"></asp:Label></td>
                    <td><asp:TextBox ID="label5Box" Visible="false" placeholder=" Required" runat="server"></asp:TextBox></td>
                </tr>
            </table>
            <div style="margin-bottom:20px">
                <asp:CheckBox ID="chkShowText" Visible="false" runat="server" Checked="True" Text="Show index below barcode" TextAlign="right" />
            </div>
            <div style="margin-bottom:25px;margin-top:30px;">
                <asp:Button ID="btnGenerateBarcode" runat="server" Text="Generate Index" onclick="btnGenerateBarcode_Click" />
            </div>

     <%--
        Not utilized yet.
        <p>
            Barcode thickness:
            <asp:DropDownList ID="ddlBarcodeThickness" runat="server">
                <asp:ListItem Value="1">Thin</asp:ListItem>
                <asp:ListItem Value="2">Medium</asp:ListItem>
                <asp:ListItem Value="3">Thick</asp:ListItem>    
            </asp:DropDownList>
        </p>
           
     --%>   <asp:Panel ID="generateIndexSection" Visible="false" runat="server">
                <table class = table >
                    <tr>
                        <td style="vertical-align:middle"><asp:Label ID="indexLabel" runat="server">Index generated:  </asp:Label></td>
                        <td style="vertical-align:middle"><asp:TextBox ID="textToConvert" Readonly="true" Borderstyle="None" Font-Bold="true" Width=250 Font-size="20px" runat="server"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td style="vertical-align:top"><asp:Label ID="barcodeLabel" runat="server">Corresponding Barcode:</asp:Label></td>
                        <td style="vertical-align:middle"><asp:Image ID="imgBarcode" runat="server"/></td>
                    </tr>
                </table>

                <table class = tableFull style="margin-top:25px">
                    <tr>
                        <td><asp:Button ID="saveIndex" runat="server" Text="Save Index" onclick="saveIndex_Click" /></td>
                        <td style="text-align:left"><asp:Button ID="saveAndPrint" runat="server" Text="Save & Print Barcode" onclick="saveAndPrint_Click" /></td>
                    </tr>                  
                </table>
            </asp:Panel>
            <div style="margin-bottom:25px; margin-top:15px;">
                <asp:Label ID="indexSavedMsg" Visible="false" Text="Index string saved successfully..." runat="server"></asp:Label>
            </div>
        </asp:panel>

        <div style="margin-top:50px;"></div>
        <%--Link to Print Indexes page --%>     
        <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/Indexes"><span style="font-size:medium;">View all of your unprinted indexes</span></asp:HyperLink>
        <input id="indexString" type="hidden"  runat="server"  value=""/>
    </asp:Panel>

    <%-- Helper hidden buttons to help set as PRINTED the just-printed index And help get back to blank form--%>
    <div style="display:none;">
        <asp:Button ID="setAsPrinted" runat="server" Text="ShowPanel" onclick="setAsPrinted_Click"/>
    </div>
    <div style="display:none;">
        <asp:Button ID="backToForm" runat="server" Text="ShowPanel" onclick="backToForm_Click"/>
    </div>
  
    <%-- Msgs showing up when successful save or saveAndPrint operations happens.--%>
   
    <div style="margin-bottom:25px;">
        <asp:Label ID="indexSetPrintedMsg" Visible="false" Text="1 Index record was saved and set as PRINTED" runat="server"></asp:Label>
    </div>
</asp:Content>
