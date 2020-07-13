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
// <copyright>
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using org.secc.FamilyCheckin.Model;
using org.secc.FamilyCheckin.Utilities;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_secc.FamilyCheckin
{
    [DisplayName( "Kiosk List" )]
    [Category( "SECC > Check-in" )]
    [Description( "Lists all the kiosks." )]

    [LinkedPage( "Detail Page" )]
    public partial class KioskList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );


            gKiosks.DataKeyNames = new string[] { "Id" };
            gKiosks.Actions.ShowAdd = true;
            gKiosks.Actions.AddClick += gKiosk_Add;
            gKiosks.GridRebind += gKiosk_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gKiosks.Actions.ShowAdd = canAddEditDelete;
            gKiosks.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events (main grid)

        protected void gKiosk_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "KioskId", 0 );
        }


        protected void gKiosk_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "KioskId", e.RowKeyId );
        }


        protected void gKiosk_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            KioskService kioskService = new KioskService( rockContext );
            Kiosk kiosk = kioskService.Get( e.RowKeyId );

            if ( kiosk != null )
            {
                int kioskId = kiosk.Id;

                string errorMessage;
                if ( !kioskService.CanDelete( kiosk, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                kioskService.Delete( kiosk );
                rockContext.SaveChanges();

                DeviceService deviceService = new DeviceService( rockContext );
                var devices = deviceService.Queryable().Where( d => d.Name == kiosk.Name ).ToList();
                foreach ( var device in devices )
                {
                    KioskDeviceHelpers.FlushItem( device.Id );
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gKiosk_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods
        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var kioskService = new KioskService( new RockContext() );
            var kiosks = kioskService.Queryable()
                .Select( k => k )
                .OrderBy( k => k.Name )
                .ToList();

            gKiosks.DataSource = kiosks;
            gKiosks.DataBind();
        }

        #endregion
    }
}