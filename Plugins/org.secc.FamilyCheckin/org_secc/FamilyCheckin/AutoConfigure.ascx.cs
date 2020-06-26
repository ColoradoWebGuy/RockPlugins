﻿// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using org.secc.FamilyCheckin.Cache;
using org.secc.FamilyCheckin.Model;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.org_secc.FamilyCheckin
{
    [DisplayName( "AutoConfigure" )]
    [Category( "SECC > Check-in" )]
    [Description( "Checkin auto configure block" )]

    [BooleanField( "Manual", "Allow for manual configuration" )]

    public partial class AutoConfigure : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( GetAttributeValue( "Manual" ).AsBoolean() )
                {
                    ShowManual();
                }
                else
                {
                    if ( CurrentUser == null && ( PageParameter( "KioskName" ).IsNotNullOrWhiteSpace() || PageParameter( "datetime" ).IsNotNullOrWhiteSpace() ) )
                    {
                        var site = RockPage.Layout.Site;
                        if ( site.LoginPageId.HasValue )
                        {
                            site.RedirectToLoginPage( true );
                        }
                        else
                        {
                            FormsAuthentication.RedirectToLoginPage();
                        }
                    }
                    else if ( PageParameter( "KioskName" ).IsNotNullOrWhiteSpace() )
                    {
                        SetKiosk( PageParameter( "KioskName" ) );
                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript( upContent, upContent.GetType(), "GetClient", "setTimeout(function(){getClientName()},100);", true );
                    }
                }
            }
            else
            {
                if ( GetAttributeValue( "Manual" ).AsBoolean() )
                {
                    return;
                }

                if ( Request["__EVENTTARGET"] == "ClientName" )
                {
                    //Use Kiosk given client name
                    SetKiosk( Request["__EVENTARGUMENT"] );
                }
                else if ( Request["__EVENTTARGET"] == "UseDNS" )
                {
                    //if the Javascript request throws an error
                    //try to get kiosk name via Javascript
                    try
                    {
                        var ip = Rock.Web.UI.RockPage.GetClientIpAddress();
                        SetKiosk( System.Net.Dns.GetHostEntry( ip ).HostName );
                    }
                    catch
                    {
                        ltDNS.Text = "Unable to determine device name.";
                        pnlMain.Visible = true;
                    }
                }
                else
                {
                    ScriptManager.RegisterStartupScript( upContent, upContent.GetType(), "GetClient", "setTimeout(function(){getClientName()},100);", true );
                }
            }
        }

        private void ShowManual()
        {
            if ( CurrentUser != null )
            {
                pnlMain.Visible = false;
                pnlManual.Visible = true;
                BindDropDownList();
            }
        }

        private void SetKiosk( string clientName )
        {
            var rockContext = new RockContext();

            var kioskService = new KioskService( rockContext );
            var kiosk = kioskService.GetByClientName( clientName );
            if ( kiosk == null )
            {
                kiosk = new Kiosk();
                kiosk.Name = clientName;
                kiosk.Description = "Automatically created Kiosk";
                kioskService.Add( kiosk );
                rockContext.SaveChanges();
            }
            GetKioskType( kiosk, rockContext );
        }

        /// <summary>
        /// Attempts to match a known kiosk based on the IP address of the client.
        /// </summary>
        private void GetKioskType( Kiosk kiosk, RockContext rockContext )
        {
            if ( kiosk.KioskType != null )
            {
                DeviceService deviceService = new DeviceService( rockContext );
                //Load matching device and update or create information
                var device = deviceService.Queryable().Where( d => d.Name == kiosk.Name ).FirstOrDefault();

                //create new device to match our kiosk
                if ( device == null )
                {
                    device = new Device();
                    device.DeviceTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
                    device.Name = kiosk.Name;
                    deviceService.Add( device );
                }

                device.LoadAttributes();
                device.IPAddress = kiosk.IPAddress;
                device.Locations.Clear();
                foreach ( var loc in kiosk.KioskType.Locations.ToList() )
                {
                    device.Locations.Add( loc );
                }
                device.PrintFrom = kiosk.PrintFrom;
                device.PrintToOverride = kiosk.PrintToOverride;
                device.PrinterDeviceId = kiosk.PrinterDeviceId;
                rockContext.SaveChanges();

                if ( PageParameter( "DateTime" ).AsDateTime().HasValue )
                {
                    device.SetAttributeValue( "core_device_DebugDateTime", PageParameter( "datetime" ) );
                }
                else
                {
                    device.SetAttributeValue( "core_device_DebugDateTime", "" );
                }
                device.SaveAttributeValues( rockContext );

                LocalDeviceConfig.CurrentKioskId = device.Id;
                LocalDeviceConfig.CurrentGroupTypeIds = kiosk.KioskType.GroupTypes.Select( gt => gt.Id ).ToList();
                LocalDeviceConfig.CurrentCheckinTypeId = kiosk.KioskType.CheckinTemplateId;

                CurrentCheckInState = null;
                CurrentWorkflow = null;

                var kioskTypeCookie = this.Page.Request.Cookies["KioskTypeId"];

                if ( kioskTypeCookie == null )
                {
                    kioskTypeCookie = new System.Web.HttpCookie( "KioskTypeId" );
                }

                kioskTypeCookie.Expires = RockDateTime.Now.AddYears( 1 );
                kioskTypeCookie.Value = kiosk.KioskType.Id.ToString();

                this.Page.Response.Cookies.Set( kioskTypeCookie );

                Session["KioskTypeId"] = kiosk.KioskType.Id;
                Session["KioskMessage"] = kiosk.KioskType.Message;

                //Clean things up so we have the freshest possible version.
                KioskTypeCache.Remove( kiosk.KioskTypeId ?? 0 );
                KioskDevice.Remove( device.Id );

                SaveState();
                NavigateToNextPage();
            }
            else
            {
                ltDNS.Text = kiosk.Name;
                pnlMain.Visible = true;
            }
        }

        protected void Timer1_Tick( object sender, EventArgs e )
        {
        }

        protected void btnSelectKiosk_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var kioskTypeService = new KioskTypeService( rockContext );
            var kioskType = kioskTypeService.Get( ddlKioskType.SelectedValue.AsInteger() );
            if ( kioskType == null )
            {
                return;
            }


            var kioskService = new KioskService( rockContext );
            var kiosk = kioskService.GetByClientName( CurrentUser.UserName );
            if ( kiosk == null )
            {
                kiosk = new Kiosk();
                kiosk.Name = CurrentUser.UserName;
                kiosk.Description = "Automatically created personal Kiosk";
                kioskService.Add( kiosk );
            }
            kiosk.KioskTypeId = kioskType.Id;
            rockContext.SaveChanges();
            GetKioskType( kiosk, rockContext );
        }

        private void BindDropDownList( Kiosk kiosk = null )
        {
            RockContext rockContext = new RockContext();
            KioskTypeService kioskTypeService = new KioskTypeService( rockContext );


            ddlKioskType.DataSource = kioskTypeService
                .Queryable()
                .OrderBy( t => t.Name )
                .Select( t => new
                {
                    t.Name,
                    t.Id
                } )
                .ToList();
            ddlKioskType.DataBind();
        }
    }
}