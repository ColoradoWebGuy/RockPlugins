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
using System.Text.RegularExpressions;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.org_secc.CheckinMonitor
{
    [DisplayName( "Super Search" )]
    [Category( "SECC > Check-in" )]
    [Description( "Displays keypad for searching on phone numbers." )]
    [BooleanField( "Add Family Option", "Should the option to add a new family be available after search?", true )]
    public partial class SuperSearch : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( CurrentCheckInState == null )
            {
                NavigateToPreviousPage();
                return;
            }

            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                this.Page.Form.DefaultButton = lbSearch.UniqueID;
                CurrentWorkflow = null;
                CurrentCheckInState.CheckIn = new CheckInStatus();
                SaveState();
            }
            else
            {
                DisplayFamilies();
            }
        }

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            Search();

        }

        private void Search()
        {
            if ( CurrentCheckInState == null )
            {
                NavigateToPreviousPage();
                return;
            }

            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();

            string searchInput = tbPhone.Text.Trim();
            if ( string.IsNullOrWhiteSpace( searchInput ) )
            {
                return;
            }

            CurrentCheckInState.CheckIn.SearchValue = searchInput;

            if ( Regex.IsMatch( searchInput, @"^\d+$" ) )
            {
                CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER );
            }
            else
            {
                CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME );
            }


            CurrentCheckInState.CheckIn.UserEnteredSearch = true;
            CurrentCheckInState.CheckIn.ConfirmSingleFamily = true;
            ProcessSelection();
        }

        protected void ProcessSelection()
        {
            List<string> errorMessages = new List<string>();
            try
            {
                ProcessActivity( GetAttributeValue( "WorkflowActivity" ), out errorMessages );
            }
            catch ( Exception ex )
            {
                LogException( ex );
            }
            if ( errorMessages.Any() )
            {
                maWarning.Show( "Error processing workflow activity.", Rock.Web.UI.Controls.ModalAlertType.Alert );
                return;
            }

            //sort by last name
            CurrentCheckInState.CheckIn.Families = CurrentCheckInState.CheckIn.Families.OrderBy( f => f.Caption ).ToList();

            DisplayFamilies();
            SaveState();
        }

        private void DisplayFamilies()
        {
            if ( CurrentCheckInState == null )
            {
                NavigateToHomePage();
                return;
            }

            phFamilies.Controls.Clear();
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                BootstrapButton btnFamily = new BootstrapButton();
                btnFamily.CssClass = "btn btn-default btn-block checkin-family";
                btnFamily.Text = family.Caption + "<br>" + family.SubCaption;
                btnFamily.ID = family.Group.Id.ToString();
                btnFamily.Click += ( s, e ) =>
                 {
                     family.Selected = true;
                     SaveState();
                     NavigateToNextPage();
                 };
                phFamilies.Controls.Add( btnFamily );
            }
            if ( GetAttributeValue( "AddFamilyOption" ).AsBoolean() )
            {
                BootstrapButton btnNewFamily = new BootstrapButton();
                btnNewFamily.CssClass = "btn btn-primary btn-block";
                btnNewFamily.Text = "Add New Family";
                btnNewFamily.ID = "NewFamily";
                btnNewFamily.Click += ( s, e ) =>
                {
                    NavigateToNextPage();
                };
                phFamilies.Controls.Add( btnNewFamily );
            }

        }

        protected void Timer1_Tick( object sender, EventArgs e )
        {
        }
    }
}