using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace WifiControl
{
    public partial class WifiControl : Form
    {
        private string identificador;
        private string clave;
        private bool estado;        
        public WifiControl()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            this.ActualizarEstadosYComponentes();
            this.CargarConfiguracionIdentificador();
        }

        private void CargarConfiguracionIdentificador()
        {
            try
            {
                StreamReader fichero = new StreamReader("Wificontrol.conf");
                textBoxIdentificador.Text = fichero.ReadLine();
                fichero.Close();
            }
            catch (Exception ex)
            {

            }            
        }

        private void SalvarConfiguracionIdentificador()
        {
            try
            {
                StreamWriter fichero = new StreamWriter("Wificontrol.conf");
                fichero.WriteLine(this.identificador);
                fichero.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private void ActualizarEstadosYComponentes()
        {
            this.ActualizarEstadoActualServicioWifi();
            if (this.estado)
            {
                labelEstadoBandera.ForeColor = Color.Green;
                labelEstadoBandera.Text = "Activo";
                button1.Text = "Desactivar";
                textBoxIdentificador.Enabled = false;
                textBoxClave.Enabled = false;
            }
            else
            {
                labelEstadoBandera.ForeColor = Color.Red;
                labelEstadoBandera.Text = "Inactivo";
                button1.Text = "Activar";
                textBoxIdentificador.Enabled = true;
                textBoxClave.Enabled = true;
            }
        }

        private void ActualizarEstadoActualServicioWifi()
        {
            try
            {
                string[] salidaArreglo = this.EjecutarProceso("netsh", "wlan show interfaces").Split(':');
                string salida = salidaArreglo[salidaArreglo.Length - 1];
                this.estado = (salida.IndexOf("No") == -1) ? true : false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK,MessageBoxIcon.Error);
            }            
        }

        private string EjecutarProceso(string pproceso, string pparametros)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(pproceso, pparametros);            
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process proceso = Process.Start(startInfo);
            proceso.WaitForExit();
            string error = proceso.StandardError.ReadToEnd();
            if (error != null && error != "")
            {
                throw new Exception("Ha ocurido un error al ejecutar el proceso '" + pproceso + "'\n" + "Detalles:\n" + "Error: " + error);
            }
            else
            {
                return proceso.StandardOutput.ReadToEnd();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.estado)
                {
                    this.EjecutarProceso("netsh", "wlan stop hostednetwork");
                    this.ActualizarEstadosYComponentes();                               
                }else
                {
                    this.identificador = textBoxIdentificador.Text;
                    this.clave = textBoxClave.Text;

                    if (this.identificador == "")
                    {
                        throw new Exception("Debe especificar un identificador para la red.");
                    }

                    if (this.clave.Length < 8)
                    {
                        throw new Exception("Debe especificar una clave con 8 o más caracteres.");
                    }

                    this.EjecutarProceso("netsh", "wlan set hostednetwork mode=allow ssid="+this.identificador+" key="+this.clave);
                    this.EjecutarProceso("netsh", "wlan start hostednetwork");
                    this.ActualizarEstadosYComponentes();
                    this.SalvarConfiguracionIdentificador();                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void timerWifiControl_Tick(object sender, EventArgs e)
        {
            this.ActualizarEstadosYComponentes();
        }

        private void WifiControl_FormClosed(object sender, FormClosedEventArgs e)
        {
            timerWifiControl.Stop();
        }
    }
}
