using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace annealing
{
    public partial class Form1 : Form
    {
        List<Point> cities = new List<Point>();
        List<int> roads = new List<int>();

        List<Point> def_cities;
        List<int> def_roads;

        Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0));

            foreach (var citie in cities)
            {
                e.Graphics.DrawEllipse(pen, citie.X, citie.Y, 3, 3);
            }

            //foreach (var road in roads)
            for (int i = 0; i < roads.Count-1; i++)
            {
                e.Graphics.DrawLine(pen, cities[roads[i]].X, cities[roads[i]].Y, cities[roads[i + 1]].X, cities[roads[i+1]].Y);
            }
            if (cities.Count > 0)
                e.Graphics.DrawLine(pen, cities[roads[0]].X, cities[roads[0]].Y, cities[roads[roads.Count - 1]].X, cities[roads[roads.Count - 1]].Y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            roads.Clear();
            cities.Clear();

            int count = Convert.ToInt32(tb_Count.Text);
            for (int i = 0; i < count; i++)
            {
                cities.Add(new Point(rnd.Next(1, panel1.Size.Width-1), rnd.Next(1, panel1.Size.Height-1)));
            }

            for (int i = 0; i < cities.Count; i++)
            {
                roads.Add(i);
            }
            //roads.Add(0);


            label1.Text = CalculateEnergy(roads).ToString();

            def_cities = new List<Point>(cities);
            def_roads = new List<int>(roads);

            panel1.Refresh();
        }



        private void button2_Click(object sender, EventArgs e)
        {
            SimulatedAnnealing(10, 0.01f);//0.00001

            panel1.Refresh();
        }


        void Reset_to_default_state()
        {
            cities = new List<Point>(def_cities);
            roads = new List<int>(def_roads); 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Reset_to_default_state();

            panel1.Refresh();
        }



        private void button4_Click(object sender, EventArgs e)
        {
            /*List<int> buff_roads = new List<int>(roads);
            float buff_Energy;
            float min_buff_Energy = CalculateEnergy(roads);

            for (int i = 0; i < Convert.ToInt32(textBox2.Text); i++)
            {
                SimulatedAnnealing(10, 0.00001f);


                buff_Energy = CalculateEnergy(roads);
                if (buff_Energy < min_buff_Energy)
                {
                    buff_roads = new List<int>(roads);
                    min_buff_Energy = buff_Energy;
                }

                Reset_to_default_state();
            }*/
            chart1.Series["MainGraphic"].Points.Clear();

            for (int i = 0; i < Convert.ToInt32(textBox2.Text); i++)
            {
                this.Text = i.ToString();
                button2.PerformClick();


                chart1.Series["MainGraphic"].Points.AddXY(i, Convert.ToDouble(label3.Text));
            }

            

            label5.Text = (Math.Round(((Convert.ToDouble(label1.Text) - Convert.ToDouble(label3.Text)) / Convert.ToDouble(label1.Text)), 3)).ToString() + "%";


            //label2.Text = CalculateEnergy(roads).ToString();

            panel1.Refresh();
        }


        #region Optimization

        //===========Optimaze=========================================

        float DecreaseTemperature(float initialTemperature, int i)
        {
            
            

            //initialTemperature - начальная температура
            //i - номер итерации

            return initialTemperature * 0.1f / (float)i;



            //float D = roads.Count;
            //float C = 1.1f;
            //return initialTemperature * (float)Math.Exp(-C * Math.Pow(i, (1 / D)));
        }

        float GetTransitionProbability(float dEm, float T)
        {
            return (float)Math.Exp(-dEm/T);
        }

        bool IsTransition(float probability)
        {
            float value = rnd.Next(1000) / 1000f;//1000
            return (value <= probability) ? true : false;
        }

        float distance(Point p1, Point p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        List<int> GenerateStateCandidate(List<int> state)
        {
            //int count = rnd.Next(0, state.Count/2);
            List<int> buf = new List<int>(state);


            int c1 = rnd.Next(roads.Count);
            int c2 = rnd.Next(roads.Count);


            if (c1 < c2)
                buf.Reverse(c1, c2 - c1 + 1);
            else
                buf.Reverse(c2, c1 - c2 + 1);

            return buf;
        }

        float CalculateEnergy(List<int> state)
        {
            int n = state.Count;
            float E = 0;

            for (int i = 0; i < state.Count-1; i++)
            {
                E = E + distance(cities[state[i]], cities[state[i+1]]);
            }
            E = E + distance(cities[state[0]], cities[state[state.Count - 1]]);

            return E;

        }


        List<int> min_roads;
        float min_energy;

        void SimulatedAnnealing(float initialTemperature, float endTemperature)
        {
            chart1.Series[0].Points.Clear();


            int n = cities.Count;

            float currentEnergy = CalculateEnergy(roads);
            min_energy = currentEnergy;
            min_roads = new List<int>(roads);
            label2.Text = currentEnergy.ToString();

            float T = initialTemperature;
    
            int i = 0;
            while ((i < Convert.ToInt32(textBox1.Text)) || (checkBox2.Checked && (T >= endTemperature)))
            {
                i++;
                List<int> stateCandidate = GenerateStateCandidate(roads);
                float candidateEnergy = CalculateEnergy(stateCandidate);
        
                if(candidateEnergy < currentEnergy)
                {
                    currentEnergy = candidateEnergy;
                    roads = stateCandidate;
                }
                else
                {
                    float p = GetTransitionProbability(candidateEnergy-currentEnergy, T);
                    if (IsTransition(p))
                    {
                        currentEnergy = candidateEnergy;
                        roads = stateCandidate;
                    }
                }

                T = DecreaseTemperature(initialTemperature, i);

                chart1.Series["MainGraphic"].Points.AddXY(i, currentEnergy);
                chart1.Invalidate();

                
                if (min_energy > currentEnergy)
                {

                    min_energy = currentEnergy;
                    min_roads = new List<int>(roads);
                }

                if (checkBox1.Checked)
                {
                    label3.Text = CalculateEnergy(roads).ToString();
                    label3.Refresh();
                    label4.Text = ((decimal)T).ToString();
                    label4.Refresh();
                }

            }


            roads = new List<int>(min_roads);

            label3.Text = CalculateEnergy(roads).ToString();


            //panel1.Refresh();
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            panel1.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            float max_dist = float.MinValue;
            int c1 = 0;
            int c2 = 0;
            float dist;
            int q1 = 0;
            int q2 = 0;


            for (int i = 0; i < roads.Count-1; i++)
			{
			 
                dist = distance(cities[roads[i]], cities[roads[i+1]]);
                if (max_dist < dist)
                {
                    max_dist = dist;
                    c1 = roads[i];
                    c2 = roads[i + 1];
                    q1 = i;
                    q2 = i + 1;
                }
            }
            dist = distance(cities[roads[0]], cities[roads[roads.Count - 1]]);
            if (max_dist < dist)
            {
                max_dist = dist;
                q1 = 0;
                q2 = roads.Count - 1;
                c1 = roads[0];
                c2 = roads[roads.Count - 1];
            }


            MessageBox.Show(q1.ToString() + " " + q2.ToString() + " " + c1.ToString() + " " + c2.ToString() + " " + max_dist.ToString());
        }

        private void label5_Click(object sender, EventArgs e)
        {
            label5.Text = (Math.Round(((Convert.ToDouble(label1.Text) - Convert.ToDouble(label3.Text)) / Convert.ToDouble(label1.Text)), 3)).ToString() + "%";
        }



        //===========Optimaze=========================================
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.PerformClick();
        }
    }
}