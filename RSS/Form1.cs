using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
[assembly: CLSCompliant(true)]
namespace Rss{
    public partial class Form1 : Form{
        List<KeyValuePair<DateTime, News>> listNews;
        SortedList<int,Uri> sourceList;
        BackgroundWorker bk;
        System.Windows.Forms.Timer tm;
        private struct News: IEquatable<News>{
            public string site { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string link { get; set; }
            public override int GetHashCode(){
                return (site + title + description + link).GetHashCode();}
            public override bool Equals(object obj){
                if(!(obj is News)) return false;
                return Equals((News)obj);}
            public bool Equals(News n){
                if (site == n.site && title == n.title && description == n.description && link == n.link)
                    return true;
                else return false;}}
        public Form1(){
            InitializeComponent();
            bk = new BackgroundWorker();
            sourceList = new SortedList<int,Uri>();
            listNews = new List<KeyValuePair<DateTime, News>>();
            listNews = parseXML(new Uri(@"C:\Users\Евгений\Documents\Visual Studio 2013\Projects\RssLenta\RssLenta\bin\Debug\ex2.xml"));
            viewListNews();
            tm = new System.Windows.Forms.Timer();
            tm.Interval = 600000;
            tm.Tick += tm_Tick;
            bk.DoWork += bk_DoWork;
            bk.RunWorkerCompleted += bk_RunWorkerCompleted;
            tm.Start();}
        void bk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e){
            if (e.Error != null) MessageBox.Show(e.Error.Message);
            else {
                toolStripStatusLabel1.Text = "Загрузка завершена.";
                listNews.Clear();
                listNews = e.Result as List<KeyValuePair<DateTime, News>>;
                viewListNews();}}
        void bk_DoWork(object sender, DoWorkEventArgs e){
            toolStripStatusLabel1.Text = "Пожалуйста подождите, идет загрузка.";
            BackgroundWorker worker = sender as BackgroundWorker;
            List<KeyValuePair<DateTime, News>>  tmp = new List<KeyValuePair<DateTime, News>>();
            foreach (KeyValuePair<int,Uri> url in (SortedList<int,Uri>)e.Argument)
                tmp = parseXML(url.Value).Union(tmp).ToList();
            tmp.Sort(Compare);
            e.Result = tmp;}
        void tm_Tick(object sender, EventArgs e){
            if (!bk.IsBusy){bk.RunWorkerAsync(sourceList);}}
        private void Form1_Load(object sender, EventArgs e){}
        private void viewListNews(){
            listView1.Items.Clear();
            foreach (KeyValuePair<DateTime, News> kvp in listNews){
                ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
                lvsi.Text = kvp.Value.site;
                ListViewItem lvi = new ListViewItem();
                lvi.Text = kvp.Value.title;
                lvi.ToolTipText = kvp.Value.description;
                lvi.SubItems.Add(lvsi);
                lvsi = new ListViewItem.ListViewSubItem();
                lvsi.Text = kvp.Key.ToString();
                lvi.SubItems.Add(lvsi);
                listView1.Items.Add(lvi);}}
        private List<KeyValuePair<DateTime, News>> parseXML(Uri url){
            List<KeyValuePair<DateTime, News>> t = new List<KeyValuePair<DateTime, News>>();
            XmlDocument xd = new XmlDocument();
            try{xd.Load(url.ToString());}
            catch (System.Net.WebException webEx) {
                MessageBox.Show(webEx.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.sourceList.RemoveAt(url.GetHashCode());}
            foreach (XmlNode node in xd.SelectNodes("rss"))
                foreach (XmlNode node2 in node.SelectNodes("channel"))
                    foreach (XmlNode child in node2.SelectNodes("item")){
                        News tmp = new News();
                        tmp.description = child.SelectNodes("description").Item(0).InnerText;
                        tmp.link = child.SelectNodes("link").Item(0).InnerText;
                        tmp.site = node2.SelectNodes("title").Item(0).InnerText;
                        tmp.title = child.SelectNodes("title").Item(0).InnerText;
                        try { t.Add(new KeyValuePair<DateTime, News>(DateTime.Parse(child.SelectNodes("pubDate").Item(0).InnerText), tmp)); }
                        catch (Exception ex) { MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error); }}
            return t;}
        private void addBtnSource_Click(object sender, EventArgs e){
            try{
                Uri URL = new Uri(addSource.Text);
                if (URL.Scheme == "http" || URL.Scheme == "https"){
                    sourceList.Add(URL.GetHashCode(),URL);
                    bk.RunWorkerAsync(sourceList);}
                else {MessageBox.Show("Неверно задан адрес!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);};}
            catch (Exception exp) { MessageBox.Show(exp.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error); }}
        private int Compare(KeyValuePair<DateTime, News> kvp1, KeyValuePair<DateTime, News> kvp2){
            if (kvp1.Equals(null)){
                if (kvp2.Equals(null)) return 0;
                else return 1;}
            else {
                if (kvp2.Equals(null)) return -1;
                else { int retval = kvp1.Key.CompareTo(kvp2.Key);
                    if (retval != 0)return -retval;
                    else return -kvp1.Key.CompareTo(kvp2.Key);}}}
        private void listView1_DoubleClick(object sender, EventArgs e){
            if (e != null) {
                ListView lv = (ListView)sender;
                Process proc = new Process();
                proc.StartInfo= new ProcessStartInfo(listNews[lv.FocusedItem.Index].Value.link);
                proc.Start();}}
        private void toolStripMenuItem1_Click(object sender, EventArgs e) { changeInterval(600000); }
        private void toolStripMenuItem2_Click(object sender, EventArgs e) { changeInterval(300000); }
        private void toolStripMenuItem3_Click(object sender, EventArgs e) { changeInterval(150000); }
        private void toolStripMenuItem4_Click(object sender, EventArgs e) { changeInterval(50000); }
        private void changeInterval(int time){
            tm.Stop();
            tm.Interval = time;
            tm.Start();}}}