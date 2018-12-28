using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.Globalization;
using System.Resources;

namespace WBrowser
{
    public partial class WBrowser : Form
    {
        ResourceManager res_man;    
        CultureInfo cul;            
        //string exit_text_string, exit_caption_string;
        public static String favXml = "favorits.xml", linksXml = "links.xml";
        String settingsXml="settings.xml", historyXml="history.xml";
        List<String> urls = new List<String>();
        XmlDocument settings = new XmlDocument();
        String homePage;
        CultureInfo currentCulture;

        public WBrowser()
        {
            InitializeComponent();
            currentCulture = CultureInfo.CurrentCulture;
        }

        #region Form load/Closing/Closed
       
//visible items
        private void setVisibility()
        {
            if (!File.Exists(settingsXml))
            {
                XmlElement r = settings.CreateElement("settings");
                settings.AppendChild(r);
                XmlElement el ;
                
                el=settings.CreateElement("menuBar");
                el.SetAttribute("visible","True");
                r.AppendChild(el);

                el = settings.CreateElement("adrBar");
                el.SetAttribute("visible","True");
                r.AppendChild(el);

                el = settings.CreateElement("linkBar");
                el.SetAttribute("visible","True");
                r.AppendChild(el);

                el = settings.CreateElement("favoritesPanel");
                el.SetAttribute("visible","True");
                r.AppendChild(el);

                el = settings.CreateElement("homepage");
                el.InnerText="about:blank";
                r.AppendChild(el);

                el = settings.CreateElement("dropdown");
                el.InnerText = "15";
                r.AppendChild(el);
            }
            else
            {
                settings.Load(settingsXml);
                XmlElement r = settings.DocumentElement;
                menuBar.Visible = (r.ChildNodes[0].Attributes[0].Value.Equals("True"));
                adrBar.Visible = (r.ChildNodes[1].Attributes[0].Value.Equals("True"));
                favoritesPanel.Visible = (r.ChildNodes[3].Attributes[0].Value.Equals("True"));
                homePage=r.ChildNodes[5].InnerText;
            }

            this.commandBarToolStripMenuItem.Checked = adrBar.Visible;
            homePage = settings.DocumentElement.ChildNodes[5].InnerText;
        }
        // form load
        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(3000);
            this.toolStripStatusLabel1.Text = "Done";
            comboBox1.SelectedItem = comboBox1.Items[0];
            hindiToolStripMenuItem.Checked = false;    
            englishToolStripMenuItem.Checked = true;
            res_man = new ResourceManager("WBrowser.Resource.Res", typeof(WBrowser).Assembly);
            switch_language();
            setVisibility();
            addNewTab();
        }
        //form closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (browserTabControl.TabCount != 2)
            {
                DialogResult dlg_res = (new Close()).ShowDialog();

                if (dlg_res == DialogResult.No) { e.Cancel = true; closeTab(); }
                else if (dlg_res == DialogResult.Cancel) e.Cancel = true;
                else Application.ExitThread();
            }
        }
        //form closed
        private void WBrowser_FormClosed(object sender, FormClosedEventArgs e)
        {
            settings.Save(settingsXml);
            File.Delete("source.txt");
        }

         #endregion

        #region FAVORITES,LINKS,HISTORY METHODS 

        //addFavorit method
        private void addFavorit(String url, string name)
        {
            XmlDocument myXml = new XmlDocument();
            XmlElement el = myXml.CreateElement("favorit");
            el.SetAttribute("url", url);
            el.InnerText = name;
            if (!File.Exists(favXml))
            {
                XmlElement root = myXml.CreateElement("favorites");
                myXml.AppendChild(root);
                root.AppendChild(el);
            }
            else
            {
                myXml.Load(favXml);
                myXml.DocumentElement.AppendChild(el);
            }
            if (favoritesPanel.Visible == true)
            {
                TreeNode node = new TreeNode(el.InnerText, faviconIndex(el.GetAttribute("url")), faviconIndex(el.GetAttribute("url")));
                node.ToolTipText = el.GetAttribute("url");
                node.Name = el.GetAttribute("url");
                node.ContextMenuStrip = favContextMenu;
                favTreeView.Nodes.Add(node);
            }
            myXml.Save(favXml);
        }
        //addLink method
        private void addLink(String url, string name)
        {
            XmlDocument myXml = new XmlDocument();
            XmlElement el = myXml.CreateElement("link");
            el.SetAttribute("url", url);
            el.InnerText = name;

            if (!File.Exists(linksXml))
            {
                XmlElement root = myXml.CreateElement("links");
                myXml.AppendChild(root);
                root.AppendChild(el);
            }
            else
            {
                myXml.Load(linksXml);
                myXml.DocumentElement.AppendChild(el);
            }
            if (favoritesPanel.Visible == true)
            {
                TreeNode node = new TreeNode(el.InnerText, faviconIndex(url), faviconIndex(el.GetAttribute("url")));
                node.Name = el.GetAttribute("url");
                node.ToolTipText = el.GetAttribute("url");
                node.ContextMenuStrip = linkContextMenu;
                favTreeView.Nodes[0].Nodes.Add(node);
            }
            myXml.Save(linksXml);
        }
        //delete link method
        private void deleteLink()
        {
             if (favoritesPanel.Visible == true)
                favTreeView.Nodes[0].Nodes[adress].Remove();
            XmlDocument myXml = new XmlDocument();
            myXml.Load(linksXml);
            XmlElement root = myXml.DocumentElement;
            foreach (XmlElement x in root.ChildNodes)
            {
                if (x.GetAttribute("url").Equals(adress))
                {
                    root.RemoveChild(x);
                    break;
                }
            }

            myXml.Save(linksXml);
        }
        //renameLink method
        private void renameLink()
        {
            RenameLink rl = new RenameLink(name);
            if (rl.ShowDialog() == DialogResult.OK)
            {
                XmlDocument myXml = new XmlDocument();
                myXml.Load(linksXml);
                foreach (XmlElement x in myXml.DocumentElement.ChildNodes)
                {
                    if (x.InnerText.Equals(name))
                    {
                        x.InnerText = rl.newName.Text;
                        break;
                    }
                }
                if(favoritesPanel.Visible==true)
                favTreeView.Nodes[0].Nodes[adress].Text = rl.newName.Text;
                myXml.Save(linksXml);
            }
            rl.Close();
        }
        //delete favorit method
        private void deleteFavorit()
        {
            favTreeView.SelectedNode.Remove();

            XmlDocument myXml = new XmlDocument();
            myXml.Load(favXml);
            XmlElement root = myXml.DocumentElement;
            foreach (XmlElement x in root.ChildNodes)
            {
                if (x.GetAttribute("url").Equals(adress))
                {
                    root.RemoveChild(x);
                    break;
                }
            }

            myXml.Save(favXml);

        }
        //renameFavorit method
        private void renameFavorit()
        {
            RenameLink rl = new RenameLink(name);
            if (rl.ShowDialog() == DialogResult.OK)
            {
                XmlDocument myXml = new XmlDocument();
                myXml.Load(favXml);
                foreach (XmlElement x in myXml.DocumentElement.ChildNodes)
                {
                    if (x.InnerText.Equals(name))
                    {
                        x.InnerText = rl.newName.Text;
                        break;
                    }
                }
                favTreeView.Nodes[adress].Text = rl.newName.Text;
                myXml.Save(favXml);
            }
            rl.Close();
        }

        //addHistory method
        private void addHistory(Uri url,string data)
        {
            XmlDocument myXml = new XmlDocument();
            int i=1;
            XmlElement el = myXml.CreateElement("item");
            el.SetAttribute("url", url.ToString());
            el.SetAttribute("lastVisited", data);

            if (!File.Exists(historyXml))
            {
                XmlElement root = myXml.CreateElement("history");
                myXml.AppendChild(root);
                el.SetAttribute("times", "1");
                root.AppendChild(el);
            }
            else
            {
                myXml.Load(historyXml);

                foreach (XmlElement x in myXml.DocumentElement.ChildNodes)
                {
                    if (x.GetAttribute("url").Equals(url.ToString()))
                    {
                        i = int.Parse(x.GetAttribute("times")) + 1;
                        myXml.DocumentElement.RemoveChild(x);
                        break;
                    }
                }

                el.SetAttribute("times", i.ToString());
                myXml.DocumentElement.InsertBefore(el, myXml.DocumentElement.FirstChild);

                if (favoritesPanel.Visible == true)
                {
                    /*view by site*/
                    if (comboBox1.Text.Equals("View By Site"))
                    {
                        if (!historyTreeView.Nodes.ContainsKey(url.Host.ToString()))
                        {
                            historyTreeView.Nodes.Add(url.Host.ToString(), url.Host.ToString(), 0, 0);

                            TreeNode node =
                                   new TreeNode(url.ToString(), 3, 3);
                            node.ToolTipText = url.ToString() + "\nLast Visited: " + data + "\nTimes visited: " + i.ToString();
                            node.Name = url.ToString();
                            node.ContextMenuStrip = histContextMenu;
                            historyTreeView.Nodes[url.Host.ToString()].Nodes.Add(node);
                        }

                        else
                            if (!historyTreeView.Nodes[url.Host.ToString()].Nodes.ContainsKey(url.ToString()))
                            {
                                TreeNode node =
                                    new TreeNode(url.ToString(), 3, 3);
                                node.ToolTipText = url.ToString() + "\nLast Visited: " + data + "\nTimes visited: " + i.ToString();
                                node.Name = url.ToString();
                                node.ContextMenuStrip = histContextMenu;
                                historyTreeView.Nodes[url.Host.ToString()].Nodes.Add(node);
                            }
                            else
                                historyTreeView.Nodes[url.Host.ToString()].Nodes[url.ToString()].ToolTipText
                                        = url.ToString() + "\nLast Visited: " + data + "\nTimes visited" + i.ToString();

                    }
                    /* view by date*/
                    if (comboBox1.Text.Equals("View by Date"))
                    {
                        if (historyTreeView.Nodes[4].Nodes.ContainsKey(url.ToString()))
                            historyTreeView.Nodes[url.ToString()].ToolTipText
                                    = url.ToString() + "\nLast Visited: " + data + "\nTimes visited: " + i.ToString();
                        else
                        {
                            TreeNode node =
                            new TreeNode(url.ToString(), 3, 3);
                            node.ToolTipText = url.ToString() + "\nLast Visited: " + data + "\nTimes visited :" + i.ToString();
                            node.Name = url.ToString();
                            node.ContextMenuStrip = histContextMenu;
                            historyTreeView.Nodes[4].Nodes.Add(node);
                        }
                    }
                }

            } 
            myXml.Save(historyXml);
        }
//delete history
        private void deleteHistory()
        {
            XmlDocument myXml = new XmlDocument();
            myXml.Load(historyXml);
            XmlElement root = myXml.DocumentElement;
            foreach (XmlElement x in root.ChildNodes)
            {
                if (x.GetAttribute("url").Equals(adress))
                {
                    root.RemoveChild(x);
                    break;
                }
            }
            historyTreeView.SelectedNode.Remove();
            myXml.Save(historyXml);
        }

        #endregion

        #region TABURI
        /*TAB-uri*/

        //addNewTab method
        private void addNewTab()
        {
            TabPage tpage = new TabPage();
            tpage.BorderStyle = BorderStyle.Fixed3D;
            browserTabControl.TabPages.Insert(browserTabControl.TabCount - 1, tpage);
            WebBrowser browser = new WebBrowser();
            browser.Navigate(homePage);   
            tpage.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
            browserTabControl.SelectTab(tpage);
            browser.ProgressChanged += new WebBrowserProgressChangedEventHandler(Form1_ProgressChanged);
            browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(Form1_DocumentCompleted);
            browser.Navigating += new WebBrowserNavigatingEventHandler(Form1_Navigating);
            browser.CanGoBackChanged += new EventHandler(browser_CanGoBackChanged);
            browser.CanGoForwardChanged += new EventHandler(browser_CanGoForwardChanged);
           
        }

       
        //DocumentCompleted
        private void Form1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            WebBrowser currentBrowser = getCurrentBrowser();
            this.toolStripStatusLabel1.Text = "Done";
            String text = "Blank Page";

            if (!currentBrowser.Url.ToString().Equals("about:blank"))
            {
                text = currentBrowser.Url.Host.ToString();
            }

            this.adrBarTextBox.Text = currentBrowser.Url.ToString();
            browserTabControl.SelectedTab.Text = text;

            img.Image = favicon(currentBrowser.Url.ToString(), "net.png");

            if (!urls.Contains(currentBrowser.Url.Host.ToString()))
                urls.Add(currentBrowser.Url.Host.ToString());

            if (!currentBrowser.Url.ToString().Equals("about:blank") && currentBrowser.StatusText.Equals("Done"))
                addHistory(currentBrowser.Url,DateTime.Now.ToString(currentCulture));
        }
        //ProgressChanged    
        private void Form1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (e.CurrentProgress < e.MaximumProgress)
                toolStripProgressBar1.Value=(int)e.CurrentProgress;
            else toolStripProgressBar1.Value = toolStripProgressBar1.Minimum;

        }
        //Navigating
        private void Form1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            this.toolStripStatusLabel1.Text = getCurrentBrowser().StatusText;

        }
        //closeTab method
        private void closeTab()
        {
            if (browserTabControl.TabCount != 2)
            {
                browserTabControl.TabPages.RemoveAt(browserTabControl.SelectedIndex);
            }

        }
        //selected index changed
        private void browserTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (browserTabControl.SelectedIndex == browserTabControl.TabPages.Count - 1) addNewTab();
            else
            {
                if (getCurrentBrowser().Url != null)
                    adrBarTextBox.Text = getCurrentBrowser().Url.ToString();
                else adrBarTextBox.Text = "about:blank";

                if (getCurrentBrowser().CanGoBack) toolStripButton1.Enabled = true;
                else toolStripButton1.Enabled = false;

                if (getCurrentBrowser().CanGoForward) toolStripButton2.Enabled = true;
                else toolStripButton2.Enabled = false;
            }
        }

        /* tab context menu */

        private void closeTabToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            closeTab();
        }
        private void duplicateTabToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
            {
                Uri dup_url = getCurrentBrowser().Url;
                addNewTab();
                getCurrentBrowser().Url = dup_url;

            }
            else addNewTab();
        }
        #endregion

        #region FAVICON
       
        // favicon
        public static Image favicon(String u, string file)
        {
                Uri url = new Uri(u);
                String iconurl = "http://" + url.Host + "/favicon.ico";

                WebRequest request = WebRequest.Create(iconurl);
                try
                {
                    WebResponse response = request.GetResponse();
                    Stream s = response.GetResponseStream();
                    return Image.FromStream(s);
                }
                catch (Exception ex)
                {
                    return Image.FromFile(file);
                }
            
           
        }
        //favicon index
        private int faviconIndex(string url)
        {
            Uri key = new Uri(url);
            if (!imgList.Images.ContainsKey(key.Host.ToString()))
                imgList.Images.Add(key.Host.ToString(), favicon(url, "link.png"));
            return imgList.Images.IndexOfKey(key.Host.ToString());
        }
        //getFavicon from key
        private Image getFavicon(string key)
        {
            Uri url = new Uri(key);
            if (!imgList.Images.ContainsKey(url.Host.ToString()))
                imgList.Images.Add(url.Host.ToString(), favicon(key
                    , "link.png"));
            return imgList.Images[url.Host.ToString()];
        }
        #endregion

        #region     TOOL CONTEXT MENU
        /* TOOL CONTEXT MENU*/
        //address bar
        private void commandBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            adrBar.Visible = !adrBar.Visible;
            this.commandBarToolStripMenuItem.Checked = adrBar.Visible;
            settings.DocumentElement.ChildNodes[1].Attributes[0].Value = adrBar.Visible.ToString();
        }
        #endregion

        #region ADDRESS BAR
        /*ADDRESS BAR*/

        private WebBrowser getCurrentBrowser()
        {
            return (WebBrowser)browserTabControl.SelectedTab.Controls[0];
        }
        //ENTER
        private void adrBarTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                getCurrentBrowser().Navigate(adrBarTextBox.Text);

            }
        }
        //select all from adr bar
        private void adrBarTextBox_Click(object sender, EventArgs e)
        {
            adrBarTextBox.SelectAll();
        }
        //show urls

        private void showUrl()
        {
            if (File.Exists(historyXml))
            {
                XmlDocument myXml = new XmlDocument();
                myXml.Load(historyXml);
                int i = 0;
                int num=int.Parse(settings.DocumentElement.ChildNodes[6].InnerText.ToString());
                foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                {
                    if (num <= i++ ) break;
                    else  adrBarTextBox.Items.Add(el.GetAttribute("url").ToString());
                           
                }
            }
        }

        private void adrBarTextBox_DropDown(object sender, EventArgs e)
        {
            adrBarTextBox.Items.Clear();
            showUrl();
        }
        //navigate on selected url 
        private void adrBarTextBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            getCurrentBrowser().Navigate(adrBarTextBox.SelectedItem.ToString());
        }
     //canGoForwardChanged
        void browser_CanGoForwardChanged(object sender, EventArgs e)
        {
            toolStripButton2.Enabled = !toolStripButton2.Enabled;
        }
        //canGoBackChanged
        void browser_CanGoBackChanged(object sender, EventArgs e)
        {
            toolStripButton1.Enabled = !toolStripButton1.Enabled;
        }
        //back  
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoBack();
        }
        //forward
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoForward();
        }
        //go
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Navigate(adrBarTextBox.Text);

        }
        //refresh
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Refresh();
        }
        //stop
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Stop();
        }
        //favorits
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            favoritesPanel.Visible = !favoritesPanel.Visible;
            settings.DocumentElement.ChildNodes[3].Attributes[0].Value = favoritesPanel.Visible.ToString();
        }
        //add to favorits
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
            {
                AddFavorites dlg = new AddFavorites(getCurrentBrowser().Url.ToString());
                DialogResult res = dlg.ShowDialog();

                if (res == DialogResult.OK)
                {
                    if (dlg.favFile == "Favorites")
                        addFavorit(getCurrentBrowser().Url.ToString(), dlg.favName);
                    else addLink(getCurrentBrowser().Url.ToString(), dlg.favName);
                }
                dlg.Close();
            }

        }
        //search
        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                if (googleSearch.Checked == true)
                    getCurrentBrowser().Navigate("http://google.com/search?q=" + searchTextBox.Text);
                else
                    getCurrentBrowser().Navigate("http://search.live.com/results.aspx?q="+searchTextBox.Text);                       
        }

        private void googleSearch_Click(object sender, EventArgs e)
        {
            liveSearch.Checked =!googleSearch.Checked;
        }

        private void liveSearch_Click(object sender, EventArgs e)
        {
            googleSearch.Checked = !liveSearch.Checked;
        }

        #endregion

        #region LINKS BAR

        /*LINKS BAR*/

        string adress, name;

        //favorits button
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            favoritesPanel.Visible = !favoritesPanel.Visible;
            settings.DocumentElement.ChildNodes[3].Attributes[0].Value = favoritesPanel.Visible.ToString();
        }
        //add to favorits bar button
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
                addLink(getCurrentBrowser().Url.ToString(), getCurrentBrowser().Url.ToString());
        }

        //showLinks on link bar
        private void showLinks()
        {
            if (File.Exists(linksXml))
            {
                XmlDocument myXml = new XmlDocument();
                myXml.Load(linksXml);
                XmlElement root = myXml.DocumentElement;
                foreach (XmlElement el in root.ChildNodes)
                {
                    ToolStripButton b =
                    new ToolStripButton(el.InnerText, getFavicon(el.GetAttribute("url")), items_Click, el.GetAttribute("url"));
                    b.ToolTipText = el.GetAttribute("url");
                    b.MouseUp += new MouseEventHandler(b_MouseUp);
                }
            }
        }
        //click link button
        private void items_Click(object sender, EventArgs e)
        {
            ToolStripButton b = (ToolStripButton)sender;
            getCurrentBrowser().Navigate(b.ToolTipText);
        }
        //show context menu on button
        private void b_MouseUp(object sender, MouseEventArgs e)
        {
            ToolStripButton b = (ToolStripButton)sender;
            adress = b.ToolTipText;
            name = b.Text;

            if (e.Button == MouseButtons.Right)
                linkContextMenu.Show(MousePosition);
        }
        #endregion

        #region LINK, FAVORITES, HISTORY CONTEXT MENU
        /*GENERAL*/

        //open
        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Navigate(adress);
        }
        //open in new tab
        private void openInNewTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addNewTab();
            getCurrentBrowser().Navigate(adress);
        }
        //open in new window
        private void openInNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WBrowser new_form = new WBrowser();
            new_form.Show();
            new_form.getCurrentBrowser().Navigate(adress);
        }
                     /*LINK CONTEXT MENU*/
        //delete link
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteLink();
        }
        //rename link
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            renameLink();
        }
                          /*FAVORITES CONTEXT MENU*/
        //delete favorit
        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            deleteFavorit();
        }
        //rename favorit
        private void renameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            renameFavorit();
        }
           
              /*HISTORY CONTEXT MENU */

        private void openToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Navigate(historyTreeView.SelectedNode.Text);
        }

//delete history
        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            deleteHistory();
        }
 //add to favorites
        private void addToFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
        {   
            AddFavorites dlg = new AddFavorites(historyTreeView.SelectedNode.Text);
             DialogResult res = dlg.ShowDialog();
                if (res == DialogResult.OK)
                {
                    if (dlg.favFile == "Favorites")
                        addFavorit(getCurrentBrowser().Url.ToString(), dlg.favName);
                    //else 
                    //    addLink(getCurrentBrowser().Url.ToString(), dlg.favName);
                    deleteHistory();
                }
                dlg.Close();

                
        }

        #endregion

        #region FAVORITES WINDOW

        private void showFavorites()
        {
            XmlDocument myXml = new XmlDocument();
            TreeNode link = new TreeNode("Links",0,0);
            link.NodeFont =new  Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            favTreeView.Nodes.Add(link);

            if (File.Exists(favXml))
            {
                myXml.Load(favXml);

                foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                {
                    TreeNode node = 
                        new TreeNode(el.InnerText,faviconIndex(el.GetAttribute("url")), faviconIndex(el.GetAttribute("url")));
                    node.ToolTipText = el.GetAttribute("url");
                    node.Name = el.GetAttribute("url");
                    node.ContextMenuStrip = favContextMenu;
                    favTreeView.Nodes.Add(node);
                }

            }

            if (File.Exists(linksXml))
            {
                myXml.Load(linksXml);

                foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
                {
                    TreeNode node = 
                        new TreeNode(el.InnerText, faviconIndex(el.GetAttribute("url")), faviconIndex(el.GetAttribute("url")));
                    node.ToolTipText = el.GetAttribute("url");
                    node.Name = el.GetAttribute("url");
                    node.ContextMenuStrip = linkContextMenu;
                    favTreeView.Nodes[0].Nodes.Add(node);
                }

            }

        }
//node click
        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                favTreeView.SelectedNode = e.Node;
                adress = e.Node.ToolTipText;
                name = e.Node.Text;
            }
            else
                if (e.Node != favTreeView.Nodes[0])
                    getCurrentBrowser().Navigate(e.Node.ToolTipText);

        }
//show history in tree wiew
        private void showHistory()
        {
            historyTreeView.Nodes.Clear();
            XmlDocument myXml = new XmlDocument();

            if (File.Exists(historyXml))
            {
                myXml.Load(historyXml);
                DateTime now=DateTime.Now;
                }
               
           if (comboBox1.Text.Equals("View By Site"))
           {
              historyTreeView.ShowRootLines = true;
              foreach(XmlElement el in myXml.DocumentElement.ChildNodes)
              { Uri site=new Uri(el.GetAttribute("url"));
                  
                  if(!historyTreeView.Nodes.ContainsKey(site.Host.ToString()))
                      historyTreeView.Nodes.Add(site.Host.ToString(),site.Host.ToString(),0,0);
                   TreeNode node =new TreeNode(el.GetAttribute("url"), 3, 3);
                    node.ToolTipText = el.GetAttribute("url") + "\nLast Visited: " + el.GetAttribute("lastVisited") + "\nTimes Visited: " + el.GetAttribute("times");
                    node.Name = el.GetAttribute("url");
                    node.ContextMenuStrip = histContextMenu;
                    historyTreeView.Nodes[site.Host.ToString()].Nodes.Add(node);
               }

           }

           if (comboBox1.Text.Equals("View by Date"))
           {  
               historyTreeView.ShowRootLines = true;
               historyTreeView.Nodes.Add("Yesterday","Yesterday",2,2);
               historyTreeView.Nodes.Add("Today","Today",2,2);
               //foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
               //{
               //   DateTime d=DateTime.Parse(el.GetAttribute("lastVisited"),currentCulture);
                  
               //    TreeNode node = new TreeNode(el.GetAttribute("url"), 3, 3);
               //    node.ToolTipText = el.GetAttribute("url") + "\nLast Visited: " + el.GetAttribute("lastVisited") + "\nTimes Visited: " + el.GetAttribute("times");
               //    node.Name = el.GetAttribute("url");
               //    node.ContextMenuStrip = histContextMenu;

               //    if (d.Date==now.Date)
               //        historyTreeView.Nodes[4].Nodes.Add(node);
               //    else
               //        if (d.AddDays(1).ToShortDateString().Equals(now.ToShortDateString()))
               //            historyTreeView.Nodes[3].Nodes.Add(node);
               //        else
               //            if (d.AddDays(7) > now)
               //                historyTreeView.Nodes[2].Nodes.Add(node);
               //            else
               //                if (d.AddDays(14) > now)
               //                    historyTreeView.Nodes[1].Nodes.Add(node);
               //                else
               //                    if (d.AddDays(21) > now)
               //                        historyTreeView.Nodes[0].Nodes.Add(node);
               //                       else
               //                        if (d.AddDays(22) > now)
               //                            myXml.DocumentElement.RemoveChild(el);
               //}
           }
         }
            
            
        
//history nodes click
        private void historyTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                historyTreeView.SelectedNode = e.Node;
                adress = e.Node.Text;
            }
            else
                if (!comboBox1.Text.Equals("Ordered Visited Today"))
                {
                    if (!historyTreeView.Nodes.Contains(e.Node))
                        getCurrentBrowser().Navigate(e.Node.Text);
                }
                else 
                    getCurrentBrowser().Navigate(e.Node.Text);
        }

//fav panel visible change
        private void favoritesPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (favoritesPanel.Visible == true)
            {
                showFavorites();
                showHistory();
            }
            else
            {
                favTreeView.Nodes.Clear();
                historyTreeView.Nodes.Clear();
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            showHistory();
        }

        #endregion

        #region FAVORITS
        /*FAVORITES*/

        //add to favorits
        private void addToFavoritsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
            {
                AddFavorites dlg = new AddFavorites(getCurrentBrowser().Url.ToString());
                DialogResult res = dlg.ShowDialog();

                if (res == DialogResult.OK)
                {
                    if (dlg.favFile == "Favorites")
                        addFavorit(getCurrentBrowser().Url.ToString(), dlg.favName);
                    else addLink(getCurrentBrowser().Url.ToString(), dlg.favName);
                }
                dlg.Close();
            }
        }
        //show favorites in menu
        private void favoritesToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            XmlDocument myXml = new XmlDocument();
            myXml.Load(favXml);

            for (int i = favoritesToolStripMenuItem.DropDownItems.Count - 1; i > 5; i--)
            {
                favoritesToolStripMenuItem.DropDownItems.RemoveAt(i);
            }
            foreach (XmlElement el in myXml.DocumentElement.ChildNodes)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(el.InnerText, getFavicon(el.GetAttribute("url")), fav_Click);
                item.ToolTipText = el.GetAttribute("url");
                favoritesToolStripMenuItem.DropDownItems.Add(item);
            }
        }
        private void fav_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem m = (ToolStripMenuItem)sender;
            getCurrentBrowser().Navigate(m.ToolTipText);
        }
        #endregion

        #region FILE
        /*FILE*/

        //new tab
        private void newTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addNewTab();
        }
        //duplicate tab
        private void duplicateTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (getCurrentBrowser().Url != null)
            {
                Uri dup_url = getCurrentBrowser().Url;
                addNewTab();
                getCurrentBrowser().Url = dup_url;

            }
            else addNewTab();
        }
        //new window
        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new WBrowser()).Show();

        }
        //close tab
        private void closeTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            closeTab();
        }
        //open
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new Open(getCurrentBrowser())).Show();
        }
        //page setup
        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPageSetupDialog();
        }
        //save as
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowSaveAsDialog();
        }
        //print
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPrintDialog();

        }
        //print preview
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPrintPreviewDialog();
        }
        //properties
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().ShowPropertiesDialog();
        }
       #endregion

        #region EDIT
        /*EDIT*/
        //cut
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Document.ExecCommand("Cut", false, null);

        }
        //copy
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Document.ExecCommand("Copy", false, null);

        }
        //paste
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Document.ExecCommand("Paste", false, null);
        }
        //select all
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Document.ExecCommand("SelectAll", true, null);
        }
        #endregion

        #region VIEW
       
        /* VIEW */


//favorites,history checked
        /*Go to*/
//drop down opening
        private void goToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            backToolStripMenuItem.Enabled = getCurrentBrowser().CanGoBack;
            forwardToolStripMenuItem.Enabled = getCurrentBrowser().CanGoForward;

            while (goToMenuItem.DropDownItems.Count > 5)
                goToMenuItem.DropDownItems.RemoveAt(goToMenuItem.DropDownItems.Count-1);
            
            foreach (string a in urls)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(a, null, goto_click);

                item.Checked = (getCurrentBrowser().Url.Host.ToString().Equals(a));

                goToMenuItem.DropDownItems.Add(item);
            }
        }
        private void goto_click(object sender, EventArgs e)
        {
            getCurrentBrowser().Navigate(sender.ToString());
        }
        //back
        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoBack();
        }
        //forward
        private void forwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().GoForward();
        }
        //home
        private void homePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Navigate(homePage);
        }
                    /*Stop*/
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Stop();
        }
                    /*Refresh*/
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getCurrentBrowser().Refresh();
        }
        //text size 
        private void textSizeToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string level = e.ClickedItem.ToString();
            smallerToolStripMenuItem.Checked = false;
            smallestToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = false;
            largerToolStripMenuItem.Checked = false;
            largestToolStripMenuItem.Checked = false;
            switch (level)
            {
                case "Smallest": getCurrentBrowser().Document.ExecCommand("FontSize", true, "0");
                                 smallestToolStripMenuItem.Checked = true;
                                 break;
                case "Smaller": getCurrentBrowser().Document.ExecCommand("FontSize", true, "1");
                                 smallerToolStripMenuItem.Checked = true;
                                 break;
                case "Medium": getCurrentBrowser().Document.ExecCommand("FontSize",true,"2");
                                 mediumToolStripMenuItem.Checked = true; 
                                break;
                case "Larger": getCurrentBrowser().Document.ExecCommand("FontSize",true,"3");
                                largerToolStripMenuItem.Checked = true; 
                                break;
                case "Largest": getCurrentBrowser().Document.ExecCommand("FontSize",true,"4");
                                largestToolStripMenuItem.Checked = true;
                                 break;
            }
        }
        //full screen
        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(this.FormBorderStyle == FormBorderStyle.None && this.WindowState == FormWindowState.Maximized))
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.TopMost = true;
                menuBar.Visible = false;
                adrBar.Visible = false;
                favoritesPanel.Visible = false;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.TopMost = false;
                menuBar.Visible = (settings.DocumentElement.ChildNodes[0].Attributes[0].Value.Equals("True"));
                adrBar.Visible = (settings.DocumentElement.ChildNodes[1].Attributes[0].Value.Equals("True"));
                favoritesPanel.Visible = (settings.DocumentElement.ChildNodes[3].Attributes[0].Value.Equals("True"));
            }
        }
      #endregion

        #region TOOLS

//delete browsing history
        private void deleteBrowserHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteBrowsingHistory b = new DeleteBrowsingHistory();
            if (b.ShowDialog() == DialogResult.OK)
            {
                if (b.History.Checked == true)
                {
                    File.Delete(historyXml);
                    historyTreeView.Nodes.Clear();
                }
                if (b.TempFiles.Checked == true)
                {
                    urls.Clear();
                    while (imgList.Images.Count > 4)
                        imgList.Images.RemoveAt(imgList.Images.Count-1);
                    File.Delete("source.txt");

                }
            }
        }
//internet options
        private void internetOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InternetOption intOp = new InternetOption(getCurrentBrowser().Url.ToString());
            if (intOp.ShowDialog() == DialogResult.OK)
            {
                if (!intOp.homepage.Text.Equals(""))
                {
                    homePage = intOp.homepage.Text;
                    settings.DocumentElement.ChildNodes[5].InnerText = intOp.homepage.Text;
                }
                    if (intOp.deleteHistory.Checked == true)
                {
                    File.Delete(historyXml);
                    historyTreeView.Nodes.Clear();
                }
                ActiveForm.ForeColor = intOp.forecolor;
                ActiveForm.BackColor = intOp.backcolor;
                adrBar.BackColor = intOp.backcolor;
                ActiveForm.Font = intOp.font;
                menuBar.Font = intOp.font;
            }


        }

        //calculator
        private void yahooMessengerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("calc.exe");
        }
        //solitaire
        private void solitaireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("sol.exe");
        }
        #endregion

        #region HELP
        //about
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new About(false)).Show();
        }
       private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("mailto:goga.claudia@gmail.com");
        }
 #endregion         

       private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
       {

       }

       private void notepadToolStripMenuItem_Click(object sender, EventArgs e)
       {
           Process.Start("mynotepad.exe");
       }

       private void englishToolStripMenuItem_Click(object sender, EventArgs e)
       {
           i = 0;
           hindiToolStripMenuItem_Click(null, null);
       }
       int i;
       private void hindiToolStripMenuItem_Click(object sender, EventArgs e)
       {
           if (hindiToolStripMenuItem.Checked == true)
           {
               i = 0;
               hindiToolStripMenuItem.Checked = false;
               englishToolStripMenuItem.Checked = true;
           }
           else
           {
               i = 1;
               hindiToolStripMenuItem.Checked = true;
               englishToolStripMenuItem.Checked = false;
           }
           switch_language();
       }
       private void switch_language()
       {
           if (hindiToolStripMenuItem.Checked == true)    //in hindi
           {
               cul = CultureInfo.CreateSpecificCulture("hi");        //create culture for hindi
           }
           else if (englishToolStripMenuItem.Checked == true)          //in english
           {
               cul = CultureInfo.CreateSpecificCulture("en");        //create culture for english
           }
           
           this.Text = res_man.GetString("WBrowser_text", cul);
           fileToolStripMenuItem.Text = res_man.GetString("File_menu", cul);
           toolToolStripMenuItem.Text = res_man.GetString("Tool_menu", cul);
           languagesToolStripMenuItem.Text = res_man.GetString("Language_menu", cul);
           hindiToolStripMenuItem.Text = res_man.GetString("Hindi_menu", cul);
           englishToolStripMenuItem.Text = res_man.GetString("English_menu", cul);
           toolbarsToolStripMenuItem.Text = res_man.GetString("Toolbars_menu", cul);
           pageSetupToolStripMenuItem .Text = res_man.GetString("Page Setup_menu", cul);
           pasteToolStripMenuItem.Text = res_man.GetString("Paste_menu", cul);
           copyToolStripMenuItem.Text = res_man.GetString("Copy_menu", cul);
           cutToolStripMenuItem.Text = res_man.GetString("Cut_menu", cul);
           selectAllToolStripMenuItem.Text = res_man.GetString("Select All_menu", cul);
           refreshToolStripMenuItem.Text = res_man.GetString("Refresh_menu", cul);
           smallerToolStripMenuItem.Text = res_man.GetString("Smaller_menu", cul);
           smallestToolStripMenuItem.Text = res_man.GetString("Smallest_menu", cul);
           largerToolStripMenuItem.Text = res_man.GetString("Larger_menu", cul);
           largestToolStripMenuItem.Text = res_man.GetString("Largest_menu", cul);
           mediumToolStripMenuItem.Text = res_man.GetString("Medium_menu", cul);
           propertiesToolStripMenuItem.Text = res_man.GetString("Properties_menu", cul);
           printToolStripMenuItem.Text = res_man.GetString("Print_menu", cul);
           textSizeToolStripMenuItem.Text = res_man.GetString("Text Size_menu", cul);
           viewToolStripMenuItem.Text = res_man.GetString("View_menu", cul);
           stopToolStripMenuItem.Text = res_man.GetString("Stop_menu", cul);
           saveAsToolStripMenuItem.Text = res_man.GetString("Save As_menu", cul);
           openToolStripMenuItem.Text = res_man.GetString("Open_menu", cul);
           openToolStripMenuItem1.Text = res_man.GetString("Open_menu", cul);
           openToolStripMenuItem2.Text = res_man.GetString("Open_menu", cul);
           openToolStripMenuItem3.Text = res_man.GetString("Open_menu", cul);
           newTabToolStripMenuItem.Text = res_man.GetString("New Tab_menu", cul);
           internetOptionsToolStripMenuItem.Text = res_man.GetString("Internet Options_menu", cul);
           helpToolStripMenuItem1.Text = res_man.GetString("Help_menu", cul);
           goToMenuItem.Text = res_man.GetString("Goto_menu", cul);
           fullScreenToolStripMenuItem.Text = res_man.GetString("Full Screen_menu", cul);
           forwardToolStripMenuItem.Text = res_man.GetString("Forward_menu", cul);
           addToFavoritsToolStripMenuItem.Text = res_man.GetString("Add To Favorites_menu", cul);
           editToolStripMenuItem.Text = res_man.GetString("Edit_menu", cul);
           duplicateTabToolStripMenuItem.Text = res_man.GetString("Duplicate Tab_menu", cul);
           deleteBrowserHistoryToolStripMenuItem.Text = res_man.GetString("Delete Browser History_menu", cul);
           closeTabToolStripMenuItem.Text = res_man.GetString("Close Tab_menu", cul);
           backToolStripMenuItem.Text = res_man.GetString("Back_menu", cul);
           adrBar .Text = res_man.GetString("AddressBar_menu", cul);
           toolStripButton3.Text = res_man.GetString("ToolStripButton_text", cul);
           toolStripButton1.Text = res_man.GetString("ToolStripButton1_text", cul);
           toolStripButton2.Text = res_man.GetString("ToolStripButton2_text", cul);
           newWindowToolStripMenuItem.Text = res_man.GetString("New Window_menu", cul);
           favoritesTabControl.Name = res_man.GetString("Favorites_menu", cul);
           historyTabPage.Text = res_man.GetString("History_menu", cul);
           helpToolStripMenuItem.Text = res_man.GetString("Help_menu", cul);
           printPreviewToolStripMenuItem.Text = res_man.GetString("Print Preview_menu", cul);
           favoritesToolStripMenuItem.Text = res_man.GetString("Favorites_menu", cul);
           openInNewTabToolStripMenuItem.Text = res_man.GetString("New_menu", cul);
           favTabPage.Text = res_man.GetString("Favorites_menu", cul);
           NewTab.Text = res_man.GetString("New_menu", cul);
           homePageToolStripMenuItem.Text = res_man.GetString("Homepage_menu", cul);
           aboutToolStripMenuItem.Text = res_man.GetString("About_menu", cul);
       }

       private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
       {

       }
    }
}
