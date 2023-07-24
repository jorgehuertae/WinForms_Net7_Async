using System.Diagnostics;

namespace WinForms_Net7_Async
{
    public partial class Form1 : Form
    {
        HttpClient httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        //peligroso async avoid debe ser evitado, excepto en eventos
        private async void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            //MessageBox.Show("presionado");
            //Thread.Sleep(5000);
            //await Task.Delay(5000);
            await ProcesamientoLargo();
            pictureBox1.Visible = false;
        }

        private async Task ProcesamientoLargo()
        {
            await Task.Delay(3000);
        }

        private async Task<string> ProcesamientoLargo2()
        {
            await Task.Delay(3000);
            return "felipe";
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            var nombre = await ProcesamientoLargo2();
            MessageBox.Show($"hola {nombre}");
            pictureBox1.Visible = false;
        }

        private async Task ProcesamientoLargoA()
        {
            await Task.Delay(1000);
            Console.WriteLine("Proceso A finalizado");
        }

        private async Task ProcesamientoLargoB()
        {
            await Task.Delay(1000);
            Console.WriteLine("Proceso B finalizado");
        }

        private async Task ProcesamientoLargoC()
        {
            await Task.Delay(1000);
            Console.WriteLine("Proceso C finalizado");
        }

        private async void button3_Click(object sender, EventArgs e)
        {

            pictureBox1.Visible = true;
            var sw = new Stopwatch();
            sw.Start();

            await ProcesamientoLargoA();
            await ProcesamientoLargoB();
            await ProcesamientoLargoC();

            sw.Stop();

            var duracion = $"tiempo ejecucion: {sw.ElapsedMilliseconds / 1000.0} segundos";
            Console.WriteLine(duracion);
            pictureBox1.Visible = false;
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            var sw = new Stopwatch();
            sw.Start();
            var tareas = new List<Task>()
            {
                ProcesamientoLargoA(),
                ProcesamientoLargoB(),
                ProcesamientoLargoC()
            };
            await Task.WhenAll(tareas);

            sw.Stop();

            var duracion = $"tiempo ejecucion: {sw.ElapsedMilliseconds / 1000.0} segundos";
            Console.WriteLine(duracion);
            pictureBox1.Visible = false;
        }

        private async Task ProcesarImagen(string directorio, Imagen imagen)
        {
            var respuesta = await httpClient.GetAsync(imagen.URL);

            var contenido = await respuesta.Content.ReadAsByteArrayAsync();

            Bitmap bitmap;

            using (var ms = new MemoryStream(contenido))
            {
                bitmap = new Bitmap(ms);
            }

            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

            var destino = Path.Combine(directorio, imagen.Nombre);

            bitmap.Save(destino);
        }

        private static List<Imagen> ObtenerImagenes()
        { 
            var imagenes = new List<Imagen>();
            for (int i = 0; i < 7; i++)
            {
                imagenes.Add(
                    new Imagen() { 
                                    Nombre=$"Cacicazgos{i}.png", 
                                    URL= "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/Copia_de_Cacicazgos_de_la_Hispaniola.png/800px-Copia_de_Cacicazgos_de_la_Hispaniola.png" 
                                  }
                    );

                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Desangles{i}.png",
                        URL = "https://upload.wikimedia.org/wikipedia/commons/4/43/Desangles_Colon_engrillado.jpg"
                    }
                    );

                imagenes.Add(
                    new Imagen()
                    {
                        Nombre = $"Alcazar{i}.png",
                        URL = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d7/Santo_Domingo_-_Alc%C3%A1zar_de_Col%C3%B3n_0777.JPG/800px-Santo_Domingo_-_Alc%C3%A1zar_de_Col%C3%B3n_0777.JPG"
                    }
                    );
            }
            return imagenes;
        }

        private void BorrarArchivos(string directorio)
        {
            var archivos = Directory.EnumerateFiles(directorio);

            foreach (var item in archivos)
            {
                File.Delete(item);
            }
        }

        private void PrepararEjecucion(string destinoBaseParalelo, string destinoBaseSecuencial)
        {
            if (!Directory.Exists(destinoBaseParalelo))
            {
                Directory.CreateDirectory(destinoBaseParalelo);
            }

            if (!Directory.Exists(destinoBaseSecuencial))
            {
                Directory.CreateDirectory(destinoBaseSecuencial);
            }

            BorrarArchivos(destinoBaseParalelo);

            BorrarArchivos(destinoBaseSecuencial);
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            
            var directorioActual=AppDomain.CurrentDomain.BaseDirectory;

            var destinoSecuencial = Path.Combine(directorioActual, @"Imagenes\resultado-secuencial");

            var destinoParalelo = Path.Combine(directorioActual, @"Imagenes\resultado-paralelo");

            PrepararEjecucion(destinoParalelo, destinoSecuencial);

            Console.WriteLine("Inicio");

            List<Imagen> imagenes = ObtenerImagenes();
            
            
            var sw=new Stopwatch();

            //parte secuencial

            sw.Start();

            foreach (var imagen in imagenes)
            {
                await ProcesarImagen(destinoSecuencial, imagen);
            }

            Console.WriteLine("Secuencial - duracion segundos : {0}", sw.ElapsedMilliseconds/1000.0);

            sw.Reset();

            //parte paralelo
            sw.Start();
            
            var tareasEnumerable = 
                imagenes.Select(
                    async imagen => { 
                                    await ProcesarImagen(destinoParalelo, imagen);
                                    });
            
            await Task.WhenAll(tareasEnumerable);

            Console.WriteLine("Paralelo - duracion segundos : {0}", sw.ElapsedMilliseconds / 1000.0);

            sw.Stop();

            pictureBox1.Visible = false;
        }
    }
}