using System;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.WebControls;

namespace HedgehogDevelopment.SharedSource.DescriptiveTreelist.Shell.Applications.ContentEditor
{
    /// <summary>
    /// The TreeList Descriptive class.
    /// </summary>
    public class TreeListDesc : TreeList
    {
        // The default is the same as in TreeList, that is to show the root
        private const bool ShowRootDefault = true;
        private const bool ShowExtendedHeadersDefault = true;
        private const string SeparatorDefault = " > ";

        /// <summary>
        /// The DataSource property.
        /// </summary>
        public override string DataSource
        {
            get
            {
                return GetViewStateString("DataSource");
            }
            set
            {
                SetViewStateString("DataSource", value);
            }
        }

        /// <summary>
        /// The ShowExtendedHeaders property.
        /// Controls whether extended headers will be shown or not.
        /// 
        /// The property is saved to view state only if its value is different
        /// from the default value.
        /// </summary>
        public bool ShowExtendedHeaders
        {
            get
            {
                object o = ViewState["ShowExtendedHeaders"];
                if (o == null)
                    return ShowExtendedHeadersDefault;
                else
                    return o.ToString() == "1";
            }
            set
            {
                if (value != ShowExtendedHeadersDefault)
                    ViewState["ShowExtendedHeaders"] = value == true ? "1" : "0";
            }
        }

        /// <summary>
        /// The ShowRoot property.
        /// Controls whether the tree root will be shown or not.
        /// 
        /// The property is saved to view state only if its value is different
        /// from the default value.
        /// </summary>
        public bool ShowRoot
        {
            get
            {
                object o = ViewState["ShowRoot"];
                if (o == null)
                    return ShowRootDefault;
                else
                    return o.ToString() == "1";
            }
            set
            {
                if (value != ShowRootDefault)
                    ViewState["ShowRoot"] = value == true ? "1" : "0";
            }
        }

        /// <summary>
        /// The Separator property.
        /// 
        /// The property is saved to view state only if its value is different
        /// from the default value.
        /// </summary>
        public string Separator
        {
            get
            {
                object o = ViewState["Separator"];
                if (o == null)
                    return SeparatorDefault;
                else
                    return o.ToString();
            }
            set
            {
                if (value != SeparatorDefault)
                    ViewState["Separator"] = value;
            }
        }

        /// <summary>
        /// Called when the page is loaded. Set up the object the first time
        /// the page is loaded (i.e. this is not an AJAX event event).
        /// </summary>
        /// <param name="args">Event arguments.</param>
        protected override void OnLoad(EventArgs args)
        {
            base.OnLoad(args);
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                // Set the properties
                SetProperties();

                // Configure the Treelist to show/not show the tree root
                ShowTreeRoot(this.ShowRoot);

                // Show extended headers
                if (this.ShowExtendedHeaders)
                    ChangeHeaders();
            }
        }

        /// <summary>
        /// Adds a new item to the listbox.
        /// </summary>
        protected new void Add()
        {
            base.Add();

            // Show extended headers
            if (this.ShowExtendedHeaders)
                ChangeHeaders();
        }

        /// <summary>
        /// Changes the headers of the items displayed in the listbox to
        /// extended headers.
        /// </summary>
        private void ChangeHeaders()
        {
            // Get the database name
            Database contentDatabase = Sitecore.Context.ContentDatabase;
            if (!string.IsNullOrEmpty(this.DatabaseName))
            {
                contentDatabase = Sitecore.Configuration.Factory.GetDatabase(this.DatabaseName);
            }
            
            // Go through all the items in the treelist's listbox and modify
            // the header for each item
            Listbox listbox = FindControl(ID + "_selected") as Listbox;
            if (listbox == null)
            {
                return;
            }

            ListItem[] listItems = listbox.Items;
            foreach (ListItem listItem in listItems)
            {
                // A list item has 2 IDs: the first one is for the list item
                // itself, the second one is for the data
                string[] listItemValues = listItem.Value.Split(new char[] { '|' });
                if (listItemValues.Length <= 1)
                {
                    continue;
                }

                Item item = contentDatabase.GetItem(listItemValues[1]);
                if (item != null)
                {
                    listItem.Header = GetDescriptiveText(item);
                }
                else
                {
                    listItem.Header = string.Concat(listItemValues[1], " ", Sitecore.Globalization.Translate.Text("[Item not found]"));
                }
            }

            Sitecore.Context.ClientPage.ClientResponse.Refresh(listbox);
            SetModified();
        }

        /// <summary>
        /// Gets the extended header for an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="dataSourceHierarchy">The data source directory hierarchy.</param>
        /// <returns>The extended header.</returns>
        private string GetDescriptiveText(Item item)
        {
            // if the item is the actual datasource, return the item name
            if (string.Equals(item.Paths.Path, DataSource, StringComparison.OrdinalIgnoreCase))
            {
                return item.Name;
            }

            // If the item is not in the data source hierarchy, return the full item path
            if (!item.Paths.Path.StartsWith(DataSource, StringComparison.OrdinalIgnoreCase))
            {
                return item.Paths.Path.TrimStart('/').Replace("/", Separator);
            }

            // Determine the leading path to remove
            string removePrefix = DataSource;
            if (ShowRoot)
            {
                // Grab the path, except the actual datasource item
                string[] segments = DataSource.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                removePrefix = string.Join("/", segments.Take(segments.Length - 1).ToArray());
            }

            // remove the leading path
            string extHeader = Regex.Replace(item.Paths.Path, removePrefix, "", RegexOptions.IgnoreCase);

            return extHeader.TrimStart('/').Replace("/", Separator);
        }

        /// <summary>
        /// Sets properties: DataSource, ShowExtendedHeaders, ShowRoot and Separator.
        /// </summary>
        private void SetProperties()
        {
            string cleanedSource = Source.Trim().ToLower();
            if (!cleanedSource.StartsWith("/") ||
                !cleanedSource.StartsWith("query:") ||
                !cleanedSource.StartsWith("fast:"))
            {
                // Set DataSource
                this.DataSource = StringUtil.ExtractParameter("DataSource", Source).Trim();

                // Set ShowExtendedHeaders
                string showExtendedHeaders = StringUtil.ExtractParameter("ShowExtendedHeaders", Source).Trim().ToLower();
                if (showExtendedHeaders == "no")
                    this.ShowExtendedHeaders = false;
                else
                    this.ShowExtendedHeaders = true;

                // Set ShowRoot
                string showRoot = StringUtil.ExtractParameter("ShowRoot", Source).Trim().ToLower();
                if (showRoot == "no")
                    this.ShowRoot = false;
                else
                    this.ShowRoot = true;

                // Set Separator
                string separator = StringUtil.ExtractParameter("Separator", Source);
                if (separator != "")
                    this.Separator = separator;
            }
        }

        /// <summary>
        /// Controls whether the root of the tree should be shown or not.
        /// </summary>
        /// <param name="value"></param>
        private void ShowTreeRoot(bool value)
        {
            TreeviewEx treeviewEx = FindControl(ID + "_all") as TreeviewEx;
            if (treeviewEx != null)
                treeviewEx.ShowRoot = value;
        }
    }
}