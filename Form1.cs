using System;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    public partial class Fahrtenkosten : Form
    {
        //Globale Variablen
        public int ZeitStundeVonStart = 6;
        public int ZeitStundeVonEnde = 12;
        public int ZeitStundeBisStart = 12;
        public int ZeitStundeBisEnde = 20;
        private OleDbConnection con = new OleDbConnection();
        private OleDbCommand cmd = new OleDbCommand();
        private OleDbDataReader reader;
        private List<int> fahrtnr = new List<int>();

        public Fahrtenkosten()
        {
            InitializeComponent();
        }

        private void Fahrtenkosten_Load(object sender, EventArgs e)
        {
            //Testcode wie Liste mit Datum funktioniert
            /*List<DateTime> li = new List<DateTime>();
            string aus = "Ausgabe: ";

            lstAusgabeListe.Items.Clear();

            li.Add(DateTime.Today);
            li.Add(DateTime.Parse("21.02.2100"));
            li.Add(DateTime.Parse("21.02.2020"));

            foreach(DateTime x in li)
                 aus += x.ToShortDateString() + " ";
            lstAusgabeListe.Items.Add(aus);*/
            //Testbereich Ende

            con.ConnectionString =
                "Provider=Microsoft.ACE.OLEDB.12.0;" +
                @"Data Source=C:\Users\hmarv\Desktop\Programmieren\C#\Fahrtenschreiber\WindowsFormsApp1\Fahrtenbuch.accdb";
            cmd.Connection = con;

            for (int i = ZeitStundeVonStart; i<= ZeitStundeVonEnde; i++)
            {
                for(int k = 0; k <60; k+=10)
                {
                    //Hier kann es noch besser gemacht werden, wenn man mit dem Datentyp DateTime arbeitet und
                    //die Uhrzeit in Stunden eingibt und dann immer 10 Minuten drauf addiert
                    //Es gibt hierzu ein extra Kapitel zu "Rechnen mit Zeit"
                    CmbUhrzeitVon.Items.Add(String.Format("{0:00}",i) + ":" + String.Format("{0:00}",k));
                }
            }

            for (int i = ZeitStundeBisStart; i <= ZeitStundeBisEnde; i++)
            {
                for (int k = 0; k < 60; k += 10)
                {
                    CmbUhrzeitBis.Items.Add(String.Format("{0:00}", i) + ":" + String.Format("{0:00}", k));
                }
            }
        }

        private void AlleSehen()
        {
            try
            {
                con.Open();
                cmd.CommandText = "SELECT * FROM Fahrten";
                Ausgabe();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            con.Close();

            TxtStraßeVon.Text = "";
            TxtNummerVon.Text = "";
            TxtPLZVon.Text = "";
            TxtOrtVon.Text = "";
            TxtStraßeNach.Text = "";
            TxtNummerNach.Text = "";
            TxtPLZNach.Text = "";
            TxtOrtNach.Text = "";
        }

        private void CmdAnzeigen_Click(object sender, EventArgs e)
        {
            AlleSehen();
            //Möchte ermöglichen, dass man nach Daten filtern kann. Zum Beispiel nur die Datenbankeinträge für das Jahr 2019
        }

        private void Ausgabe()
        {
            DateTime Datum;
            DateTime UhrzeitVon;
            DateTime UhrzeitBis;

            reader = cmd.ExecuteReader();
            LstAnzeige.Items.Clear();
            fahrtnr.Clear();

            while (reader.Read())
            {
                //Datum, Uhrzeiten werden erst in Datentyp konvertiert 
                //und später in gewünschtem Format angezeigt
                Datum = Convert.ToDateTime(reader["Datum"]);
                UhrzeitVon = Convert.ToDateTime(reader["UhrzeitVon"]);
                UhrzeitBis = Convert.ToDateTime(reader["UhrzeitBis"]);

                LstAnzeige.Items.Add(Datum.ToShortDateString() + " # " +
                    reader["FavNr"] + " # " +
                    reader["GrundNr"] + " # " +
                    UhrzeitVon.ToShortTimeString() + " # " +
                    UhrzeitBis.ToShortTimeString() + " # " +
                    reader["Dauer"] + " # " +
                    reader["Pendlerpauschale"] + " # " +
                    reader["VMA"] + " # " +
                    reader["WK"]);
                fahrtnr.Add((int)reader["FahrtNr"]);
            }

            reader.Close();

        }

        private void CmdFahrtEintragen_Click(object sender, EventArgs e)
        {
            int anzahl;
            DateTime Datum;

            try
            {
                Datum = Convert.ToDateTime(DtpDatum.Value);
                con.Open();
                cmd.CommandText = "INSERT INTO Fahrten ( Datum, FavNr, GrundNr, UhrzeitVon, UhrzeitBis, Dauer, " +
                    "Pendlerpauschale, VMA, WK) VAlUES ('" + Datum.ToShortDateString() + "',1,1, '" + CmbUhrzeitVon.Text + "'," +
                    "'" + CmbUhrzeitBis.Text + "'," +
                    " 7,0.6,24,300)";
                Ausgabe();
                MessageBox.Show(cmd.CommandText);

                anzahl = cmd.ExecuteNonQuery();
                if (anzahl > 0)
                    MessageBox.Show("Ein Datensatz eingefügt");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Bitte mindestens ein Datum, Von und Zu, Dauer, Pendlerpauschale, VMA, WK eintragen");
            }

            con.Close();
            AlleSehen();

           
        }

        private void CmdGrund_Click(object sender, EventArgs e)
        {
            int anzahl;
            if (TxtGrundEingabe.Text != "")
            {
                try
                {
                    con.Open();

                    cmd.CommandText = "INSERT INTO Gruende (Grund) VAlUES ('" + TxtGrundEingabe.Text + "')";
                    MessageBox.Show(cmd.CommandText);

                    anzahl = cmd.ExecuteNonQuery();
                    if (anzahl > 0)
                        MessageBox.Show("Ein Datensatz eingefügt");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    MessageBox.Show("Bitte einen Grund eintragen");
                }

                con.Close();
            }
            else
                MessageBox.Show("Es wurde kein Grund eingetragen");
               

            
        }

        private void LstAnzeige_SelectedIndexChanged(object sender, EventArgs e)
        {

            DateTime Datum;
            DateTime UhrzeitVon;
            DateTime UhrzeitBis;

            try
            {
                con.Open();
                cmd.CommandText = "SELECT * FROM Fahrten WHERE " +
                    "FahrtNr = " + fahrtnr[LstAnzeige.SelectedIndex];

                reader = cmd.ExecuteReader();
                reader.Read();

                CmbFavorit.Text = "" + reader["FavNr"];

                Datum = Convert.ToDateTime(reader["Datum"]);
                DtpDatum.Text = Datum.ToShortDateString();
                UhrzeitVon = Convert.ToDateTime(reader["UhrzeitVon"]);
                CmbUhrzeitVon.Text = UhrzeitVon.ToShortTimeString();
                UhrzeitBis = Convert.ToDateTime(reader["UhrzeitBis"]);
                CmbUhrzeitBis.Text = UhrzeitBis.ToShortTimeString();

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            con.Close();
        }

        private void CmdAendern_Click(object sender, EventArgs e)
        {
            if(LstAnzeige.SelectedIndex == -1)
            {
                MessageBox.Show("Bitte einen Datensatz auswählen");
                return;
            }

            try
            {
                con.Open();
                cmd.CommandText = "UPDATE Fahrten SET Datum = '" + DtpDatum.Value + "' " +
                    ", UhrzeitVon = '" + CmbUhrzeitVon.Text + " ', UhrzeitBis = '" + CmbUhrzeitBis.Text + "', " +
                    "FavNr = '" + CmbFavorit.Text + "' WHERE " + "FahrtNr = " + fahrtnr[LstAnzeige.SelectedIndex];
                MessageBox.Show(cmd.CommandText);

                int anzahl = cmd.ExecuteNonQuery();
                if (anzahl > 0)
                    MessageBox.Show("Datensatz geändert");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Bitte einen Datensatz auswählen und gültige Eingaben machen");
            }

            con.Close();
            AlleSehen();
        }

        private void CmdLoeschen_Click(object sender, EventArgs e)
        {
            if(LstAnzeige.SelectedIndex == -1)
            {
                MessageBox.Show("Bitte einen Datensatz auswählen");
                return;
            }

            if (MessageBox.Show("Wollen Sie den ausgewählten " +
                            "Datensatz wirklich löschen?", "Löschen", 
                            MessageBoxButtons.YesNo) == DialogResult.No)
                            return;
            try
            {
                con.Open();
                cmd.CommandText = "DELETE FROM Fahrten WHERE " +
                    "FahrtNr = " + fahrtnr[LstAnzeige.SelectedIndex];
                MessageBox.Show(cmd.CommandText);

                int anzahl = cmd.ExecuteNonQuery();
                if (anzahl > 0)
                    MessageBox.Show("Datensatz gelöscht");

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            con.Close();
            AlleSehen();
        }

        private void CmdSuchen_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                DateTime DatumSuche;
                DatumSuche = Convert.ToDateTime("01.01." + TxtSuche.Text);
                //Nach Fav Nr Suchen
                cmd.CommandText = "SELECT * FROM Fahrten WHERE " +
                    "FavNr =  " + TxtSuche.Text;
                //Nach Datum suchen
                //cmd.CommandText = "SELECT * FROM Fahrten WHERE " +
                //    "Datum =  " + DatumSuche.ToShortDateString();
                MessageBox.Show(cmd.CommandText);
                //Ausgabe();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            con.Close();
        }

        private void CmdDatumAnzeigen_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                cmd.CommandText = "SELECT * FROM Fahrten ORDER BY Datum";
                DateTime Datum;
                DateTime UhrzeitVon;
                DateTime UhrzeitBis;

                reader = cmd.ExecuteReader();
                LstAnzeige.Items.Clear();
                fahrtnr.Clear();

                while (reader.Read())
                {
                    //Datum, Uhrzeiten werden erst in Datentyp konvertiert 
                    //und später in gewünschtem Format angezeigt
                    Datum = Convert.ToDateTime(reader["Datum"]);
                    UhrzeitVon = Convert.ToDateTime(reader["UhrzeitVon"]);
                    UhrzeitBis = Convert.ToDateTime(reader["UhrzeitBis"]);


                    LstAnzeige.Items.Add(Datum.ToShortDateString() + " # " +
                        reader["FavNr"] + " # " +
                        reader["GrundNr"] + " # " +
                        reader["Dauer"] + " # " +
                        reader["VMA"] + " # " +
                        reader["WK"]);
                    fahrtnr.Add((int)reader["FahrtNr"]);
                }

                reader.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            con.Close();
        }

        private void CmdFavoritEintragen_Click(object sender, EventArgs e)
        {
            int anzahl;
            //string HausnummerVon;

            //HausnummerVon = TxtNummerVon.Text;

            try
            {
                con.Open();
                //cmd.CommandText = "INSERT INTO Favoriten (FavName," + //1
                //    "StraßeVon," + //2
                //    "HausnummerVon," + //3
                //    "PLZVon," + //4
                //    "OrtVon," + //5
                //    "StraßeNach," + //6
                //    "HausnummerNach," + //7
                //    "PLZNach ," + //8
                //    "OrtNach) " + //9
                //    "VAlUES ('" + TxtFavName.Text + "', " + //1
                //    "'" + TxtStraßeVon.Text + "', " + //2
                //    "'" + TxtNummerVon.Text + "', " + //3
                //    "'" + TxtPLZVon.Text + "', " + //4
                //    "'" + TxtOrtVon.Text + "'," + //5
                //    "'" + TxtStraßeNach.Text + "', " + //6
                //    "'" + TxtNummerNach.Text + "', " + //7
                //    "'" + TxtPLZNach.Text + "', " + //8
                //    "'" + TxtOrtNach.Text + "')"; //9
                cmd.CommandText = "INSERT INTO Favoriten (FavName, StraßeVon, HausnummerVon, PLZVon, OrtVon, StraßeNach, " +
                    "HausnummerNach, PLZNach, OrtNach, EK) " +
                    "VALUES ('" + TxtFavName.Text + "', '" + TxtStraßeVon.Text + "','" + TxtNummerVon.Text + "', '" + TxtPLZVon.Text + "', " +
                    "'" + TxtOrtVon.Text + "' , '" + TxtStraßeNach.Text + "','" + TxtNummerNach.Text + "', '" + TxtPLZNach.Text + "', " +
                    "'" + TxtOrtNach.Text + "', '" + TxtEK.Text + "')";

                //cmd.CommandText = "INSERT INTO Favoriten (FavName)" +
                //   "VAlUES ('" + TxtFavName.Text + "')";

                Ausgabe();
                MessageBox.Show(cmd.CommandText);

                anzahl = cmd.ExecuteNonQuery();
                if (anzahl > 0)
                    MessageBox.Show("Ein Datensatz eingefügt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show("Bitte mindestens ein Datum, Von und Zu, Dauer, Pendlerpauschale, VMA, WK eintragen");
            }

            con.Close();
            AlleSehen();
        }
    }
    
}
