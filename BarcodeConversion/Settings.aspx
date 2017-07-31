<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="BarcodeConversion.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script>
        function FadeOut3() {
            $("span[id$='success']").fadeOut(4000);
        } 
    </script>

    <div style="margin-top:45px; margin-bottom:40px; height:50px; border-bottom:solid 1px green;width:899px;">
        <table>
            <tr>
                <td><h2 style="display:inline; padding-top:25px;">Settings</h2></td>
                <td> 
                    <%-- COLLAPSE ALL--%>
                    <div style="display:block;margin-left:710px;padding-top:5px;">
                        <asp:Button ID="collapseAll" Visible="true" Width="87px" runat="server" Text="Collapse All" OnClick="collapseAll_Click"/>
                    </div>
                </td>
            </tr>
        </table>
    </div>

    <table>
        <tr>
            <td style="width:615px;">
                 <%-- JOB SECTION --%>
                <div style="width:284px; border:solid 2px black; border-radius:3px; background-color:lightgray; display:inline-block;">
                    <asp:Button ID="newJobBtn" Visible="true" Width="330px" runat="server" Text="Job Section" OnClick="newJobShow_Click" />
                </div>
            </td>
            <td style="width:324px;">
                 <%--USER & PERMISSION SECTION --%>
                <div style="width:284px; border: solid 2px black; border-radius:3px;">
                    <asp:Button ID="newUserBtn" Visible="true" runat="server" Text="User & Permission Section" Width="310px" OnClick="permissionsShow_Click" />
                </div>
            </td>
        </tr>   
        <tr>
            <td style="width: 615px; vertical-align:top;">
                <%-- JOB SECTION BODY --%>
                <div style="display:block; width: 26%;" class="auto-style5">
                    <asp:Panel ID="jobSection" Visible="false" runat="server" Width="408px" > 
                        <asp:Label runat="server"><h4 style="margin-top:25px;">Create/Delete Jobs</h4></asp:Label>
                        <asp:Label runat="server">
                            <h6>Note: You can assign a new job to an operator right away.<br />
                                If operator entered doesn't exist, a new job is created anyway.<br />
                                A job assigned here can't be processed by the operator until<br />
                                configured.
                            </h6>
                        </asp:Label>
                        <table  style="margin-top:25px; width: 76%; margin-right: 36px; height: 149px;"  class=auto-style3 > 
                            <tr>
                                <td style="padding-bottom:15px;"><asp:Label Text="Choose your Action: " runat="server"></asp:Label></td>
                                <td style="padding-bottom:15px;">
                                    <asp:DropDownList ID="selectAction" AutoPostBack="True" runat="server" OnSelectedIndexChanged="actionChange">
                                        <asp:ListItem Selected="true" Value="create">Create New Job</asp:ListItem>
                                        <asp:ListItem Value="edit">Edit Existing Job</asp:ListItem>
                                        <asp:ListItem Value="delete">Delete Existing Job</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td class="auto-style2" style="height: 35px; width: 286px;"><asp:Label runat="server">Job Abbreviation: </asp:Label></td>
                                <td style="height: 35px">
                                    <asp:TextBox ID="jobAbb" placeholder=" Required" runat="server"></asp:TextBox>
                                    <asp:DropDownList ID="selectJobList" OnSelectedIndexChanged="onJobAbbSelect" runat="server">
                                        <asp:ListItem Value="Select">Select</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr> 
                            <tr>
                                <td class="auto-style2" style="width: 286px"><asp:Label ID="jobNameLabel" runat="server">Job Name: </asp:Label></td>
                                <td><asp:TextBox ID="jobName" placeholder=" Required" runat="server"></asp:TextBox></td>
                            </tr>
                            <tr>
                                <td class="auto-style2" style="padding-top:5px; width: 286px;"><asp:Label ID="jobActiveLabel" runat="server">Active: </asp:Label></td>
                                <td>
                                    <asp:DropDownList ID="jobActiveBtn" style="margin-top:5px;" AutoPostBack="True" OnSelectedIndexChanged="onActiveSelect" runat="server">
                                        <asp:ListItem Selected="True" Value="1">True</asp:ListItem>
                                        <asp:ListItem Value="0">False</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                             <tr>
                                <td class="auto-style2" style="padding-top:25px; width: 286px;"><asp:Label ID="jobAssignedToLabel" runat="server">Assign To: </asp:Label></td>
                                <td><asp:TextBox ID="jobAssignedTo" style="margin-top:15px;" placeholder=" Optional" runat="server"></asp:TextBox></td>
                            </tr>
                
                        </table>
                        <table style="margin-top:20px; " class="auto-style4">
                            <tr style="height:15px;">
                               <td style="height: 15px; text-align:right;">
                                    <asp:Button ID="deleteJobBtn" Visible="false" runat="server" Text="Delete " 
                                        OnClientClick="return confirm('ATTENTION Deleting this job will also delete its configuration. 
                                        Unprinted indexes related to this job will lose some info in the Details section of the printouts.
                                        We advise to Deactivate job instead. Do you still want to procede with Delete?');" OnClick="deleteJob_Click"/> </td>
                            </tr>
                            <tr>
                                 <td style="height: 15px; text-align:right;">
                                    <asp:Button ID="editJobBtn" Visible="false" runat="server" Text="Edit " OnClick="editJob_Click" /></td>
                            </tr>
                            <tr>
                                 <td style="height: 10px;text-align:right;">
                                    <asp:Button ID="createJobBtn"  Visible="true" runat="server" Text="Create" OnClick="createJob_Click"/></td>
                            </tr>
                            <tr>
                                 <td style="height: 10px;text-align:left;">
                                    <asp:Label ID="success"  Visible="false" runat="server" Text="" OnClick="createJob_Click"/></td>
                            </tr>
                        </table>
           
                    </asp:Panel>
                </div>
            </td>
            <td style="width: 324px; vertical-align:top;">
                <%--USER & PERMISSION SECTION BODY--%>
                <div style="display:inline-block; width: 26%;" class="auto-style5">
                    <asp:Panel ID="newUserSection" Visible="false" runat="server" Width="322px" Height="250px" style="margin-top: 0px" >
                        <asp:Label runat="server"><h4 style="margin-top:25px;">Add Users & Set/Remove Admin Privileges</h4></asp:Label>
                        <asp:Label runat="server">
                            <h6>Note: Anyone visiting the site for the 1st time is automatically
                                added as user. A user account can still be created prior to user
                                visiting the site. <br />
                                To create, just type in operator's username, set permissions & submit.
                                You can also change existing user's permissions.
                            </h6>
                        </asp:Label>
                        <table  style="margin-top:25px; height: 72px;"  class=auto-style3 >
                            <tr>
                                <td class="auto-style2" style="height: 31px; margin-left: 200px;"><asp:Label runat="server">Operator: </asp:Label></td>
                                <td style="height: 31px"><asp:TextBox ID="user" placeholder=" Required" runat="server"></asp:TextBox>
                                </td>
                            </tr> 
                            <tr>
                                <td class="auto-style2"><asp:Label runat="server">Permissions: </asp:Label></td>
                                <td>
                                    <asp:DropDownList ID="permissions" AutoPostBack="True" runat="server">
                                        <asp:ListItem Selected="true" Value="0">Operator</asp:ListItem>
                                        <asp:ListItem Value="1">Admin</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </table>
                        <div style="text-align:right; margin-top:15px;" class="auto-style4" id="abc">
                            <asp:Button ID="createBtn2" Visible="true" runat="server" Text="Submit" OnClick="setPermissions_Click" />
                        </div>
                    </asp:Panel>
                </div>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                 <div id="line" visible="false" style=" height:50px; border-bottom:solid 1px green;width:899px;" runat="server"></div>
            </td>
        </tr>
        <tr>
            <td style="width: 615px">
                <%--JOB ACCESS SECTION --%>
                <div style="width:284px; border: solid 2px black; border-radius:3px; margin-bottom:25px;margin-top:50px;">
                    <asp:Button ID="assignBtn" Visible="true" runat="server" Text="Job Access Section" Width="310px" OnClick="assignShow_Click" />
                </div>
            </td>
            <td style="width: 324px">
                <%--JOB INDEX CONFIG SECTION --%>
                <div style="width:284px; border: solid 2px black; border-radius:3px; margin-bottom:25px;margin-top:50px;">
                    <asp:Button ID="jobIndexEditingBtn" Visible="true" runat="server" Text="Job Index Configuration Section" Width="310px" OnClick="jobIndexEditingShow_Click" />
                </div>
            </td>
        </tr>
        <tr>
            <td style="width: 615px; vertical-align:top;">
                <%--JOB ACCESS SECTION BODY --%>
                <asp:Panel ID="assignPanel" Visible="false" runat="server">
                    <asp:Label runat="server"><h4>Assign Jobs to Operators</h4></asp:Label>
                    <asp:Label runat="server">
                        <h6>Note: Jobs assigned here can't be processed by the operator <br />
                            until configured.
                        </h6>
                    </asp:Label>
                    <table  style="margin-top:10px; height: 72px; width: 51.5%;"  class=auto-style3 >
                        <tr>
                            <td class="auto-style2" style="height: 31px"><asp:Label runat="server">Operator: </asp:Label></td>
                            <td style="height: 25px"><asp:TextBox ID="assignee" placeholder=" Required" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr> 
                    </table>
                    <table style="margin-top:20px; margin-bottom:20px; width: 316px;">
                        <tr style="height:15px;">
                            <td style="height: 10px; text-align:left;"><asp:Button ID="assignedBtn"  Visible="true" runat="server" Text="Assigned" OnClick="assignedJob_Click" /></td>
                            <td style="height: 10px; text-align:right;"><asp:Button ID="unassignedBtn" style="margin-left:25px;" Visible="true" runat="server" Text="Unassigned " OnClick="unassignedJob_Click" Width="118px" /></td>
                        </tr> 
                    </table>
                    <div> 
                        <asp:Label ID="jobsLabel" Text="Currently Unassigned Jobs" runat="server"></asp:Label>
                        <asp:GridView ID="jobAccessGridView" Width="318px" runat="server" style="margin-top:8px" CssClass="mydatagrid" PagerStyle-CssClass="pager"
                                    PageSize="10" HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="true" OnPageIndexChanging="pageChange_Click" > 
                            <columns>             
                                <asp:templatefield HeaderText="Select">
                                    <HeaderTemplate>
                                        &nbsp;<asp:checkbox ID="selectAll" AutoPostBack="true" OnCheckedChanged="selectAll_changed" runat="server"></asp:checkbox>
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
                            </columns>       
                        </asp:GridView>     
                    </div>
                    <div style="display:block; width: 535px;" >
                        <table class = table style="margin-top:25px; width: 320px;">
                            <tr>
                                <td style="text-align:left"><asp:Button ID="deleteAssignedBtn" Visible="true" runat="server" Text="Unassign" OnClick="deleteAssigned_Click"/></td>
                                <td style="text-align:right;width: 100%;"><asp:Button ID="jobAccessBtn" Visible="true" runat="server" Text="Assign" OnClick="jobAccess_Click" Width="59px"/></td>                 
                            </tr>                  
                        </table>
                    </div>   
                </asp:Panel>
 
            </td>
            <td style="width: 324px; vertical-align:top;">
                <%--JOB INDEX CONFIG BODY --%>
                <asp:Panel ID="jobIndexEditingPanel" Visible="false" runat="server">
                    <asp:Label runat="server"><h4>Set Index Form Controls & Rules</h4></asp:Label>
                    <asp:Label runat="server">
                        <h6>
                            Note: Only jobs configured here can be processed by the operator.<br />
                            Red colored dropdown items are jobs already configured.
                        </h6>
                    </asp:Label>
                    <table class = table style="width:320px;">
                        <tr> <th colspan="2">Please Select a Job below </th></tr>
                        <tr>
                            <td style="width: 160px"><asp:Label ID="selectJobLabel" runat="server">Job Abbreviation:</asp:Label></td>
                            <td style="text-align:left;"> 
                                <asp:DropDownList ID="selectJob" OnSelectedIndexChanged="onJobSelect" runat="server">
                                    <asp:ListItem Value="Select">Select</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </table>
                    <table  style="margin-top:20px; width: 99%;"  class=auto-style3 >
                        <tr style="height:35px;">
                            <td style=""><asp:Label Text="LABEL1:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="label1" placeholder=" Required only for Set" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="REGEX1:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="regex1" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="LABEL2:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="label2" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="REGEX2:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="regex2" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="LABEL3:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="label3" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="REGEX3:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="regex3" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="LABEL4:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="label4" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="REGEX4:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="regex4" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="LABEL5:" runat="server"></asp:Label></td>
                            <td style="height: 25px; text-align:right;"><asp:TextBox ID="label5" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                        <tr style="height:35px;">
                            <td><asp:Label Text="REGEX5:" runat="server"></asp:Label></td>
                            <td style="height:25px; text-align:right;"><asp:TextBox ID="regex5" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                        </tr>
                    </table>
                    <table style="margin-top:20px; margin-bottom:20px; width: 316px;">
                        <tr style="height:15px;">
                            <td style="height: 10px; text-align:left;">
                                <asp:Button ID="unsetRules" Visible="true" runat="server" Text="Unset"
                                    OnClientClick="return confirm('ATTENTION Removing or changing configuration will affect
                                    the Details section in the printouts of still unprinted indexes related to this job. We strongly advise to make sure that there are no more unprinted indexes
                                    related to this job accross all operators. Then reconfigure the job before new indexes related to it can be created.
                                     Do you still want to procede with Reconfiguration?');"
                                    OnClick="unsetRules_Click" /></td>
                            <td style="height: 10px; text-align:right;"><asp:Button ID="setRules" style="margin-left:25px;" Visible="true" runat="server" Text="Set " OnClick="setRules_Click" /></td>
                        </tr> 
                    </table>
        
                </asp:Panel>
            </td>
        </tr>
    </table>

</asp:Content>
