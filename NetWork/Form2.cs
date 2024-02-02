using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting.Channels;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static NetWork.Form2;

namespace NetWork
{
    public partial class Form2 : Form
    {
        // ���������� �������� ��� ������� ��������� �����
        private int nodesCoordsColumns = 3;

        // �����, �������������� ���������� ����� ������
        public class Connection
        {
            // ��������� ���� ����������
            public Button StartNode { get; set; }

            // �������� ���� ����������
            public Button EndNode { get; set; }

            // ����, �����������, �������� �� ����������
            public bool IsSaturated { get; set; }

            // ����, �����������, �� �������� �� ����������
            public bool IsUnsaturated { get; set; }
        }

        // ���������� ������ �����
        public int centerX;

        // ���������� ������ �����
        public int centerY;

        // ������ ���������� ����� ������
        private List<Connection> connections = new List<Connection>();

        // ����, �����������, ������ �� ������������ ����
        bool checkedFullGraph = false;

        // ����, �����������, ����������� �� ���������� ����� ������
        bool checkedConnections = false;

        // ����� �������� ������� ���������� ������������
        double sum;

        // ����� �������
        double sumTraffic;

        // �������� ���������� D
        double D;


        public Form2()
        {
            InitializeComponent();
            Storage.Data = new StorageData();
            this.KeyPreview = true;

        }



        private void Element_Click(object sender)
        {
            // �������� ������� �����
            DateTime currentTime = DateTime.Now;

            // ���������� ��� ��������, �� ������� ��� ������ ����
            string elementType = sender.GetType().Name;

            // �������� ����� ��������, ���� �� ������������ �������� Text
            string elementText = (sender is Control control) ? control.Text : "";

            // ������� ������ � ����������� � �������
            string clickInfo = $"{currentTime}: ��� ��������: {elementType}, ����� ��������: {elementText}";

            // ������ ���� � ����� ��� ������ ����������
            string filePath = "log.txt";

            // �������� ���������� � ����
            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine("�������: Click");
                    writer.WriteLine(clickInfo);
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Element_ChangeCoords(double coord1, double coord2, object sender)
        {
            // �������� ������� �����
            DateTime currentTime = DateTime.Now;

            // ���������� ��� ��������, �� ������� ��� ������ ����
            string elementType = sender.GetType().Name;

            // �������� ����� ��������, ���� �� ������������ �������� Text
            string elementText = (sender is Control control) ? control.Tag.ToString() : "";

            // ������� ������ � ����������� � �������
            string clickInfo = $"{currentTime}: �� �������� ���������� ���� {elementText}. X = {coord1} Y = {coord2}";

            // ������ ���� � ����� ��� ������ ����������
            string filePath = "log.txt";

            // �������� ���������� � ����
            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine("�������: ��������� ���������");
                    writer.WriteLine(clickInfo);
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Element_ChangeCoords_dataGridView(double value, double coord, int index)
        {
            // �������� ������� �����
            DateTime currentTime = DateTime.Now;

            string coordinateXY;

            if (coord == 1)
            {
                coordinateXY = "X";
            }
            else
            {
                coordinateXY = "Y";
            }


            // ������� ������ � ����������� � �������
            string clickInfo = $"�� �������� ���������� ���� {index}. ������� ���������� {coordinateXY} ����� {value}";

            // ������ ���� � ����� ��� ������ ����������
            string filePath = "log.txt";

            // �������� ���������� � ����
            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine("�������: ��������� ���������");
                    writer.WriteLine(clickInfo);
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {

            }
        }



        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // ������� �������������� ���������� ������ � ����� � ��������� ����� ����� � ���������
                Storage.Data.nodesNumber = int.Parse(textBox1.Text);
                Storage.Data.nodesNumberNoTopology = int.Parse(textBox1.Text);

                // �������� �� ����������� ����� �����
                if (Storage.Data.nodesNumber <= 0)
                {
                    MessageBox.Show("���������� ����� ������ ���� ����������� ������!");
                    return;
                }

                // �������� �� ���������� ����� ������ ������
                if (Storage.Data.nodesNumber == 1)
                {
                    MessageBox.Show("���������� ����� ������ ���� ������ ������!");
                    return;
                }

                // ������������� ������ ��������� �����
                Storage.Data.nodesCoords = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
                Storage.Data.nodesCoordsNoTopology = new double[Storage.Data.nodesNumber, nodesCoordsColumns];

                // �������� ����� ��� ����� ���������
                Coords coords = new Coords();
                Element_Click(sender);
                coords.ShowDialog();
            }
            catch (Exception ex)
            {
                // ����� ��������� �� ������ ��� ����� ������������ ������
                MessageBox.Show("������� ���������� �����!");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Element_Click(sender);
            this.Close();
        }

        // ���������� ������� �������� �����
        private void Form2_Load(object sender, EventArgs e)
        {
            // �������� ������� ����������� ������
            if (Storage.IsDataSaved)
            {
                try
                {
                    // ������� �������� ������ �� �����
                    Storage.LoadData("data.dat");

                    // ���������� ��������� ����� ������� �� ���������
                    textBox2.Text = Storage.Data.d.ToString();
                    textBox3.Text = Storage.Data.maxAverageLetency.ToString();
                    textBox1.Text = Storage.Data.nodesNumber.ToString();
                    textBox4.Text = Storage.Data.T.ToString();
                    textBox6.Text = Storage.Data.maxLatency.ToString();
                    comboBox1.SelectedIndex = Storage.Data.comboBoxSelectedIndex;

                    // �������� ������ � ����� ����� �������� ������
                    makeButtonsAfterSaved();

                    // �������� �����-����� ����� �������� ������
                    File.Delete("saved.txt");

                    // ����� ��������� �� �������� �������� ������
                    MessageBox.Show("������ ���������.");
                }
                catch
                {
                    // ��������� ���������� ��� ������ �������� ������
                }
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            // ��������� ������ ������ �����
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "��������� ����� (*.txt)|*.txt";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = openFileDialog.FileName;
                    try
                    {
                        using (StreamReader reader = new StreamReader(fileName))
                        {
                            // ��������� ���������� �����
                            int nodeCount = int.Parse(reader.ReadLine());

                            // ���������, ��� ���������� ����� ��������� � Storage.Data.nodesNumber
                            if (nodeCount != Storage.Data.nodesNumber)
                            {
                                throw new Exception("���������� ����� � ����� �� ��������� � ��������� ���������.");
                            }

                            // ������� �������
                            Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
                            Storage.Data.adjacencyMatrixNoTopology = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

                            // ��������� �������� � ��������� �������
                            for (int i = 0; i < Storage.Data.nodesNumber; i++)
                            {
                                string[] values = reader.ReadLine().Split(' ');
                                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                                {
                                    int value = int.Parse(values[j]);
                                    // ���������, ��� �� ������� ��������� ����� 0
                                    if (i == j && value != 0)
                                    {
                                        throw new Exception("�������� �� ������� ��������� �� ����� 0.");
                                    }
                                    Storage.Data.adjacencyMatrix[i, j] = value;
                                    Storage.Data.adjacencyMatrixNoTopology[i, j] = value;
                                }
                            }
                        }

                        Element_Click(sender);
                        // ��� ������ �������
                        MessageBox.Show("������� �������� ������� ��������� �� �����.");

                        MatrixBandWidth();
                    }
                    catch (Exception ex)
                    {
                        // ��������� ������
                        MessageBox.Show("��������� ������: " + ex.Message);
                    }
                }
            }
        }



        // ����� ��� ������������� ������� ���������� ������������
        private void MatrixBandWidth()
        {
            // ������������� ������� ���������� ������������
            Storage.Data.matrixBandWidth = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            // ���������� ������� ������
            for (int i = 0; i < Storage.Data.adjacencyMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < Storage.Data.adjacencyMatrix.GetLength(1); j++)
                {
                    Storage.Data.matrixBandWidth[i, j] = 0;
                }
            }

            // ���������� ������� ���������� ������������
            for (int k = 1; k < Storage.Data.bandWidth.Count; k++)
            {
                for (int i = 0; i < Storage.Data.adjacencyMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < Storage.Data.adjacencyMatrix.GetLength(1); j++)
                    {
                        // ��������, ����� �� ��������� ������������ ��������
                        if (i != j)
                        {
                            // ��������� ���������� ����������� � �������
                            if (Storage.Data.adjacencyMatrix[i, j] > Storage.Data.bandWidth[k] &&
                                Storage.Data.adjacencyMatrix[i, j] < Storage.Data.bandWidth[k - 1] &&
                                Storage.Data.matrixBandWidth[i, j] == 0)
                            {
                                Storage.Data.matrixBandWidth[i, j] = Storage.Data.bandWidth[k];
                            }
                            else if (Storage.Data.adjacencyMatrix[i, j] < Storage.Data.bandWidth[k - 1] &&
                                     Storage.Data.matrixBandWidth[i, j] == 0)
                            {
                                Storage.Data.matrixBandWidth[i, j] = Storage.Data.bandWidth[k - 1];
                            }
                        }
                        else
                        {
                            // ��������� ������������ ���������
                            Storage.Data.matrixBandWidth[i, j] = 0;
                        }
                    }
                }
            }
        }


        private void makeButtonsAfterSaved()
        {
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                // �������� ������ � ��������� �����������
                Button nodeButton = new Button
                {
                    BackColor = Color.Black, // ���� ������
                    Size = new Size(30, 30), // ������ ������
                    Location = new Point((int)Storage.Data.nodesCoords[i, 1], (int)Storage.Data.nodesCoords[i, 2]), // ������� ������ �� ������
                };


                nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                // ���������� ������������ ������� ��� ����������� ������
                nodeButton.MouseDown += NodeButton_MouseDown;
                nodeButton.MouseMove += NodeButton_MouseMove;
                nodeButton.MouseUp += NodeButton_MouseUp;

                // ���������� ������ �� ������
                panel2.Controls.Add(nodeButton);
            }

            UpdateConnections();
        }

        private void makeButtons(bool t = false)
        {


            connections.Clear();
            // ��������� panel2
            panel2.Controls.Clear();

            panel2.Invalidate();


            // ��������� ������� ������
            int panelWidth = panel2.Width;
            int panelHeight = panel2.Height;

            // ���������� ������ ������
            centerX = panelWidth / 2;
            centerY = panelHeight / 2;

            // �������� ������������� ��������������� ��� X � Y
            double scaleX = (double)panelWidth / Size.Width;
            double scaleY = (double)panelHeight / Size.Height;


            // ��������� ���������� ����� � �������� � �������
            int numRows = Storage.Data.nodesCoords.GetLength(0);

            // ��������� ��������� ��������� �� comboBox1
            Storage.Data.selectedTopology = comboBox1.SelectedItem.ToString();
            if (Storage.Data.selectedTopology == "���")
            {
                DialogResult result = DialogResult.No;


                if (checkedFullGraph)
                {
                    // ������� ���������� ���� � ��������
                    result = MessageBox.Show("������ ������� ������������ ����?", "������", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        checkedConnections = true;
                    }
                    else
                    {
                        checkedConnections = false;
                    }
                }




                // ������� ����� �� ������� Storage.Data.nodesCoords � �������� ������ ��� ������� ����
                for (int i = 0; i < numRows; i++)
                {
                    int nodeNumber = (int)Storage.Data.nodesCoords[i, 0];
                    int nodeX = (int)(Storage.Data.nodesCoords[i, 1]);
                    int nodeY = (int)(Storage.Data.nodesCoords[i, 2]);

                    // �������� ������ ������ �� ������� ������ �� ���� X � Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // ���� ������ ������� �� ��� �������, ������� � ��������� ����
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                        Storage.Data.nodesCoords[i, 1] = nodeX;
                        Storage.Data.nodesCoords[i, 2] = nodeY;
                    }

                    // �������� ������ � ��������� �����������
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // ���� ������
                        Size = new Size(30, 30), // ������ ������
                        Location = new Point(nodeX, nodeY), // ������� ������ �� ������
                    };


                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                    // �������� ����������� ������� ��� ����������� ������
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // �������� ������ �� ������
                    panel2.Controls.Add(nodeButton);

                    if (result == DialogResult.Yes || checkedConnections)
                    {
                        foreach (Control otherNodeControl in panel2.Controls)
                        {
                            if (otherNodeControl is Button otherNodeButton && otherNodeButton != nodeButton)
                            {
                                // �������� ��������� Connection � �������� ��� � ������ connections
                                Connection connection = new Connection
                                {
                                    StartNode = nodeButton,
                                    EndNode = otherNodeButton
                                };
                                connections.Add(connection);
                            }
                        }
                    }

                    checkedFullGraph = false;

                }
            }

            if (Storage.Data.selectedTopology == "������")
            {
                int numNodes = numRows - 1;
                // �������� ������ connections
                connections.Clear();

                // ������ "������" (���������� �� ������������ ���� �� ���������)
                int starRadius = 100; // ���������� �������� ������



                // ����������� ���������� ������������ ����
                int centerXlocal = panelWidth / 2 - 15;
                int centerYlocal = panelHeight / 2 - 15;

                // ���� ����� ������� "������"
                double angleStep = 2 * Math.PI / numNodes;

                // ��������� ����������� ����
                Button centerNodeButton = new Button
                {
                    BackColor = Color.Black, // ���� ������
                    Size = new Size(30, 30), // ������ ������
                    Location = new Point(centerXlocal, centerYlocal),// ������� ������ �� ������
                    Tag = Storage.Data.nodesCoords[numNodes, 0].ToString()
                };

                int centerNodeId = numRows - 1;
                // ���������� ���������� ������������ ���� � Storage.Data.nodesCoords
                Storage.Data.nodesCoords[centerNodeId, 1] = centerXlocal + centerNodeButton.Width / 2;
                Storage.Data.nodesCoords[centerNodeId, 2] = centerYlocal + centerNodeButton.Height / 2;

                centerNodeButton.Tag = Storage.Data.nodesCoords[numNodes, 0].ToString();

                // �������� ����������� ������� ��� ����������� ������������ ����
                centerNodeButton.MouseDown += NodeButton_MouseDown;
                centerNodeButton.MouseMove += NodeButton_MouseMove;
                centerNodeButton.MouseUp += NodeButton_MouseUp;

                panel2.Controls.Add(centerNodeButton);



                for (int i = 0; i < numNodes; i++)
                {
                    // ��������� ���������� ���� ������ ������������ ����
                    int nodeX = centerXlocal + (int)(starRadius * Math.Cos(i * angleStep) - 15);
                    int nodeY = centerYlocal + (int)(starRadius * Math.Sin(i * angleStep) - 15);

                    // �������� ������ ������ �� ������� ������ �� ���� X � Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // ���� ������ ������� �� ��� �������, ������� � ��������� ����
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                    }

                    // �������� ������ � ��������� �����������
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // ���� ������
                        Size = new Size(30, 30), // ������ ������
                        Location = new Point(nodeX, nodeY), // ������� ������ �� ������
                    };

                    // ���������� ����� ����
                    int nodeNumber = i;

                    // ���������� ���������� ���� � Storage.Data.nodesCoords
                    Storage.Data.nodesCoords[nodeNumber, 1] = nodeX + nodeButton.Width / 2;
                    Storage.Data.nodesCoords[nodeNumber, 2] = nodeY + nodeButton.Height / 2;


                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();
                    // �������� ����������� ������� ��� ����������� ������
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // �������� ������ �� ������
                    panel2.Controls.Add(nodeButton);

                    // �������� ��������� Connection � �������� ��� � ������ connections
                    Connection connection = new Connection
                    {
                        StartNode = centerNodeButton,
                        EndNode = nodeButton
                    };
                    connections.Add(connection);
                }
            }


            else if (Storage.Data.selectedTopology == "������")
            {
                // ���������� ����� � ��������� "������"
                int numNodes = numRows;

                // ������ ������
                int ringRadius = 100; // ���������� �������� ������

                // ���� ����� ������ ������
                double angleStep = 2 * Math.PI / numNodes;

                List<Button> ringNodes = new List<Button>(); // ������ ����� ������

                for (int i = 0; i < numNodes; i++)
                {
                    // ��������� ���������� ���� �� ���������� ������
                    int nodeX = centerX + (int)(ringRadius * Math.Cos(i * angleStep) - 15);
                    int nodeY = centerY + (int)(ringRadius * Math.Sin(i * angleStep) - 15);

                    // �������� ������ ������ �� ������� ������ �� ���� X � Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // ���� ������ ������� �� ��� �������, ������� � ��������� ����
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                    }

                    // �������� ������ � ��������� �����������
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // ���� ������
                        Size = new Size(30, 30), // ������ ������
                        Location = new Point(nodeX, nodeY), // ������� ������ �� ������

                    };

                    // �������� ���������� � Storage.Data.nodesCoords
                    int nodeNumber = i;
                    Storage.Data.nodesCoords[nodeNumber, 1] = nodeX + nodeButton.Width / 2;
                    Storage.Data.nodesCoords[nodeNumber, 2] = nodeY + nodeButton.Height / 2;

                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                    // �������� ����������� ������� ��� ����������� ������
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // �������� ������ �� ������
                    panel2.Controls.Add(nodeButton);

                    // �������� ���� � ������ ����� ������
                    ringNodes.Add(nodeButton);

                    // �������� ���������� ����� ������� ����� � ���������� ����� (���� ����)
                    if (ringNodes.Count > 1)
                    {
                        Connection connection = new Connection
                        {
                            StartNode = ringNodes[ringNodes.Count - 2], // ���������� ����
                            EndNode = nodeButton // ������� ����
                        };
                        connections.Add(connection);


                    }


                }

                // �������� ���������� ����� ������ � ��������� ����� ��� �������� ������
                if (ringNodes.Count > 2)
                {
                    Connection connection = new Connection
                    {
                        StartNode = ringNodes[ringNodes.Count - 1], // ��������� ����
                        EndNode = ringNodes[0] // ������ ����
                    };
                    connections.Add(connection);


                }
            }

            else if (Storage.Data.selectedTopology == "����")
            {
                // ���������� ����� � ��������� "����"
                int numNodes = numRows;

                // ���������� ����� ������ �� ����
                int spacing = 30; // ���������� �������� ����������

                // ��������� ������� X ��� ������� ����
                int startX = centerX - (numNodes - 1) * spacing / 2;

                List<Button> busNodes = new List<Button>(); // ������ ����� ����

                for (int i = 0; i < numNodes; i++)
                {
                    // ��������� ���������� ���� �� ����
                    int nodeX = startX + i * spacing;
                    int nodeY = centerY - 15; // ������������� �������� Y (�� ������)

                    // �������� ������ ������ �� ������� ������ �� ���� X � Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // ���� ������ ������� �� ��� �������, ������� � ��������� ����
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                    }

                    // �������� ������ � ��������� �����������
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // ���� ������
                        Size = new Size(30, 30), // ������ ������
                        Location = new Point(nodeX, nodeY), // ������� ������ �� ������

                    };

                    int nodeNumber = i;
                    Storage.Data.nodesCoords[nodeNumber, 1] = nodeX + nodeButton.Width / 2;
                    Storage.Data.nodesCoords[nodeNumber, 2] = nodeY + nodeButton.Height / 2;

                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                    // �������� ����������� ������� ��� ����������� ������
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // �������� ������ �� ������
                    panel2.Controls.Add(nodeButton);

                    // �������� ���� � ������ ����� ����
                    busNodes.Add(nodeButton);

                    // �������� ���������� ����� ������ (����� ������� ����, ��� ��� �� ������ � ����)
                    if (i > 0)
                    {
                        Connection connection = new Connection
                        {
                            StartNode = busNodes[i - 1],
                            EndNode = nodeButton
                        };
                        connections.Add(connection);
                    }

                }
            }


            else if (Storage.Data.selectedTopology == "������������")
            {
                // ���������� ����� � ��������� "������������"
                int numNodes = numRows;


                // ���������� ����� ������ (�� ����������� � ���������)
                int nodeSpacing = 10; // ���������� �������� ����������

                // ��������� ���������� ����� � ����� ���� � ����� �������, ����� ����������� �������
                int nodesPerRow = (int)Math.Ceiling(Math.Sqrt(numNodes));
                int nodesPerColumn = (int)Math.Ceiling((double)numNodes / nodesPerRow);

                for (int i = 0; i < numNodes; i++)
                {
                    // ��������� ������� X � Y ��� �������� ����
                    int row = i / nodesPerRow;
                    int col = i % nodesPerRow;

                    int nodeX = centerX + col * (30 + nodeSpacing) - ((nodesPerRow - 1) * (30 + nodeSpacing)) / 2;
                    int nodeY = centerY + row * (30 + nodeSpacing) - ((nodesPerColumn - 1) * (30 + nodeSpacing)) / 2;

                    // �������� ������ ���� �� ������� ������ �� ���� X � Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // ���� ���� ������� �� ��� �������, ������� � ��������� ����
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                    }

                    // �������� ���� � ���� �������������� � ��������� �����������
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // ���� ������
                        Size = new Size(30, 30), // ������ ������
                        Location = new Point(nodeX, nodeY), // ������� ������ �� ������

                    };

                    int nodeNumber = i;
                    Storage.Data.nodesCoords[nodeNumber, 1] = nodeX + nodeButton.Width / 2;
                    Storage.Data.nodesCoords[nodeNumber, 2] = nodeY + nodeButton.Height / 2;

                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                    // �������� ����������� ������� ��� ����������� ����
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // �������� ���� �� ������
                    panel2.Controls.Add(nodeButton);
                }

                // �������� ���������� ����� ����� ������ �����
                for (int i = 0; i < numNodes; i++)
                {
                    for (int j = i + 1; j < numNodes; j++)
                    {
                        Connection connection = new Connection
                        {
                            StartNode = panel2.Controls[i] as Button,
                            EndNode = panel2.Controls[j] as Button
                        };
                        connections.Add(connection);
                    }
                }
            }

            if (t == true)
            {
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (Storage.Data.nodesConnections[i, j] == 1)
                        {
                            Connection connection = new Connection
                            {
                                StartNode = panel2.Controls[i] as Button,
                                EndNode = panel2.Controls[j] as Button
                            };
                            connections.Add(connection);
                        }
                    }
                }
            }
            else
            {
                // �������� ������� ���������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        Storage.Data.nodesConnections[i, j] = 0;
                    }
                }


                // ��������� ������� ��������� �� ������ ������ connections
                foreach (Connection connection in connections)
                {
                    int startNodeId = int.Parse(connection.StartNode.Tag.ToString());
                    int endNodeId = int.Parse(connection.EndNode.Tag.ToString());

                    // ���������� �������� 1 � ������� ��������� ��� ����������� �����
                    Storage.Data.nodesConnections[startNodeId, endNodeId] = 1;
                    Storage.Data.nodesConnections[endNodeId, startNodeId] = 1; // ���� ���� ���������������, �� ��� ����� ������ ��� ������ �����������
                }
            }



            // ��������� ����� ����������
            FillLengthConnections();

            panel2.Invalidate();
        }

        private void FillLengthConnections()
        {
            // ������������� ������� ���� ����������
            Storage.Data.lengthConnections = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];


            foreach (Connection connection in connections)
            {
                int startNodeId = int.Parse(connection.StartNode.Tag.ToString());
                int endNodeId = int.Parse(connection.EndNode.Tag.ToString());

                // ��������� ���������� ���������� � ��������� ���� ����� �������� Location ������
                Point startLocation = connection.StartNode.Location;
                Point endLocation = connection.EndNode.Location;

                // ���������� ���������� ����� ������ �� ������� ���������� ����� ����� �������
                double length = Math.Sqrt(Math.Pow(endLocation.X - startLocation.X, 2) + Math.Pow(endLocation.Y - startLocation.Y, 2));

                // ���������� ���������������� �������� ������� ���� ����������
                Storage.Data.lengthConnections[startNodeId, endNodeId] = length;
                Storage.Data.lengthConnections[endNodeId, startNodeId] = length; // ���� ���� ���������������, �� ��� ����� ������ ��� ������ �����������
            }
        }


        // ���������� ������� ��� ������� ������ "�������������"
        private void button1_Click_1(object sender, EventArgs e)
        {
            // �������� ��������� ���������
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("�������� ���������!");
                return;
            }

            // ������������� ������ ��� ������� �����
            Storage.Data.nodesNumber = Storage.Data.nodesNumberNoTopology;
            Storage.Data.nodesCoords = new double[Storage.Data.nodesNumber, 3];
            Storage.Data.nodesConnections = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
            Storage.Data.lengthConnections = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
            Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            // �����������
            Element_Click(sender);

            // ��������� ����� ������� �����
            checkedFullGraph = true;

            // ����������� ��������� ����� 
            for (int i = 0; i < Storage.Data.nodesNumberNoTopology; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Storage.Data.nodesCoords[i, j] = Storage.Data.nodesCoordsNoTopology[i, j];
                }
            }

            // ����������� ������� ��������� 
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    Storage.Data.adjacencyMatrix[i, j] = Storage.Data.adjacencyMatrixNoTopology[i, j];
                }
            }

            // �������� ��������� �����
            makeButtons();
        }

        // �������� ������ ����
        private Button activeNodeButton = null;

        // ���� ��������������
        private bool isDragging = false;

        // ������� ������
        private Button buttonPressed = null;

        // ������ ���� ��� ����������
        private Button firstNodeToConnect = null;

        // ������ ���� ��� ����������
        private Button secondNodeToConnect = null;

        // ���� ������� ������� Ctrl
        private bool isCtrlPressed = false;

        // ���� ������� ������� Shift
        private bool isShiftPressed = false;

        // ���� ������� ������� Alt
        private bool isAltPressed = false;

        // ��������� ��������� ����
        private Point initialNodeLocation;


        // ���������� ������� ������� ������ ���� �� ����
        private void NodeButton_MouseDown(object sender, MouseEventArgs e)
        {
            // ��������, ��� ���� ������ ����� ������ ����
            if (e.Button == MouseButtons.Left)
            {
                // ��������� ������, ����� ������ ������� Ctrl
                if (isCtrlPressed)
                {
                    if (firstNodeToConnect == null)
                    {
                        firstNodeToConnect = sender as Button;
                    }
                    else if (secondNodeToConnect == null)
                    {
                        secondNodeToConnect = sender as Button;
                        // �������� ���������� ����� ���������� ������
                        CreateConnection();
                    }
                }
                // ��������� ������, ����� ������ ������� Shift
                else if (isShiftPressed)
                {
                    // ��������, ��� ����� ����� �� ������ ������ ����
                    if (Storage.Data.nodesNumber == 2)
                    {
                        MessageBox.Show("�� �� ������ �������� ������ ���� �����!");
                        return;
                    }

                    // ��������, ��� ������� ��������� "���"
                    if (comboBox1.SelectedItem.ToString() != "���")
                    {
                        MessageBox.Show("�� �� ������ ������� ���� �� ����������� ���������!");
                        return;
                    }

                    // ���� ������� Shift � ��� �������� ������, ���������� �������� ������ ��� �������� ����������
                    if (buttonPressed == null)
                    {
                        buttonPressed = sender as Button;
                        // �������� ����������
                        DeleteConnection();
                    }
                }
                // ��������� ������, ����� �� ������ ������� Ctrl ��� Shift
                else
                {
                    // ��������� �������� ������
                    activeNodeButton = sender as Button;

                    // ���������� ��������� ��������� ���� ����� ��� ������������
                    initialNodeLocation = activeNodeButton.Location;
                    isDragging = true;
                }
            }
        }





        // ���������� ������� ����������� ����
        private void NodeButton_MouseMove(object sender, MouseEventArgs e)
        {
            // ��������, ��� ���� ����������� � �������� ������ ���� �����������
            if (isDragging && activeNodeButton != null)
            {
                // ��������� ������ ��������� �������� ������
                Point newLocation = panel2.PointToClient(MousePosition);
                activeNodeButton.Location = new Point(newLocation.X - activeNodeButton.Width / 2, newLocation.Y - activeNodeButton.Height / 2);

                // ��������� ���� (������ ����) �������� ������
                int nodeTag = int.Parse(activeNodeButton.Tag.ToString());

                // ���������� ��������� ����� ���� � ������� Storage.Data.nodesCoords
                Storage.Data.nodesCoords[nodeTag, 1] = activeNodeButton.Left + activeNodeButton.Width / 2;
                Storage.Data.nodesCoords[nodeTag, 2] = activeNodeButton.Top + activeNodeButton.Height / 2;

                // ���������� ���� ����������
                FillLengthConnections();

                // ����� ������ ��� ��������� ��������� ��������
                Element_ChangeCoords(Storage.Data.nodesCoords[nodeTag, 1], Storage.Data.nodesCoords[nodeTag, 2], sender);

                // ���������� ������
                panel2.Invalidate();
            }
        }

        private void NodeButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                activeNodeButton = null;
                isDragging = false;
            }
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            int offset = 5; // �������� ������ ������
            float lineWidth = 3.0f; // ������������� ������� �����

            foreach (var connection in connections)
            {
                Button startNode = connection.StartNode;
                Button endNode = connection.EndNode;

                // �������� ����������� ����� ������� ����
                Point startPoint = new Point(startNode.Left + startNode.Width / 2, startNode.Top + startNode.Height / 2);
                Point endPoint = new Point(endNode.Left + endNode.Width / 2, endNode.Top + endNode.Height / 2);

                // �������� ��������� � �������� ����� �� offset �������� ������ ������
                int deltaX = endPoint.X - startPoint.X;
                int deltaY = endPoint.Y - startPoint.Y;
                double length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                double ratio = (length - offset) / length;

                int lineStartX = startPoint.X + (int)(deltaX * ratio);
                int lineStartY = startPoint.Y + (int)(deltaY * ratio);

                int lineEndX = endPoint.X - (int)(deltaX * ratio);
                int lineEndY = endPoint.Y - (int)(deltaY * ratio);

                // ���������� ���� ����� �� ������ ������� IsSaturated � IsUnsaturated
                Color lineColor;
                if (connection.IsSaturated)
                {
                    lineColor = Color.FromArgb(255, 0, 0); // �������
                }
                else if (connection.IsUnsaturated)
                {
                    lineColor = Color.FromArgb(0, 255, 0); // �������
                }
                else
                {
                    lineColor = Color.FromArgb(0, 0, 0); // �������
                }

                // ��������� ����� ����� ������ � ������������ ������ � ������������� ��������
                using (Pen pen = new Pen(lineColor, lineWidth))
                {
                    e.Graphics.DrawLine(pen, lineStartX, lineStartY, lineEndX, lineEndY);
                }
            }
        }



        private void CreateConnection()
        {
            if (firstNodeToConnect != null && secondNodeToConnect != null)
            {
                // ���������� ������� ����� � ������� ��������� �� �� Tag
                int startIndex = int.Parse(firstNodeToConnect.Tag.ToString());
                int endIndex = int.Parse(secondNodeToConnect.Tag.ToString());

                // �������� ���������� � �������� ��� � ������ connections
                Connection connection = new Connection
                {
                    StartNode = firstNodeToConnect,
                    EndNode = secondNodeToConnect
                };
                connections.Add(connection);


                // �������� ������� ���������
                Storage.Data.nodesConnections[startIndex, endIndex] = 1;
                Storage.Data.nodesConnections[endIndex, startIndex] = 1; // ���� ���� ���������������, �� ��� ����� ������ ��� ������ �����������

                // �������� ��������� ����
                firstNodeToConnect = null;
                secondNodeToConnect = null;

                FillLengthConnections();
                // ����������� ������ ��� ����������� ����������
                panel2.Invalidate();
            }
        }

        private void DeleteConnection()
        {
            if (buttonPressed != null)
            {
                int buttonIndex = int.Parse(buttonPressed.Tag.ToString());

                // ������� ����� � ��������� ����� � ������� nodesConnections
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    if (Storage.Data.nodesConnections[buttonIndex, i] == 1)
                    {
                        // ���������� ���������� � 0, ����� ������� ���
                        Storage.Data.nodesConnections[buttonIndex, i] = 0;

                        // ������� ���������� �� ������ connections, ���� ��� ��� ��������
                        Connection connectionToRemove = connections.FirstOrDefault(conn => (conn.StartNode == buttonPressed && conn.EndNode == panel2.Controls[i] as Button) || (conn.EndNode == buttonPressed && conn.StartNode == panel2.Controls[i] as Button));
                        if (connectionToRemove != null)
                        {
                            connections.Remove(connectionToRemove);
                        }
                    }
                }

                // ������� ������ �� ����������
                panel2.Controls.Remove(buttonPressed);



                if (buttonIndex != Storage.Data.nodesConnections.GetLength(0) - 1)
                {
                    foreach (Button nodeButton in panel2.Controls.OfType<Button>())
                    {

                        if (int.Parse(nodeButton.Tag.ToString()) > buttonIndex)
                        {
                            nodeButton.Tag = int.Parse(nodeButton.Tag.ToString()) - 1;
                        }
                        else
                        {
                            nodeButton.Tag = int.Parse(nodeButton.Tag.ToString());
                        }
                    }
                }


                // �������� ���������� buttonPressed
                buttonPressed = null;



                // ������� ��������� ���� �� ���� ��������� ������ � ��������
                UpdateMatricesAndArraysAfterNodeDeletion(buttonIndex);

                // ����������� ������ ��� ����������� ���������
                panel2.Invalidate();
            }
        }


        // ����� ��� ���������� ������ � �������� ����� �������� ����
        private void UpdateMatricesAndArraysAfterNodeDeletion(int deletedNodeIndex)
        {
            // ���������� ������� ������ � �������� �� 1
            int newSize = Storage.Data.nodesNumber - 1;

            // ����������� ���������� �����
            double[,] updatedNodesCoords = new double[newSize, 3];

            // ����������� ����� ����������
            double[,] updatedLengthConnections = new double[newSize, newSize];

            // ����������� ���������� ����� ������
            int[,] updatedNodesConnections = new int[newSize, newSize];

            // ����������� ������� ���������
            double[,] updatedAdjacencyMatrix = new double[newSize, newSize];

            int row = 0;
            int col = 0;

            // ���� �� �����
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                // ������� ���������� ����
                if (i == deletedNodeIndex)
                {
                    continue;
                }

                // ���������� ��������� ����
                if (i > deletedNodeIndex)
                {
                    updatedNodesCoords[row, 0] = Storage.Data.nodesCoords[i, 0] - 1;
                }
                else
                {
                    updatedNodesCoords[row, 0] = Storage.Data.nodesCoords[i, 0];
                }

                // ���������� ��������� ���� (�� ����������)
                if (i != deletedNodeIndex)
                {
                    updatedNodesCoords[row, 1] = Storage.Data.nodesCoords[i, 1];
                    updatedNodesCoords[row, 2] = Storage.Data.nodesCoords[i, 2];

                    col = 0;
                    // ���� �� ����� ��� ���������� ����������
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        // ������� ���������� ����
                        if (j != deletedNodeIndex)
                        {
                            updatedNodesConnections[row, col] = Storage.Data.nodesConnections[i, j];
                            updatedLengthConnections[row, col] = Storage.Data.lengthConnections[i, j];
                            updatedAdjacencyMatrix[row, col] = Storage.Data.adjacencyMatrix[i, j];

                            col++;
                        }
                    }

                    row++;
                }
            }

            // ���������� ������ � ���������
            Storage.Data.nodesCoords = updatedNodesCoords;
            Storage.Data.nodesConnections = updatedNodesConnections;
            Storage.Data.lengthConnections = updatedLengthConnections;
            Storage.Data.adjacencyMatrix = updatedAdjacencyMatrix;
            Storage.Data.nodesNumber = newSize;
        }




        private void UpdateConnections()
        {
            connections.Clear();
            // ���������� ������� ��������� � �������� ���������� �� ������ � ��������
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    // �������� �������� � ������� ���������
                    int connectionValue = Storage.Data.nodesConnections[i, j];

                    // ���������, ��� ���� ���������� (�������� 1)
                    if (connectionValue == 1)
                    {
                        // ������� ��������������� ���� �� �� Tag
                        string startNodeId = Storage.Data.nodesCoords[i, 0].ToString();
                        string endNodeId = Storage.Data.nodesCoords[j, 0].ToString();

                        // ������� ������, ��������������� �����
                        Button startNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == startNodeId);
                        Button endNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == endNodeId);

                        if (startNode != null && endNode != null)
                        {
                            // ���������, �� ���������� �� ��� ������ ���������� � ������ connections
                            Connection existingConnection = connections.FirstOrDefault(conn => (conn.StartNode == startNode && conn.EndNode == endNode) || (conn.StartNode == endNode && conn.EndNode == startNode));

                            if (existingConnection == null)
                            {
                                // �������� ���������� � �������� ��� � ������ connections
                                Connection connection = new Connection
                                {
                                    StartNode = startNode,
                                    EndNode = endNode
                                };
                                connections.Add(connection);
                            }
                        }
                    }
                }
            }

            FillLengthConnections();

            // ����������� ������ ��� ����������� ����������
            panel2.Invalidate();
        }






        private void UpdateConnectionsFromDataGridView()
        {
            // ���������� ������� ��������� � �������� ���������� �� ������ � ��������
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    // �������� �������� � ������� ���������
                    int connectionValue = Storage.Data.nodesConnections[i, j];

                    // ���������, ��� ���� ���������� (�������� 1)
                    if (connectionValue == 1)
                    {
                        // ������� ��������������� ���� �� �� Tag
                        string startNodeId = Storage.Data.nodesCoords[i, 0].ToString();
                        string endNodeId = Storage.Data.nodesCoords[j, 0].ToString();

                        // ������� ������, ��������������� �����
                        Button startNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == startNodeId);
                        Button endNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == endNodeId);

                        if (startNode != null && endNode != null)
                        {
                            // ���������, �� ���������� �� ��� ������ ���������� � ������ connections
                            Connection existingConnection = connections.FirstOrDefault(conn => (conn.StartNode == startNode && conn.EndNode == endNode) || (conn.StartNode == endNode && conn.EndNode == startNode));

                            if (existingConnection == null)
                            {
                                // �������� ���������� � �������� ��� � ������ connections
                                Connection connection = new Connection
                                {
                                    StartNode = startNode,
                                    EndNode = endNode
                                };
                                connections.Add(connection);
                            }
                        }
                    }
                }
            }

            FillLengthConnections();

            // ����������� ������ ��� ����������� ����������
            panel2.Invalidate();
        }




        private void UpdateCoordsFromDataGridView()
        {

        }





        private void button5_Click(object sender, EventArgs e)
        {
            Storage.Data.nodesNumber = 7;
            Storage.Data.nodesNumberNoTopology = 7;
            Storage.Data.nodesCoords = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
            Storage.Data.nodesCoordsNoTopology = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
            Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
            Storage.Data.adjacencyMatrixNoTopology = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            Storage.Data.nodesCoords[0, 0] = 0;
            Storage.Data.nodesCoords[0, 1] = 377;
            Storage.Data.nodesCoords[0, 2] = 389;

            Storage.Data.nodesCoords[1, 0] = 1;
            Storage.Data.nodesCoords[1, 1] = 185;
            Storage.Data.nodesCoords[1, 2] = 35;

            Storage.Data.nodesCoords[2, 0] = 2;
            Storage.Data.nodesCoords[2, 1] = 185;
            Storage.Data.nodesCoords[2, 2] = 389;

            Storage.Data.nodesCoords[3, 0] = 3;
            Storage.Data.nodesCoords[3, 1] = 377;
            Storage.Data.nodesCoords[3, 2] = 35;

            Storage.Data.nodesCoords[4, 0] = 4;
            Storage.Data.nodesCoords[4, 1] = 500;
            Storage.Data.nodesCoords[4, 2] = 200;

            Storage.Data.nodesCoords[5, 0] = 5;
            Storage.Data.nodesCoords[5, 1] = 300;
            Storage.Data.nodesCoords[5, 2] = 300;

            Storage.Data.nodesCoords[6, 0] = 6;
            Storage.Data.nodesCoords[6, 1] = 700;
            Storage.Data.nodesCoords[6, 2] = 150;

            Storage.Data.nodesCoordsNoTopology[0, 0] = 0;
            Storage.Data.nodesCoordsNoTopology[0, 1] = 377;
            Storage.Data.nodesCoordsNoTopology[0, 2] = 389;

            Storage.Data.nodesCoordsNoTopology[1, 0] = 1;
            Storage.Data.nodesCoordsNoTopology[1, 1] = 185;
            Storage.Data.nodesCoordsNoTopology[1, 2] = 35;

            Storage.Data.nodesCoordsNoTopology[2, 0] = 2;
            Storage.Data.nodesCoordsNoTopology[2, 1] = 185;
            Storage.Data.nodesCoordsNoTopology[2, 2] = 389;

            Storage.Data.nodesCoordsNoTopology[3, 0] = 3;
            Storage.Data.nodesCoordsNoTopology[3, 1] = 377;
            Storage.Data.nodesCoordsNoTopology[3, 2] = 35;

            Storage.Data.nodesCoordsNoTopology[4, 0] = 4;
            Storage.Data.nodesCoordsNoTopology[4, 1] = 500;
            Storage.Data.nodesCoordsNoTopology[4, 2] = 200;

            Storage.Data.nodesCoordsNoTopology[5, 0] = 5;
            Storage.Data.nodesCoordsNoTopology[5, 1] = 300;
            Storage.Data.nodesCoordsNoTopology[5, 2] = 300;

            Storage.Data.nodesCoordsNoTopology[6, 0] = 6;
            Storage.Data.nodesCoordsNoTopology[6, 1] = 700;
            Storage.Data.nodesCoordsNoTopology[6, 2] = 150;

            Element_Click(sender);
        }



        private void button6_Click(object sender, EventArgs e)
        {
            Storage.Data.nodesNumber = 5;
            Storage.Data.nodesNumberNoTopology = 5;
            Storage.Data.nodesCoords = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
            Storage.Data.nodesCoordsNoTopology = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
            Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
            Storage.Data.adjacencyMatrixNoTopology = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            Storage.Data.nodesCoords[0, 0] = 0;
            Storage.Data.nodesCoords[0, 1] = 155;
            Storage.Data.nodesCoords[0, 2] = 216;

            Storage.Data.nodesCoords[1, 0] = 1;
            Storage.Data.nodesCoords[1, 1] = -155;
            Storage.Data.nodesCoords[1, 2] = -216;

            Storage.Data.nodesCoords[2, 0] = 2;
            Storage.Data.nodesCoords[2, 1] = -155;
            Storage.Data.nodesCoords[2, 2] = 216;

            Storage.Data.nodesCoords[3, 0] = 3;
            Storage.Data.nodesCoords[3, 1] = 155;
            Storage.Data.nodesCoords[3, 2] = -216;

            Storage.Data.nodesCoords[4, 0] = 4;
            Storage.Data.nodesCoords[4, 1] = 456;
            Storage.Data.nodesCoords[4, 2] = 145;

            Storage.Data.nodesCoordsNoTopology[0, 0] = 0;
            Storage.Data.nodesCoordsNoTopology[0, 1] = 155;
            Storage.Data.nodesCoordsNoTopology[0, 2] = 216;

            Storage.Data.nodesCoordsNoTopology[1, 0] = 1;
            Storage.Data.nodesCoordsNoTopology[1, 1] = -155;
            Storage.Data.nodesCoordsNoTopology[1, 2] = -216;

            Storage.Data.nodesCoordsNoTopology[2, 0] = 2;
            Storage.Data.nodesCoordsNoTopology[2, 1] = -155;
            Storage.Data.nodesCoordsNoTopology[2, 2] = 216;

            Storage.Data.nodesCoordsNoTopology[3, 0] = 3;
            Storage.Data.nodesCoordsNoTopology[3, 1] = 155;
            Storage.Data.nodesCoordsNoTopology[3, 2] = -216;

            Storage.Data.nodesCoordsNoTopology[4, 0] = 4;
            Storage.Data.nodesCoordsNoTopology[4, 1] = 456;
            Storage.Data.nodesCoordsNoTopology[4, 2] = 145;

            Element_Click(sender);
        }



        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            // �������� �� ������� ������� Ctrl
            if (e.Control)
            {
                isCtrlPressed = true;
            }
            // �������� �� ������� ������� Shift
            if (e.Shift)
            {
                isShiftPressed = true;
            }

            // �������� �� ������� ������� Alt
            if (e.Alt)
            {
                isAltPressed = true;
            }
        }

        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {
            // �������� �� ���������� ������� Ctrl
            if (!e.Control)
            {
                isCtrlPressed = false;
            }
            // �������� �� ������� ������� Shift
            if (!e.Shift)
            {
                isShiftPressed = false;
            }
            // �������� �� ������� ������� Alt
            if (!e.Shift)
            {
                isAltPressed = false;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

            // ���������, ��� ������� ������� � ������� "������� ���������"
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "������� ���������")
            {
                DataTable adjacencyMatrix = new DataTable();

                // �������� ������� � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    adjacencyMatrix.Columns.Add(i.ToString(), typeof(int));
                }

                // �������� ������ � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    adjacencyMatrix.Rows.Add();
                }

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        adjacencyMatrix.Rows[i][j] = Storage.Data.nodesConnections[i, j];
                    }
                }

                dataGridView1.DataSource = adjacencyMatrix;
            }

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "������� ��������")
            {
                DataTable loadMatrix = new DataTable();

                // �������� ������� � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    loadMatrix.Columns.Add(i.ToString(), typeof(int));
                }

                // �������� ������ � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    loadMatrix.Rows.Add();
                }

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        loadMatrix.Rows[i][j] = Storage.Data.adjacencyMatrix[i, j];
                    }
                }

                dataGridView2.DataSource = loadMatrix;

            }


            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "������� ���� ����������")
            {
                DataTable lengthMatrix = new DataTable();

                // �������� ������� � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    lengthMatrix.Columns.Add(i.ToString(), typeof(int));
                }

                // �������� ������ � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    lengthMatrix.Rows.Add();
                }

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        lengthMatrix.Rows[i][j] = Storage.Data.lengthConnections[i, j];
                    }
                }

                dataGridView3.DataSource = lengthMatrix;

            }

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "������� ��������")
            {
                DataTable middleLatency = new DataTable();

                // �������� ������� � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    middleLatency.Columns.Add(i.ToString(), typeof(double));
                }

                // �������� ������ � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    middleLatency.Rows.Add();
                }

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        middleLatency.Rows[i][j] = Math.Round(Storage.Data.middleLatency[i, j], 5);
                    }
                }

                dataGridView5.DataSource = middleLatency;

            }



            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "����������")
            {
                DataTable coordsMatrix = new DataTable();

                coordsMatrix.Columns.Add("����� ����", typeof(string));
                coordsMatrix.Columns.Add("X", typeof(string));
                coordsMatrix.Columns.Add("Y", typeof(string));

                // �������� ������ � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    coordsMatrix.Rows.Add();
                }

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        coordsMatrix.Rows[i][j] = Storage.Data.nodesCoords[i, j];
                    }
                }

                // ��������� �������� ComboBox1 � ���������� ReadOnly � ����������� �� �������
                if (comboBox1.SelectedItem != null && comboBox1.SelectedItem.ToString() != "���")
                {
                    dataGridView4.ReadOnly = true;
                }
                else
                {
                    dataGridView4.ReadOnly = false;
                }

                dataGridView4.DataSource = coordsMatrix;
            }

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "������������� �������")
            {
                DataTable sectionMatrix = new DataTable();

                // �������� ������� � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    sectionMatrix.Columns.Add(i.ToString(), typeof(double));
                }

                // �������� ������ � ������� ��� ������ �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    sectionMatrix.Rows.Add();
                }

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        sectionMatrix.Rows[i][j] = Storage.Data.sectionMatrix[i, j];
                    }
                }

                dataGridView6.DataSource = sectionMatrix;
            }

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "��������� ���������")
            {

                DataTable devices = new DataTable();

                // �������� ������� � �������
                for (int i = 0; i < 5; i++)
                {
                    devices.Columns.Add(i.ToString(), typeof(string));
                }

                // �������� ������ � �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    devices.Rows.Add();
                }

                // ��������� ������� ������� �� devicesMatrix
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        devices.Rows[i][j] = Storage.Data.devicesMatrix[i, j];
                    }
                }

                dataGridView7.DataSource = devices;

                //// ������
                DataTable bandWidth = new DataTable();

                // �������� ������� � �������
                for(int i = 0; i < 2; i++)
                {
                    bandWidth.Columns.Add();
                }


                Storage.Data.uniqueValuesList = new List<double>();

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (Storage.Data.matrixBandWidth[i, j] != 0)                          
                        {
                            double valueToAdd = Storage.Data.matrixBandWidth[i, j];

                            // ���������, ���������� �� �������� ��� � ������
                            if (!Storage.Data.uniqueValuesList.Contains(valueToAdd))
                            {
                                // ���� �������� ��� ��� � ������, ��������� ���
                                Storage.Data.uniqueValuesList.Add(valueToAdd);
                            }
                        }
                        
                    }
                }

                Storage.Data.uniqueValuesList.Sort();

                for (int i = 0; i < Storage.Data.uniqueValuesList.Count; i++)
                {
                    bandWidth.Rows.Add();
                }


                for (int i = 0; i < Storage.Data.uniqueValuesList.Count; i++)
                {             
                    for(int j = 0; j < 1; j++)
                    bandWidth.Rows[i][j] = Storage.Data.uniqueValuesList[i];                    
                }

                dataGridView9.DataSource = bandWidth;
            }

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "���������� �����������")
            {

                DataTable bandWidth = new DataTable();

                // �������� ������� � �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    bandWidth.Columns.Add(i.ToString(), typeof(string));
                }

                // �������� ������ � �������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    bandWidth.Rows.Add();
                }

                // ��������� ������� ������� �� devicesMatrix
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        bandWidth.Rows[i][j] = Storage.Data.matrixBandWidth[i, j];
                    }
                }

                dataGridView8.DataSource = bandWidth;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return; // �������� ��������� ������� ��� ����������
            }

            // �������� �������� ������
            object cellValue = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            // ���������� ������������� �������� � Int32
            if (int.TryParse(cellValue.ToString(), out int newValue))
            {
                // ���������, ��� ���������� ������ �� ��������� �� ������� ��������� � �������� ����� 0 ��� 1
                if (e.RowIndex == e.ColumnIndex)
                {
                    // ���� ������ �� ������� ���������, �������� ������ �������� 0
                    if (newValue != 0)
                    {
                        // �������� ��������������
                        MessageBox.Show("������ �������� �������� �� ������� ���������. �������� ������ ���� 0.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // �������� ��������� � ������
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                }
                else
                {
                    // ���� ������ �� �� ������� ���������, �������� ������ �������� 0 ��� 1
                    if (newValue != 0 && newValue != 1)
                    {
                        // �������� ��������������
                        MessageBox.Show("�������� ����� ���� ������ 0 ��� 1.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // �������� ��������� � ������
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                    else
                    {
                        // �������� ��������������� �������� � ������� ���������
                        Storage.Data.nodesConnections[e.RowIndex, e.ColumnIndex] = newValue;

                        // ���� �������� ���� �������� ��� ������� ���������
                        if (e.RowIndex != e.ColumnIndex)
                        {
                            // �������� �������� � ������������ ������
                            dataGridView1.Rows[e.ColumnIndex].Cells[e.RowIndex].Value = newValue;
                            // �������� ��������������� �������� � ������� ���������
                            Storage.Data.nodesConnections[e.ColumnIndex, e.RowIndex] = newValue;

                            // ���� ����������� �������� 0, ������� ����������
                            if (newValue == 0)
                            {
                                // ������� ���������� �� ����� � ������� ��� �� ������ connections
                                Button startNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == Storage.Data.nodesCoords[e.RowIndex, 0].ToString());
                                Button endNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == Storage.Data.nodesCoords[e.ColumnIndex, 0].ToString());

                                if (startNode != null && endNode != null)
                                {
                                    Connection connectionToRemove = connections.FirstOrDefault(conn => (conn.StartNode == startNode && conn.EndNode == endNode) || (conn.StartNode == endNode && conn.EndNode == startNode));

                                    if (connectionToRemove != null)
                                    {
                                        connections.Remove(connectionToRemove);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // ���� �� ������� ������������� �������� � Int32, �������� ��������������
                MessageBox.Show("������������ ��������. ����������, ������� ����� �����.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // �������� ��������� � ������
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
            }

            // �������� ����� ���������� ����������
            UpdateConnectionsFromDataGridView();
        }


        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return; // �������� ��������� ������� ��� ����������
            }

            // �������� �������� ������
            object cellValue = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            // ���������� ������������� �������� � Int32
            if (int.TryParse(cellValue.ToString(), out int newValue))
            {
                // ���������, ��� ���������� ������ �� ��������� �� ������� ��������� � �������� ����� 0 ��� 1
                if (e.RowIndex == e.ColumnIndex)
                {
                    // ���� ������ �� ������� ���������, �������� ������ �������� 0
                    if (newValue != 0)
                    {
                        // �������� ��������������
                        MessageBox.Show("������ �������� �������� �� ������� ���������. �������� ������ ���� 0.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // �������� ��������� � ������
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                }
                else
                {
                    // ���� ������ �� �� ������� ���������, �������� ������ �������� 0 ��� 1
                    if (newValue < 0)
                    {
                        // �������� ��������������
                        MessageBox.Show("������� ���������� ��������!", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // �������� ��������� � ������
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                    else
                    {
                        // �������� ��������������� �������� � ������� ���������
                        Storage.Data.adjacencyMatrix[e.RowIndex, e.ColumnIndex] = newValue;

                        // �������� ����� �������� � ��������������� ������ dataGridView1
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = newValue;
                    }
                }
            }
            else
            {
                // ���� �� ������� ������������� �������� � Int32, �������� ��������������
                MessageBox.Show("������������ ��������. ����������, ������� ����� �����.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // �������� ��������� � ������
                dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
            }
            MatrixBandWidth();
        }


        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private const int XColumnIndex = 1; // ������ ������� ��� X ���������� (��� 0 - ��� ������ �������)
        private const int YColumnIndex = 2; // ������ ������� ��� Y ����������
        private void dataGridView4_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            bool t = true;
            // ���������, ��� ��������� ��������� � ������ � ������������ (X ��� Y)
            if (e.ColumnIndex == XColumnIndex || e.ColumnIndex == YColumnIndex)
            {
                // �������� ����� �������� �� ������
                object newValue = dataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;



                // ���������, ��� �������� �� ������ � ����������
                if (newValue != null && double.TryParse(newValue.ToString(), out double newCoordinate))
                {
                    // ��������� ��������������� ���������� � Storage.Data.nodesCoords
                    int nodeNumber = e.RowIndex;
                    if (e.ColumnIndex == XColumnIndex)
                    {
                        Storage.Data.nodesCoords[nodeNumber, 1] = newCoordinate;
                        Element_ChangeCoords_dataGridView(newCoordinate, XColumnIndex, nodeNumber);
                    }
                    else if (e.ColumnIndex == YColumnIndex)
                    {
                        Storage.Data.nodesCoords[nodeNumber, 2] = newCoordinate;
                        Element_ChangeCoords_dataGridView(newCoordinate, YColumnIndex, nodeNumber);
                    }

                    // �������������� ���� �� ������
                    makeButtons(t);
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (Storage.Data.nodesNumber == 0)
            {
                MessageBox.Show("������� ���������� �����-������ ���������!");
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                if (isAltPressed)
                {
                    // ��������� ������ ������ � ��������
                    int newSize = Storage.Data.nodesNumber + 1;
                    // �������� ������� ���������� X � Y ������� � �����, ��� ���� �������
                    int cursorX = e.X;
                    int cursorY = e.Y;

                    // �������� ����� ������ � �������� �� �� ������
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // ���� ������
                        Size = new Size(30, 30), // ������ ������
                        Location = new Point(cursorX, cursorY) // ������� ������ �� ������
                    };

                    // �������� ����������� ������� ��� ����������� ������
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    nodeButton.Tag = Storage.Data.nodesNumber.ToString();

                    panel2.Controls.Add(nodeButton);

                    // �������� ������� � ������� � ������ ����� ������
                    int newIndex = Storage.Data.nodesNumber;




                    // ���������� ������� ������� nodesCoords
                    double[,] newNodesCoords = new double[newSize, 3];
                    for (int i = 0; i < newSize; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (i < Storage.Data.nodesCoords.GetLength(0) && j < 3)
                            {
                                newNodesCoords[i, j] = Storage.Data.nodesCoords[i, j];
                            }
                            else
                            {
                                // ������������� �������� �� ���������, ���� ����� �� ������� ������ �������
                                newNodesCoords[i, j] = 0;
                            }
                        }
                    }
                    Storage.Data.nodesCoords = newNodesCoords;

                    // ���������� ������� ������� nodesConnections
                    int[,] newNodesConnections = new int[newSize, newSize];
                    for (int i = 0; i < newSize; i++)
                    {
                        for (int j = 0; j < newSize; j++)
                        {
                            if (i < Storage.Data.nodesConnections.GetLength(0) && j < Storage.Data.nodesConnections.GetLength(1))
                            {
                                newNodesConnections[i, j] = Storage.Data.nodesConnections[i, j];
                            }
                            else
                            {
                                // ������������� �������� �� ���������, ���� ����� �� ������� ������ �������
                                newNodesConnections[i, j] = 0;
                            }
                        }
                    }
                    Storage.Data.nodesConnections = newNodesConnections;

                    // ���������� ������� ������� lengthConnections
                    double[,] newLengthConnections = new double[newSize, newSize];
                    for (int i = 0; i < newSize; i++)
                    {
                        for (int j = 0; j < newSize; j++)
                        {
                            if (i < Storage.Data.lengthConnections.GetLength(0) && j < Storage.Data.lengthConnections.GetLength(1))
                            {
                                newLengthConnections[i, j] = Storage.Data.lengthConnections[i, j];
                            }
                            else
                            {
                                // ������������� �������� �� ���������, ���� ����� �� ������� ������ �������
                                newLengthConnections[i, j] = 0;
                            }
                        }
                    }
                    Storage.Data.lengthConnections = newLengthConnections;

                    // ���������� ������� ������� adjacencyMatrix
                    double[,] newAdjacencyMatrix = new double[newSize, newSize];
                    for (int i = 0; i < newSize; i++)
                    {
                        for (int j = 0; j < newSize; j++)
                        {
                            if (i < Storage.Data.adjacencyMatrix.GetLength(0) && j < Storage.Data.adjacencyMatrix.GetLength(1))
                            {
                                newAdjacencyMatrix[i, j] = Storage.Data.adjacencyMatrix[i, j];
                            }
                            else
                            {
                                // ������������� �������� �� ���������, ���� ����� �� ������� ������ �������
                                newAdjacencyMatrix[i, j] = 0;
                            }
                        }
                    }
                    Storage.Data.adjacencyMatrix = newAdjacencyMatrix;




                    // �������� ����� ���������� ��� ����� ������
                    Storage.Data.nodesCoords[newIndex, 0] = newIndex; // ����������� ���������� ������������� (��������, ������) ��� ����
                    Storage.Data.nodesCoords[newIndex, 1] = nodeButton.Location.X;
                    Storage.Data.nodesCoords[newIndex, 2] = nodeButton.Location.Y;

                    // �������� ���������� � ����������� �����
                    Storage.Data.nodesNumber = newSize;
                }
            }
        }


        // ����
        private void button7_Click_1(object sender, EventArgs e)
        {
            if (Storage.Data.nodesNumber == 0)
            {
                MessageBox.Show("�� �� �������� �� ������ ����!");
                return;
            }
            if (textBox2.Text == "" || textBox3.Text == "" || textBox5.Text == "")
            {
                MessageBox.Show("������� �� ��� �������� ��� ����������� ������� ����!");
                return;
            }
            double summary = 0;
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    summary += Storage.Data.nodesConnections[i, j];
                }
            }

            if (summary != Storage.Data.nodesNumber * Storage.Data.nodesNumber - Storage.Data.nodesNumber)
            {
                MessageBox.Show("��� ��������� ���� ��������� ������������ ����!");
                return;
            }

            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                double summary1 = 0;
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    summary1 += Storage.Data.adjacencyMatrix[i, j];
                }
                if (summary1 == 0)
                {
                    MessageBox.Show("� ��� ������� ����, � �������(�� ��������) �� ���������� ������");
                    return;
                }
            }

            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                double summary1 = 0;
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    summary1 += Storage.Data.adjacencyMatrix[j, i];
                }
                if (summary1 == 0)
                {
                    MessageBox.Show("� ��� ������� ����, � �������(�� ��������) �� ���������� ������");
                    return;
                }
            }

            Storage.Data.middleLatency = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];


            // ������� �������� �� �������
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    if (i != j)
                    {
                        Storage.Data.middleLatency[i, j] = 1 / (Storage.Data.matrixBandWidth[i, j] - Storage.Data.adjacencyMatrix[i, j]);
                    }
                    else
                    {
                        Storage.Data.middleLatency[i, j] = 0;
                    }
                }
            }

            double maximum = -1;
            //������������ �������� �� �������
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    maximum = Math.Max(maximum, Storage.Data.middleLatency[i, j]);
                }
            }
            Storage.Data.maxLatency = maximum;

            Storage.Data.weightMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            bandWidth();
            ConnectionWeight();
            
            textBox4.Text = Storage.Data.T.ToString();
            textBox6.Text = Storage.Data.maxLatency.ToString();


            while (countD(Storage.Data.MatrixDij) < countD(Storage.Data.adjacencyMatrix))
            {
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        Storage.Data.adjacencyMatrix[i, j] = Storage.Data.MatrixDij[i, j];
                    }
                }

                bandWidth();
                ConnectionWeight();
                countD(Storage.Data.adjacencyMatrix);
            }

            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                sum = 0;
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    sum += Storage.Data.nodesConnections[i, j];
                    if (Storage.Data.adjacencyMatrix[i, j] == 0 || Storage.Data.MatrixDij[i, j] == double.MaxValue)
                    {
                        /*Matrix[i, j] = 0;
                        Matrix[j, i] = 0;*/

                        if (sum > 1)
                        {
                            Storage.Data.nodesConnections[i, j] = 0;
                            Storage.Data.nodesConnections[j, i] = 0;
                        }
                    }
                }
            }
            MatrixBandWidth();
            UpdateConnections();
        }



        public void bandWidth()
        {
            Storage.Data.T = 0.0;
            sumTraffic = 0;

            for (int i = 0; i < Storage.Data.matrixBandWidth.GetLength(0); i++)
            {
                for (int j = 0; j < Storage.Data.matrixBandWidth.GetLength(1); j++)
                {
                    sumTraffic += Storage.Data.adjacencyMatrix[i, j];
                }
            }


            for (int i = 0; i < Storage.Data.matrixBandWidth.GetLength(0); i++)
            {
                for (int j = 0; j < Storage.Data.matrixBandWidth.GetLength(1); j++)
                {
                    if (i != j)
                    {
                        if (Storage.Data.matrixBandWidth[i, j] - Storage.Data.adjacencyMatrix[i, j] != 0 && sumTraffic != 0)
                        {
                            Storage.Data.T += (double)(Storage.Data.adjacencyMatrix[i, j] / (Storage.Data.matrixBandWidth[i, j] - Storage.Data.adjacencyMatrix[i, j])) / sumTraffic;
                        }
                    }

                }
            }


            sum = 0;
            for (int i = 0; i < Storage.Data.lengthConnections.GetLength(0); i++)
            {
                for (int j = 0; j < Storage.Data.lengthConnections.GetLength(1); j++)
                {
                    sum += Math.Sqrt(Storage.Data.d * Storage.Data.adjacencyMatrix[i, j]);
                }
            }



            //������ ���������� ������������
            for (int i = 0; i < Storage.Data.adjacencyMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < Storage.Data.adjacencyMatrix.GetLength(1); j++)
                {
                    Storage.Data.matrixBandWidth[i, j] = Storage.Data.adjacencyMatrix[i, j] + (sum / (sumTraffic * Storage.Data.maxAverageLetency) * Math.Sqrt(Storage.Data.adjacencyMatrix[i, j] / Storage.Data.d));
                }
            }


        }

        public void ConnectionWeight()
        {
            //���������� ����� �����

            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    if (i == j)
                        Storage.Data.weightMatrix[i, j] = 0;
                    else if ((sumTraffic * Storage.Data.maxAverageLetency * Math.Sqrt(Storage.Data.d * Storage.Data.adjacencyMatrix[i, j]) != 0))
                        Storage.Data.weightMatrix[i, j] = Storage.Data.d * (1 + sum / (sumTraffic * Storage.Data.maxAverageLetency * Math.Sqrt(Storage.Data.d * Storage.Data.adjacencyMatrix[i, j])));
                    else
                        Storage.Data.weightMatrix[i, j] = 0;
                }
            }

            Dijkstra();
        }

        // ����� ��� ���������� ��������� ��������
        public void Dijkstra()
        {
            // ������������� ������� �����
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    if (i == j)
                        Storage.Data.weightMatrix[i, j] = double.MaxValue;
                }
            }

            // ������������� �������� ���������� � ���������
            double[] distance = new double[Storage.Data.nodesNumber];
            bool[] visited = new bool[Storage.Data.nodesNumber];
            Storage.Data.MatrixDij = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            // ��������� ��������� �������
            distance[Storage.Data.indexStartNode] = double.MaxValue;
            int startNode_2 = Storage.Data.indexStartNode;

            int k = 0;
            while (k < Storage.Data.nodesNumber)
            {
                if (startNode_2 == (Storage.Data.nodesNumber - 1))
                    startNode_2 = 0;

                // ������������� ���������� � ���������
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    distance[i] = Storage.Data.weightMatrix[startNode_2, i];
                    visited[i] = false;
                    if (distance[i] == 0)
                        distance[i] = double.MaxValue;
                }

                int index = 0, u;
                for (int i = 0; i < Storage.Data.nodesNumber - 1; i++)
                {
                    // ���������� ������� � ����������� �����������, ������� ��� �� ��������
                    double minDistanceNode = double.MaxValue;
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (!visited[j] && distance[j] < minDistanceNode && distance[j] != 0)
                        {
                            minDistanceNode = distance[j];
                            index = j;
                        }
                    }

                    u = index;
                    visited[u] = true;

                    // ���������� ����������
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (!visited[j] && Storage.Data.weightMatrix[u, j] != double.MaxValue && distance[u] != double.MaxValue && (distance[u] + Storage.Data.weightMatrix[u, j] < distance[j]))
                        {
                            distance[j] = distance[u] + Storage.Data.weightMatrix[u, j];
                        }
                    }
                }

                // ���������� ������� Dij
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    Storage.Data.MatrixDij[startNode_2, i] = distance[i];
                }

                startNode_2 += 1;
                k++;
            }

            // ��������� �������� double.MaxValue � ������������ ��������� ������� Dij
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    if (i == j)
                    {
                        Storage.Data.MatrixDij[i, j] = double.MaxValue;
                    }
                }
            }

            // ���������� ������� ����������
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    if (Storage.Data.MatrixDij[i, j] == double.MaxValue || Storage.Data.MatrixDij[i, j] == 0)
                        Storage.Data.nodesConnections[i, j] = 0;
                    else if (Storage.Data.nodesConnections[i, j] == 1 || Storage.Data.nodesConnections[j, i] == 1)
                        Storage.Data.nodesConnections[i, j] = 1;
                }
            }
        }

        // ����� ��� ������� D
        public double countD(double[,] Matrix)
        {
            D = 0;
            // ��������� ���������
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    D += Storage.Data.d * Matrix[i, j] + Math.Pow(sum, 2) / (sumTraffic * Storage.Data.maxAverageLetency);
                }
            }

            return D;
        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(textBox2.Text, out Storage.Data.d);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(textBox3.Text, out Storage.Data.maxAverageLetency);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            save();
        }

        private void save()
        {
            Storage.SaveData("data.dat");
            File.Create("saved.txt").Dispose(); // �������� �����-�����
            MessageBox.Show("������ ���������.");
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Storage.Data.comboBoxSelectedIndex = comboBox1.SelectedIndex;
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage.Text == "������� ��������")
            {
                if (Storage.Data.middleLatency == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("������� ������� ������ ���������� ����.");
                }
            }
            if (e.TabPage.Text == "������� ���������")
            {
                if (Storage.Data.nodesConnections == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("������� ������� ������ �������������.");
                }
            }
            if (e.TabPage.Text == "������� ���� ����������")
            {
                if (Storage.Data.lengthConnections == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("������� ������� ������ �������������.");
                }
            }
            if (e.TabPage.Text == "����������")
            {
                if (Storage.Data.nodesCoords == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("������� ������� ������ �������������.");
                }
            }
            if (e.TabPage.Text == "������������� �������")
            {
                if (Storage.Data.sectionMatrix == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("������� ������� ������ CSM.");
                }
            }
            if (e.TabPage.Text == "��������� ���������")
            {
                if (Storage.Data.devicesMatrix == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("������� ������� ������ '��������� ������'.");
                }
                else if (Storage.Data.matrixBandWidth == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("������� ��������� ������� ��������.");
                }
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = DialogResult.No;
            // ������� ���������� ���� � ��������
            result = MessageBox.Show("������ ��������� ������� ������?", "������", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                save();
            }
            else
            {
                return;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Storage.Data.nodesNumber = 9;
            Storage.Data.nodesNumberNoTopology = 9;
            Storage.Data.nodesCoords = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
            Storage.Data.nodesCoordsNoTopology = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
            Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
            Storage.Data.adjacencyMatrixNoTopology = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            Storage.Data.nodesCoords[0, 0] = 0;
            Storage.Data.nodesCoords[0, 1] = 155;
            Storage.Data.nodesCoords[0, 2] = 216;

            Storage.Data.nodesCoords[1, 0] = 1;
            Storage.Data.nodesCoords[1, 1] = -155;
            Storage.Data.nodesCoords[1, 2] = -216;

            Storage.Data.nodesCoords[2, 0] = 2;
            Storage.Data.nodesCoords[2, 1] = -155;
            Storage.Data.nodesCoords[2, 2] = 216;

            Storage.Data.nodesCoords[3, 0] = 3;
            Storage.Data.nodesCoords[3, 1] = 155;
            Storage.Data.nodesCoords[3, 2] = -216;

            Storage.Data.nodesCoords[4, 0] = 4;
            Storage.Data.nodesCoords[4, 1] = 356;
            Storage.Data.nodesCoords[4, 2] = 436;

            Storage.Data.nodesCoords[5, 0] = 5;
            Storage.Data.nodesCoords[5, 1] = -488;
            Storage.Data.nodesCoords[5, 2] = -288;

            Storage.Data.nodesCoords[6, 0] = 6;
            Storage.Data.nodesCoords[6, 1] = -328;
            Storage.Data.nodesCoords[6, 2] = 228;

            Storage.Data.nodesCoords[7, 0] = 7;
            Storage.Data.nodesCoords[7, 1] = 328;
            Storage.Data.nodesCoords[7, 2] = 228;

            Storage.Data.nodesCoords[8, 0] = 8;
            Storage.Data.nodesCoords[8, 1] = -500;
            Storage.Data.nodesCoords[8, 2] = 100;

            Storage.Data.nodesCoordsNoTopology[0, 0] = 0;
            Storage.Data.nodesCoordsNoTopology[0, 1] = 155;
            Storage.Data.nodesCoordsNoTopology[0, 2] = 216;

            Storage.Data.nodesCoordsNoTopology[1, 0] = 1;
            Storage.Data.nodesCoordsNoTopology[1, 1] = -155;
            Storage.Data.nodesCoordsNoTopology[1, 2] = -216;

            Storage.Data.nodesCoordsNoTopology[2, 0] = 2;
            Storage.Data.nodesCoordsNoTopology[2, 1] = -155;
            Storage.Data.nodesCoordsNoTopology[2, 2] = 216;

            Storage.Data.nodesCoordsNoTopology[3, 0] = 3;
            Storage.Data.nodesCoordsNoTopology[3, 1] = 155;
            Storage.Data.nodesCoordsNoTopology[3, 2] = -216;

            Storage.Data.nodesCoordsNoTopology[4, 0] = 4;
            Storage.Data.nodesCoordsNoTopology[4, 1] = 356;
            Storage.Data.nodesCoordsNoTopology[4, 2] = 436;

            Storage.Data.nodesCoordsNoTopology[5, 0] = 5;
            Storage.Data.nodesCoordsNoTopology[5, 1] = -488;
            Storage.Data.nodesCoordsNoTopology[5, 2] = -288;

            Storage.Data.nodesCoordsNoTopology[6, 0] = 6;
            Storage.Data.nodesCoordsNoTopology[6, 1] = -328;
            Storage.Data.nodesCoordsNoTopology[6, 2] = 228;

            Storage.Data.nodesCoordsNoTopology[7, 0] = 7;
            Storage.Data.nodesCoordsNoTopology[7, 1] = 328;
            Storage.Data.nodesCoordsNoTopology[7, 2] = 228;

            Storage.Data.nodesCoordsNoTopology[8, 0] = 8;
            Storage.Data.nodesCoordsNoTopology[8, 1] = -500;
            Storage.Data.nodesCoordsNoTopology[8, 2] = 100;

            Element_Click(sender);
        }


        private void button10_Click(object sender, EventArgs e)
        {
            Storage.Data.nodesNumber = 10;
            Storage.Data.nodesNumberNoTopology = 10;
            Storage.Data.nodesCoords = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
            Storage.Data.nodesCoordsNoTopology = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
            Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
            Storage.Data.adjacencyMatrixNoTopology = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            Storage.Data.nodesCoords[0, 0] = 0;
            Storage.Data.nodesCoords[0, 1] = 155;
            Storage.Data.nodesCoords[0, 2] = 216;

            Storage.Data.nodesCoords[1, 0] = 1;
            Storage.Data.nodesCoords[1, 1] = -155;
            Storage.Data.nodesCoords[1, 2] = -216;

            Storage.Data.nodesCoords[2, 0] = 2;
            Storage.Data.nodesCoords[2, 1] = -155;
            Storage.Data.nodesCoords[2, 2] = 216;

            Storage.Data.nodesCoords[3, 0] = 3;
            Storage.Data.nodesCoords[3, 1] = 155;
            Storage.Data.nodesCoords[3, 2] = -216;

            Storage.Data.nodesCoords[4, 0] = 4;
            Storage.Data.nodesCoords[4, 1] = 356;
            Storage.Data.nodesCoords[4, 2] = 436;

            Storage.Data.nodesCoords[5, 0] = 5;
            Storage.Data.nodesCoords[5, 1] = -488;
            Storage.Data.nodesCoords[5, 2] = -288;

            Storage.Data.nodesCoords[6, 0] = 6;
            Storage.Data.nodesCoords[6, 1] = -328;
            Storage.Data.nodesCoords[6, 2] = 228;

            Storage.Data.nodesCoords[7, 0] = 7;
            Storage.Data.nodesCoords[7, 1] = 328;
            Storage.Data.nodesCoords[7, 2] = 228;

            Storage.Data.nodesCoords[8, 0] = 8;
            Storage.Data.nodesCoords[8, 1] = -500;
            Storage.Data.nodesCoords[8, 2] = 100;

            Storage.Data.nodesCoords[9, 0] = 9;
            Storage.Data.nodesCoords[9, 1] = 600;
            Storage.Data.nodesCoords[9, 2] = -200;

            Storage.Data.nodesCoordsNoTopology[0, 0] = 0;
            Storage.Data.nodesCoordsNoTopology[0, 1] = 155;
            Storage.Data.nodesCoordsNoTopology[0, 2] = 216;

            Storage.Data.nodesCoordsNoTopology[1, 0] = 1;
            Storage.Data.nodesCoordsNoTopology[1, 1] = -155;
            Storage.Data.nodesCoordsNoTopology[1, 2] = -216;

            Storage.Data.nodesCoordsNoTopology[2, 0] = 2;
            Storage.Data.nodesCoordsNoTopology[2, 1] = -155;
            Storage.Data.nodesCoordsNoTopology[2, 2] = 216;

            Storage.Data.nodesCoordsNoTopology[3, 0] = 3;
            Storage.Data.nodesCoordsNoTopology[3, 1] = 155;
            Storage.Data.nodesCoordsNoTopology[3, 2] = -216;

            Storage.Data.nodesCoordsNoTopology[4, 0] = 4;
            Storage.Data.nodesCoordsNoTopology[4, 1] = 356;
            Storage.Data.nodesCoordsNoTopology[4, 2] = 436;

            Storage.Data.nodesCoordsNoTopology[5, 0] = 5;
            Storage.Data.nodesCoordsNoTopology[5, 1] = -488;
            Storage.Data.nodesCoordsNoTopology[5, 2] = -288;

            Storage.Data.nodesCoordsNoTopology[6, 0] = 6;
            Storage.Data.nodesCoordsNoTopology[6, 1] = -328;
            Storage.Data.nodesCoordsNoTopology[6, 2] = 228;

            Storage.Data.nodesCoordsNoTopology[7, 0] = 7;
            Storage.Data.nodesCoordsNoTopology[7, 1] = 328;
            Storage.Data.nodesCoordsNoTopology[7, 2] = 228;

            Storage.Data.nodesCoordsNoTopology[8, 0] = 8;
            Storage.Data.nodesCoordsNoTopology[8, 1] = -500;
            Storage.Data.nodesCoordsNoTopology[8, 2] = 100;

            Storage.Data.nodesCoordsNoTopology[9, 0] = 9;
            Storage.Data.nodesCoordsNoTopology[9, 1] = 600;
            Storage.Data.nodesCoordsNoTopology[9, 2] = -200;

            Element_Click(sender);
        }


        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            Storage.Data.indexStartNode = int.Parse(textBox5.Text);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try {
                connections.Clear();
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    int count = 0;
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (Storage.Data.nodesConnections[i, j] == 1 || Storage.Data.nodesConnections[j, i] == 1)
                        {
                            count++;
                        }
                    }
                    if (count == 0)
                    {
                        MessageBox.Show("�� ��� ���� ����� ������ �����!");
                        return;
                    }
                }

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (Storage.Data.nodesConnections[i, j] == 0 && Storage.Data.nodesConnections[i, j] == 0)
                        {
                            Storage.Data.matrixBandWidth[i, j] = Storage.Data.matrixBandWidth[j, i] = 0;
                        }
                    }
                }

                // ���������� T

                Storage.Data.T = 0.0;
                sumTraffic = 0;

                if(Storage.Data.matrixBandWidth != null) {
                    for (int i = 0; i < Storage.Data.matrixBandWidth.GetLength(0); i++)
                    {
                        for (int j = 0; j < Storage.Data.matrixBandWidth.GetLength(1); j++)
                        {
                            sumTraffic += Storage.Data.adjacencyMatrix[i, j];
                        }
                    }
                }
                else
                {
                    MessageBox.Show("���������������� �� ��� ��������!");
                    return;
                }

                


                for (int i = 0; i < Storage.Data.matrixBandWidth.GetLength(0); i++)
                {
                    for (int j = 0; j < Storage.Data.matrixBandWidth.GetLength(1); j++)
                    {
                        if (i != j)
                        {
                            if (Storage.Data.matrixBandWidth[i, j] - Storage.Data.adjacencyMatrix[i, j] != 0 && sumTraffic != 0)
                            {
                                Storage.Data.T += (double)(Storage.Data.adjacencyMatrix[i, j] / (Storage.Data.matrixBandWidth[i, j] - Storage.Data.adjacencyMatrix[i, j])) / sumTraffic;
                            }
                        }

                    }
                }




                textBox7.Text = Storage.Data.T.ToString();



                Storage.Data.weightMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (i == j)
                            Storage.Data.weightMatrix[i, j] = 0;
                        else
                            Storage.Data.weightMatrix[i, j] = Storage.Data.matrixBandWidth[i, j];
                    }
                }
                Dijkstra();

                Storage.Data.sectionMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (i != j && Storage.Data.matrixBandWidth[i, j] != 0)
                        {
                            Storage.Data.sectionMatrix[i, j] = Storage.Data.adjacencyMatrix[i, j] / Storage.Data.matrixBandWidth[i, j];
                        }
                        else
                        {
                            Storage.Data.sectionMatrix[i, j] = 0;
                        }
                    }
                }

                Storage.Data.maxSectionMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
                bool[,] processedPairs = new bool[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    double max = -1;
                    int I = 0;
                    int J = 0;

                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        // ��������, ��� ���� �������� ��� �� ����������
                        if (!processedPairs[i, j] && !processedPairs[j, i] && j != i && Storage.Data.sectionMatrix[i, j] > max && Storage.Data.sectionMatrix[i, j] != 0)
                        {
                            max = Storage.Data.sectionMatrix[i, j];
                            I = i;
                            J = j;
                        }
                    }

                    // �������� ���� �������� ��� ������������
                    processedPairs[I, J] = true;
                    processedPairs[J, I] = true;

                    Storage.Data.maxSectionMatrix[I, J] = 1;
                }

                // ��������� ������� �������� ���� �����
                double averageLoad = CalculateAverageLoad();

                // ��������� ��� �������
                double loadConstant = 0.001;

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (Storage.Data.nodesConnections[i, j] == 1)
                        {
                            double load = Storage.Data.sectionMatrix[i, j];

                            // ������ ��������
                            double lowerThreshold = averageLoad;

                            // ���������, �������� �� ���� �����������
                            bool isSaturated = Storage.Data.maxSectionMatrix[i, j] == 1;

                            // ���������, �������� �� ���� �� �����������
                            bool isUnsaturated = load < lowerThreshold;

                            // ������� ��������������� ���� �� �� Tag
                            string startNodeId = Storage.Data.nodesCoords[i, 0].ToString();
                            string endNodeId = Storage.Data.nodesCoords[j, 0].ToString();

                            // ������� ������, ��������������� �����
                            Button startNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == startNodeId);
                            Button endNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == endNodeId);

                            if (startNode != null && endNode != null)
                            {
                                // ��������, ���� �� ��� ������� ���������� � ����� �� �����������
                                bool connectionExists = connections.Any(c =>
                                    (c.StartNode == startNode && c.EndNode == endNode) ||
                                    (c.StartNode == endNode && c.EndNode == startNode));

                                // ���� ���������� ��� �� ������� ��� isSaturated == true, �������� ���
                                if (!connectionExists || isSaturated)
                                {
                                    // ������� ����� ���������� � ������ � ����������� �� ������������
                                    Connection connection = new Connection
                                    {
                                        StartNode = startNode,
                                        EndNode = endNode,
                                        IsSaturated = isSaturated,
                                        IsUnsaturated = isUnsaturated
                                    };
                                    connections.Add(connection);
                                }
                            }
                        }
                    }
                }

                MatrixBandWidth();

                // ������������ ������, ����� ���������� ����� �����
                panel2.Invalidate();
            }
            catch (NullReferenceException nullEx)
            {
                MessageBox.Show("������� �� ��� ��������!");
            }
            catch (System.Exception ex) {
                MessageBox.Show("���������������� �� ��� ��������!");
            }
            
        }

        // ����� ��� ���������� ������� �������� ���� �����
        private double CalculateAverageLoad()
        {
            double totalLoad = 0;
            int sum = 0;
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    if (Storage.Data.nodesConnections[i, j] == 1)
                    {
                        totalLoad += Storage.Data.sectionMatrix[i, j];
                    }
                }
            }

            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    sum += Storage.Data.nodesConnections[i, j];
                }
            }

            return totalLoad / sum;
        }



        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {

        }


        private void button13_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "��������� ����� (*.txt)|*.txt";
            openFileDialog1.Title = "�������� ����";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // ���������, ��� ��� ����� ������������ �� "_dv.txt"
                    if (Path.GetFileNameWithoutExtension(openFileDialog1.FileName).EndsWith("_dv"))
                    {
                        using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                        {
                            // ������ ������ ������ � ���������� � Storage.Data.nodesNumber
                            string firstLine = sr.ReadLine();
                            if (firstLine != null && firstLine.Trim() == Storage.Data.nodesNumber.ToString())
                            {
                                // ������� ������� � ������ 5 �������� � Storage.Data.nodesNumber �����
                                Storage.Data.devicesMatrix = new string[Storage.Data.nodesNumber, 5];

                                // ������ ��������� ������ � ��������� �������
                                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                                {
                                    string line = sr.ReadLine();
                                    string[] parts = line.Split(' ');

                                    if (parts.Length == 5)
                                    {
                                        for (int j = 0; j < 5; j++)
                                        {
                                            Storage.Data.devicesMatrix[i, j] = parts[j];
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("������������ ������ ������ � �����!", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("�������� ������������ ��� ����������� ���������� �����!", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                    }
                    else
                    {
                        MessageBox.Show("�������� ���� � ���������� ��������� (_dv)!", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"������ ��� ������ �����: {ex.Message}", "������", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView8_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
           
            // �������� ����������� ���������� �������
            dataGridView8.CellValueChanged -= dataGridView8_CellValueChanged;

            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    // �������� �������� ������
                    object cellValue = dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                    // �������� ������������� �������� � Int32
                    if (int.TryParse(cellValue?.ToString(), out int newValue))
                    {
                        if (e.RowIndex == e.ColumnIndex)
                        {
                            // ���� ������ �� ������� ���������, ��������� ������ �������� 0
                            if (newValue != 0)
                            {
                                MessageBox.Show("������ �������� �������� �� ������� ���������. �������� ������ ���� 0.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                // ���������� ��������� � ������
                                dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                            }
                        }
                        else
                        {
                            double value = Storage.Data.matrixBandWidth[e.RowIndex, e.ColumnIndex];
                            // ���� ������ �� �� ������� ���������, ��������� ������ �������� 0 ��� 1
                            if (newValue < 0)
                            {
                                MessageBox.Show("������� ���������� ��������!", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                // ���������� ��������� � ������
                                dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                            }
                            if (newValue < Storage.Data.adjacencyMatrix[e.RowIndex, e.ColumnIndex])
                            {
                                MessageBox.Show("��������� �������� ������, ��� ��������������� �������� " +
                                    " � ������� ��������", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                // ���������� ��������� � ������
                                dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value;
                            }
                            else
                            {
                                // ��������� ��������������� �������� � ������� ���������
                                Storage.Data.matrixBandWidth[e.RowIndex, e.ColumnIndex] = newValue;
                                dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = newValue;
                            }
                        }
                    }
                    else
                    {
                        // ���� �� ������� ������������� �������� � Int32, ������� ��������������
                        MessageBox.Show("������������ ��������. ����������, ������� ����� �����.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // ���������� ��������� � ������
                        dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                }
            }
            finally
            {
                // ���������������� ���������� �������
                dataGridView8.CellValueChanged += dataGridView8_CellValueChanged;
            }
        }

        private void dataGridView9_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView9_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // �������� ����������� ���������� �������
                dataGridView9.CellValueChanged -= dataGridView9_CellValueChanged;
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                {
                    return; // �������� ��������� ������� ��� ����������
                }

                // �������� �������� ������
                object cellValue = dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                // ���������� ������������� �������� � Int32
                if (int.TryParse(cellValue.ToString(), out int newValue))
                {
                    if (newValue < 0)
                    {
                        // �������� ��������������
                        MessageBox.Show("����������, ������� ���������� �������� ���������!", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // �������� ��������� � ������
                        dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                    else
                    {
                        // ����������� e.RowIndex ��� ����� � e.ColumnIndex ��� ��������
                        dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = newValue;
                    }
                }
                else
                {
                    // ���� �� ������� ������������� �������� � Int32, �������� ��������������
                    MessageBox.Show("������������ ��������. ����������, ������� ����� �����.", "��������������", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // �������� ��������� � ������
                    dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                }
            }
            catch
            {

            }
            finally
            {
                // ���������������� ���������� �������
                dataGridView9.CellValueChanged += dataGridView9_CellValueChanged;
            }
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            try {
                if (Storage.Data.lengthConnections == null)
                {
                    MessageBox.Show("������� ���������� ���������!");
                }
                // ��������� dataGridView7
                int lastColumnIndex7 = dataGridView7.Columns.Count - 1;
                double sumDataGridView7 = 0;
                foreach (DataGridViewRow row in dataGridView7.Rows)
                {
                    if (row.Cells[lastColumnIndex7].Value != null)
                    {
                        double cellValue;
                        if (double.TryParse(row.Cells[lastColumnIndex7].Value.ToString(), out cellValue))
                        {
                            sumDataGridView7 += cellValue;
                        }
                    }
                }

                // ��������� dataGridView9
                double totalSum = 0;

                // ������������, ��� DataGridView9 ��� ������ � � ���� ���� ��� �������.

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    double bandWidthValue = 0;
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (Storage.Data.matrixBandWidth[i, j] != 0)
                        {
                            bandWidthValue = Storage.Data.matrixBandWidth[i, j];
                        }

                        if (bandWidthValue != 0)
                        {
                            // ���� ��������������� �������� � ������ ������� DataGridView9
                            DataGridViewRow row = dataGridView9.Rows
                                .Cast<DataGridViewRow>()
                                .FirstOrDefault(r => r.Cells[0].Value != null && Convert.ToDouble(r.Cells[0].Value).Equals(bandWidthValue));

                            if (row != null)
                            {
                                // ������� ������ �������� �������
                                int columnIndex = dataGridView9.Columns["Column2"].Index;

                                // �������� �������� �� ������� ������� DataGridView9
                                double coefficient = Convert.ToDouble(row.Cells[columnIndex].Value);

                                // �������� �� ��������������� �������� ������� lengthConnections
                                totalSum += coefficient * Storage.Data.lengthConnections[i, j];

                            }
                        }
                    }
                }



                // ����� ����� ����� DataGridView � label9
                double totalSumBothGrids = sumDataGridView7 + Math.Round(totalSum);
                label9.Text = totalSumBothGrids.ToString();
            }
            catch(Exception ex) {
                MessageBox.Show("������� ������!");
            }
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}