﻿<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Main.Master" AutoEventWireup="true"
    Inherits="CodeCampServer.UI.Helpers.ViewPage.BaseViewPage<UserGroupForm>" %>

<asp:Content ID="Content2" ContentPlaceHolderID="Stylesheets" runat="server">
    <script type="text/javascript" src="/scripts/rsswidget.js"></script>
</asp:Content>

<asp:Content ContentPlaceHolderID="Main" runat="server">
    <%=Errors.Display()%>
    <%Html.RenderAction<EventController>(c => c.UpComing(null));%>
</asp:Content>

<asp:Content ContentPlaceHolderID="SidebarPlaceHolder" runat="server">
    <%Html.RenderPartial("Sponsors", Model.Sponsors);%>
    <hr />
    <h2>
        <%= Model.Name %>
        <%Html.RenderPartial("EditUserGroupLink", Model); %></h2>
    <p>
        <%= Model.City %>,
        <%= Model.Region %> -
        <%= Model.Country%></p>
</asp:Content>
