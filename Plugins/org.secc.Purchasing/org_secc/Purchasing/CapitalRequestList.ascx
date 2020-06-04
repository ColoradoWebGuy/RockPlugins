﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CapitalRequestList.ascx.cs" Inherits="RockWeb.Plugins.org_secc.Purchasing.CapitalRequestList" %>

<script>
    $(document).ready(function () {
        var pgReq = Sys.WebForms.PageRequestManager.getInstance();
        pgReq.add_initializeRequest(initializeRequest);
        pgReq.add_endRequest(endRequest);
        initPlaceholderMasks();
    });

    function initializeRequest(sender, args) {
        //intentionally left empty - CSF
    }

    function endRequest(sender, args) {
        initPlaceholderMasks();
    }

    function initPlaceholderMasks() {
       // $('[id*="txtGLAccount"]').mask("999-999-99999", { placeholder: " " });
    }
</script>

<asp:UpdatePanel ID="upMain" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <div id="pnlMain" class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-columns"></i>&nbsp;Capital Request</h1>
                <Rock:BootstrapButton runat="server" CssClass="btn-add btn btn-default btn-sm pull-right" id="btnAdd" OnClick="lbCreateCapitalRequest_Click"><i class="fa fa-plus"></i>&nbsp;Add Capital Request</Rock:BootstrapButton>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                <Rock:ModalAlert ID="maWarningDialog" runat="server" />
                <Rock:GridFilter ID="gfRequestListFilter" runat="server" OnApplyFilterClick="btnFilterApply_Click" OnClearFilterClick="btnFilterReset_Click">
                    
                    <asp:Panel runat="server">
                        <label>Status</label><br />
                        <a onclick="$('input[id*=\'cblStatus\'][type=checkbox]').prop('checked', true); return false;" href="#">Check All</a>&nbsp;
                        <a onclick="$('input[id*=\'cblStatus\'][type=checkbox]').prop('checked', false); return false;" href="#">Uncheck All</a>
                        <Rock:RockCheckBoxList ID="cblStatus" runat="server" RepeatDirection="Horizontal" />
                    </asp:Panel>
                    <Rock:RockCheckBoxList ID="cblShow" runat="server" Label="Show" RepeatDirection="Horizontal">
                        <asp:ListItem Value="Me" Text="Requested By Me" />
                        <asp:ListItem Value="Ministry" Text="Requested By My Ministry" />
                        <asp:ListItem Text="Requested by My Campus/Location" Value="Location" />
                        <asp:ListItem Value="Approver" Text="I am an Approver" />
                    </Rock:RockCheckBoxList>
                    <Rock:RockDropDownList ID="ddlMinistry" runat="server" Label="Requesting Ministry" />
                    <Rock:RockDropDownList ID="ddlSCCLocation" runat="server" Label="Southeast Location"/>
                    <Rock:PersonPicker ID="requester" runat="server" Label="Requester"></Rock:PersonPicker>
                    <Rock:RockTextBox ID="txtGLAccount" runat="server" Label="General Ledger Account"></Rock:RockTextBox>
                    <Rock:RockDropDownList ID="ddlFiscalYear" runat="server" Label="Fiscal Year"/>
                </Rock:GridFilter>
                <Rock:Grid ID="gRequestList" runat="server" RowItemText="Capital Request" AllowSorting="true" CssClass="js-grid-batch-list">
                    <Columns>
                                
                        <Rock:RockBoundField DataField="CapitalRequestId" Visible="false" />
                        <Rock:RockTemplateField HeaderText="Project Name" SortExpression="ProjectName">                     
                            <ItemTemplate>
                                <asp:HyperLink runat="server" text='<%# Eval("ProjectName") %>' NavigateUrl='<%# String.Format("{0}?CER={1}", CapitalRequestDetailPageSetting, Eval("CapitalRequestId")) %>'></asp:HyperLink> 
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:RockBoundField DataField="RequestingMinistry" HeaderText="Requesting Ministry" SortExpression="RequestingMinistry" />
                        <Rock:RockBoundField DataField="RequesterName" HeaderText="Requester" SortExpression="RequesterNameLastFirst" />
                        <Rock:RockBoundField DataField="FullAccountNumber" HeaderText="Account" SortExpression="FullAccountNumber" />
                        <Rock:RockBoundField DataField="ProjectId" HeaderText="Project" SortExpression="Project" />
                        <Rock:RockBoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                        <Rock:RockBoundField DataField="RequisitionCount" HeaderText="Requisitions" SortExpression="RequisitionCount" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                        <Rock:RockBoundField DataField="TotalCharges" HeaderText="Current Charges" SortExpression="TotalCharges" DataFormatString="{0:c}" />
                    </Columns>
                </Rock:Grid>
            </div>
        </div>
        <script type="text/javascript">
            // Expand the filters area
            var expandFilters = function ()
            {
                el = $('.grid-filter header').get();
                $('i.toggle-filter', el).toggleClass('fa-chevron-down fa-chevron-up');
                $(el).siblings('div').slideDown(0);
            }
            $(document).ready(expandFilters);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(expandFilters);
        </script>
        <style>
            .grid-filter h4 {
                display: none;
            }
            .grid-filter header {
                display: none;
            }
        </style>
    </ContentTemplate>
</asp:UpdatePanel>
