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
        // Количество столбцов для матрицы координат узлов
        private int nodesCoordsColumns = 3;

        // Класс, представляющий соединение между узлами
        public class Connection
        {
            // Начальный узел соединения
            public Button StartNode { get; set; }

            // Конечный узел соединения
            public Button EndNode { get; set; }

            // Флаг, указывающий, насыщено ли соединение
            public bool IsSaturated { get; set; }

            // Флаг, указывающий, не насыщено ли соединение
            public bool IsUnsaturated { get; set; }
        }

        // Координаты центра формы
        public int centerX;

        // Координаты центра формы
        public int centerY;

        // Список соединений между узлами
        private List<Connection> connections = new List<Connection>();

        // Флаг, указывающий, выбран ли полносвязный граф
        bool checkedFullGraph = false;

        // Флаг, указывающий, установлены ли соединения между узлами
        bool checkedConnections = false;

        // Сумма значений матрицы пропускных способностей
        double sum;

        // Сумма трафика
        double sumTraffic;

        // Значение переменной D
        double D;


        public Form2()
        {
            InitializeComponent();
            Storage.Data = new StorageData();
            this.KeyPreview = true;

        }



        private void Element_Click(object sender)
        {
            // Получить текущее время
            DateTime currentTime = DateTime.Now;

            // Определить тип элемента, на который был сделан клик
            string elementType = sender.GetType().Name;

            // Получить текст элемента, если он поддерживает свойство Text
            string elementText = (sender is Control control) ? control.Text : "";

            // Создать строку с информацией о нажатии
            string clickInfo = $"{currentTime}: Тип элемента: {elementType}, Текст элемента: {elementText}";

            // Задать путь к файлу для записи информации
            string filePath = "log.txt";

            // Записать информацию в файл
            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine("Событие: Click");
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
            // Получить текущее время
            DateTime currentTime = DateTime.Now;

            // Определить тип элемента, на который был сделан клик
            string elementType = sender.GetType().Name;

            // Получить текст элемента, если он поддерживает свойство Text
            string elementText = (sender is Control control) ? control.Tag.ToString() : "";

            // Создать строку с информацией о нажатии
            string clickInfo = $"{currentTime}: Вы изменили координаты узла {elementText}. X = {coord1} Y = {coord2}";

            // Задать путь к файлу для записи информации
            string filePath = "log.txt";

            // Записать информацию в файл
            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine("Событие: Изменение координат");
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
            // Получить текущее время
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


            // Создать строку с информацией о нажатии
            string clickInfo = $"Вы изменили координаты узла {index}. Текущая координата {coordinateXY} равна {value}";

            // Задать путь к файлу для записи информации
            string filePath = "log.txt";

            // Записать информацию в файл
            try
            {
                using (StreamWriter writer = File.AppendText(filePath))
                {
                    writer.WriteLine("Событие: Изменение координат");
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
                // Попытка преобразования введенного текста в число и установка числа узлов в хранилище
                Storage.Data.nodesNumber = int.Parse(textBox1.Text);
                Storage.Data.nodesNumberNoTopology = int.Parse(textBox1.Text);

                // Проверка на натуральное число узлов
                if (Storage.Data.nodesNumber <= 0)
                {
                    MessageBox.Show("Количество узлов должно быть натуральным числом!");
                    return;
                }

                // Проверка на количество узлов больше одного
                if (Storage.Data.nodesNumber == 1)
                {
                    MessageBox.Show("Количество узлов должно быть больше одного!");
                    return;
                }

                // Инициализация матриц координат узлов
                Storage.Data.nodesCoords = new double[Storage.Data.nodesNumber, nodesCoordsColumns];
                Storage.Data.nodesCoordsNoTopology = new double[Storage.Data.nodesNumber, nodesCoordsColumns];

                // Открытие формы для ввода координат
                Coords coords = new Coords();
                Element_Click(sender);
                coords.ShowDialog();
            }
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке при вводе некорректных данных
                MessageBox.Show("Введите количество узлов!");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Element_Click(sender);
            this.Close();
        }

        // Обработчик события загрузки формы
        private void Form2_Load(object sender, EventArgs e)
        {
            // Проверка наличия сохраненных данных
            if (Storage.IsDataSaved)
            {
                try
                {
                    // Попытка загрузки данных из файла
                    Storage.LoadData("data.dat");

                    // Заполнение текстовых полей данными из хранилища
                    textBox2.Text = Storage.Data.d.ToString();
                    textBox3.Text = Storage.Data.maxAverageLetency.ToString();
                    textBox1.Text = Storage.Data.nodesNumber.ToString();
                    textBox4.Text = Storage.Data.T.ToString();
                    textBox6.Text = Storage.Data.maxLatency.ToString();
                    comboBox1.SelectedIndex = Storage.Data.comboBoxSelectedIndex;

                    // Создание кнопок и узлов после загрузки данных
                    makeButtonsAfterSaved();

                    // Удаление файла-флага после загрузки данных
                    File.Delete("saved.txt");

                    // Вывод сообщения об успешной загрузке данных
                    MessageBox.Show("Данные загружены.");
                }
                catch
                {
                    // Обработка исключения при ошибке загрузки данных
                }
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            // Открываем диалог выбора файла
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = openFileDialog.FileName;
                    try
                    {
                        using (StreamReader reader = new StreamReader(fileName))
                        {
                            // Считываем количество узлов
                            int nodeCount = int.Parse(reader.ReadLine());

                            // Проверяем, что количество узлов совпадает с Storage.Data.nodesNumber
                            if (nodeCount != Storage.Data.nodesNumber)
                            {
                                throw new Exception("Количество узлов в файле не совпадает с ожидаемым значением.");
                            }

                            // Создаем матрицу
                            Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
                            Storage.Data.adjacencyMatrixNoTopology = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

                            // Считываем значения и заполняем матрицу
                            for (int i = 0; i < Storage.Data.nodesNumber; i++)
                            {
                                string[] values = reader.ReadLine().Split(' ');
                                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                                {
                                    int value = int.Parse(values[j]);
                                    // Проверяем, что на главной диагонали стоят 0
                                    if (i == j && value != 0)
                                    {
                                        throw new Exception("Значение на главной диагонали не равно 0.");
                                    }
                                    Storage.Data.adjacencyMatrix[i, j] = value;
                                    Storage.Data.adjacencyMatrixNoTopology[i, j] = value;
                                }
                            }
                        }

                        Element_Click(sender);
                        // Все прошло успешно
                        MessageBox.Show("Матрица нагрузки успешно загружена из файла.");

                        MatrixBandWidth();
                    }
                    catch (Exception ex)
                    {
                        // Обработка ошибок
                        MessageBox.Show("Произошла ошибка: " + ex.Message);
                    }
                }
            }
        }



        // Метод для инициализации матрицы пропускных способностей
        private void MatrixBandWidth()
        {
            // Инициализация матрицы пропускных способностей
            Storage.Data.matrixBandWidth = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            // Заполнение матрицы нулями
            for (int i = 0; i < Storage.Data.adjacencyMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < Storage.Data.adjacencyMatrix.GetLength(1); j++)
                {
                    Storage.Data.matrixBandWidth[i, j] = 0;
                }
            }

            // Заполнение матрицы пропускных способностей
            for (int k = 1; k < Storage.Data.bandWidth.Count; k++)
            {
                for (int i = 0; i < Storage.Data.adjacencyMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < Storage.Data.adjacencyMatrix.GetLength(1); j++)
                    {
                        // Проверка, чтобы не учитывать диагональные элементы
                        if (i != j)
                        {
                            // Установка пропускной способности в матрицу
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
                            // Обнуление диагональных элементов
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
                // Создание кнопки с заданными параметрами
                Button nodeButton = new Button
                {
                    BackColor = Color.Black, // Цвет кнопки
                    Size = new Size(30, 30), // Размер кнопки
                    Location = new Point((int)Storage.Data.nodesCoords[i, 1], (int)Storage.Data.nodesCoords[i, 2]), // Позиция кнопки на панели
                };


                nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                // Добавление обработчиков событий для перемещения кнопок
                nodeButton.MouseDown += NodeButton_MouseDown;
                nodeButton.MouseMove += NodeButton_MouseMove;
                nodeButton.MouseUp += NodeButton_MouseUp;

                // Добавление кнопки на панель
                panel2.Controls.Add(nodeButton);
            }

            UpdateConnections();
        }

        private void makeButtons(bool t = false)
        {


            connections.Clear();
            // Очиститка panel2
            panel2.Controls.Clear();

            panel2.Invalidate();


            // Получение размеры панели
            int panelWidth = panel2.Width;
            int panelHeight = panel2.Height;

            // Нахождение центра панели
            centerX = panelWidth / 2;
            centerY = panelHeight / 2;

            // Создание коэффициентов масштабирования для X и Y
            double scaleX = (double)panelWidth / Size.Width;
            double scaleY = (double)panelHeight / Size.Height;


            // Получение количества строк и столбцов в матрице
            int numRows = Storage.Data.nodesCoords.GetLength(0);

            // Получение выбранной топологии из comboBox1
            Storage.Data.selectedTopology = comboBox1.SelectedItem.ToString();
            if (Storage.Data.selectedTopology == "Нет")
            {
                DialogResult result = DialogResult.No;


                if (checkedFullGraph)
                {
                    // Вывести диалоговое окно с вопросом
                    result = MessageBox.Show("Хотите создать полносвязный граф?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        checkedConnections = true;
                    }
                    else
                    {
                        checkedConnections = false;
                    }
                }




                // Перебор узлов из матрицы Storage.Data.nodesCoords и создание кнопки для каждого узла
                for (int i = 0; i < numRows; i++)
                {
                    int nodeNumber = (int)Storage.Data.nodesCoords[i, 0];
                    int nodeX = (int)(Storage.Data.nodesCoords[i, 1]);
                    int nodeY = (int)(Storage.Data.nodesCoords[i, 2]);

                    // Проверка выхода кнопки за пределы панели по осям X и Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // Если кнопка выходит за оба предела, вернуть в ближайший угол
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                        Storage.Data.nodesCoords[i, 1] = nodeX;
                        Storage.Data.nodesCoords[i, 2] = nodeY;
                    }

                    // Создайте кнопку с заданными параметрами
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // Цвет кнопки
                        Size = new Size(30, 30), // Размер кнопки
                        Location = new Point(nodeX, nodeY), // Позиция кнопки на панели
                    };


                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                    // Добавьте обработчики событий для перемещения кнопок
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // Добавьте кнопку на панель
                    panel2.Controls.Add(nodeButton);

                    if (result == DialogResult.Yes || checkedConnections)
                    {
                        foreach (Control otherNodeControl in panel2.Controls)
                        {
                            if (otherNodeControl is Button otherNodeButton && otherNodeButton != nodeButton)
                            {
                                // Создайте экземпляр Connection и добавьте его в список connections
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

            if (Storage.Data.selectedTopology == "Звезда")
            {
                int numNodes = numRows - 1;
                // Очистите список connections
                connections.Clear();

                // Радиус "звезды" (расстояние от центрального узла до остальных)
                int starRadius = 100; // Подставьте желаемый радиус



                // Рассчитайте координаты центрального узла
                int centerXlocal = panelWidth / 2 - 15;
                int centerYlocal = panelHeight / 2 - 15;

                // Угол между линиями "звезды"
                double angleStep = 2 * Math.PI / numNodes;

                // Отрисуйте центральный узел
                Button centerNodeButton = new Button
                {
                    BackColor = Color.Black, // Цвет кнопки
                    Size = new Size(30, 30), // Размер кнопки
                    Location = new Point(centerXlocal, centerYlocal),// Позиция кнопки на панели
                    Tag = Storage.Data.nodesCoords[numNodes, 0].ToString()
                };

                int centerNodeId = numRows - 1;
                // Установите координаты центрального узла в Storage.Data.nodesCoords
                Storage.Data.nodesCoords[centerNodeId, 1] = centerXlocal + centerNodeButton.Width / 2;
                Storage.Data.nodesCoords[centerNodeId, 2] = centerYlocal + centerNodeButton.Height / 2;

                centerNodeButton.Tag = Storage.Data.nodesCoords[numNodes, 0].ToString();

                // Добавьте обработчики событий для перемещения центрального узла
                centerNodeButton.MouseDown += NodeButton_MouseDown;
                centerNodeButton.MouseMove += NodeButton_MouseMove;
                centerNodeButton.MouseUp += NodeButton_MouseUp;

                panel2.Controls.Add(centerNodeButton);



                for (int i = 0; i < numNodes; i++)
                {
                    // Вычислите координаты узла вокруг центрального узла
                    int nodeX = centerXlocal + (int)(starRadius * Math.Cos(i * angleStep) - 15);
                    int nodeY = centerYlocal + (int)(starRadius * Math.Sin(i * angleStep) - 15);

                    // Проверка выхода кнопки за пределы панели по осям X и Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // Если кнопка выходит за оба предела, вернуть в ближайший угол
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                    }

                    // Создайте кнопку с заданными параметрами
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // Цвет кнопки
                        Size = new Size(30, 30), // Размер кнопки
                        Location = new Point(nodeX, nodeY), // Позиция кнопки на панели
                    };

                    // Определите номер узла
                    int nodeNumber = i;

                    // Установите координаты узла в Storage.Data.nodesCoords
                    Storage.Data.nodesCoords[nodeNumber, 1] = nodeX + nodeButton.Width / 2;
                    Storage.Data.nodesCoords[nodeNumber, 2] = nodeY + nodeButton.Height / 2;


                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();
                    // Добавьте обработчики событий для перемещения кнопок
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // Добавьте кнопку на панель
                    panel2.Controls.Add(nodeButton);

                    // Создайте экземпляр Connection и добавьте его в список connections
                    Connection connection = new Connection
                    {
                        StartNode = centerNodeButton,
                        EndNode = nodeButton
                    };
                    connections.Add(connection);
                }
            }


            else if (Storage.Data.selectedTopology == "Кольцо")
            {
                // Количество узлов в топологии "Кольцо"
                int numNodes = numRows;

                // Радиус кольца
                int ringRadius = 100; // Подставьте желаемый радиус

                // Угол между узлами кольца
                double angleStep = 2 * Math.PI / numNodes;

                List<Button> ringNodes = new List<Button>(); // Список узлов кольца

                for (int i = 0; i < numNodes; i++)
                {
                    // Вычислите координаты узла на окружности кольца
                    int nodeX = centerX + (int)(ringRadius * Math.Cos(i * angleStep) - 15);
                    int nodeY = centerY + (int)(ringRadius * Math.Sin(i * angleStep) - 15);

                    // Проверка выхода кнопки за пределы панели по осям X и Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // Если кнопка выходит за оба предела, вернуть в ближайший угол
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                    }

                    // Создайте кнопку с заданными параметрами
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // Цвет кнопки
                        Size = new Size(30, 30), // Размер кнопки
                        Location = new Point(nodeX, nodeY), // Позиция кнопки на панели

                    };

                    // Обновите координаты в Storage.Data.nodesCoords
                    int nodeNumber = i;
                    Storage.Data.nodesCoords[nodeNumber, 1] = nodeX + nodeButton.Width / 2;
                    Storage.Data.nodesCoords[nodeNumber, 2] = nodeY + nodeButton.Height / 2;

                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                    // Добавьте обработчики событий для перемещения кнопок
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // Добавьте кнопку на панель
                    panel2.Controls.Add(nodeButton);

                    // Добавьте узел в список узлов кольца
                    ringNodes.Add(nodeButton);

                    // Создайте соединение между текущим узлом и предыдущим узлом (если есть)
                    if (ringNodes.Count > 1)
                    {
                        Connection connection = new Connection
                        {
                            StartNode = ringNodes[ringNodes.Count - 2], // Предыдущий узел
                            EndNode = nodeButton // Текущий узел
                        };
                        connections.Add(connection);


                    }


                }

                // Добавьте соединение между первым и последним узлом для закрытия кольца
                if (ringNodes.Count > 2)
                {
                    Connection connection = new Connection
                    {
                        StartNode = ringNodes[ringNodes.Count - 1], // Последний узел
                        EndNode = ringNodes[0] // Первый узел
                    };
                    connections.Add(connection);


                }
            }

            else if (Storage.Data.selectedTopology == "Шина")
            {
                // Количество узлов в топологии "Шина"
                int numNodes = numRows;

                // Расстояние между узлами на шине
                int spacing = 30; // Подставьте желаемое расстояние

                // Начальная позиция X для первого узла
                int startX = centerX - (numNodes - 1) * spacing / 2;

                List<Button> busNodes = new List<Button>(); // Список узлов шины

                for (int i = 0; i < numNodes; i++)
                {
                    // Вычислите координаты узла на шине
                    int nodeX = startX + i * spacing;
                    int nodeY = centerY - 15; // Фиксированное значение Y (по центру)

                    // Проверка выхода кнопки за пределы панели по осям X и Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // Если кнопка выходит за оба предела, вернуть в ближайший угол
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                    }

                    // Создайте кнопку с заданными параметрами
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // Цвет кнопки
                        Size = new Size(30, 30), // Размер кнопки
                        Location = new Point(nodeX, nodeY), // Позиция кнопки на панели

                    };

                    int nodeNumber = i;
                    Storage.Data.nodesCoords[nodeNumber, 1] = nodeX + nodeButton.Width / 2;
                    Storage.Data.nodesCoords[nodeNumber, 2] = nodeY + nodeButton.Height / 2;

                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                    // Добавьте обработчики событий для перемещения кнопок
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // Добавьте кнопку на панель
                    panel2.Controls.Add(nodeButton);

                    // Добавьте узел в список узлов шины
                    busNodes.Add(nodeButton);

                    // Создайте соединения между узлами (кроме первого узла, так как он первый в шине)
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


            else if (Storage.Data.selectedTopology == "Полносвязная")
            {
                // Количество узлов в топологии "Полносвязная"
                int numNodes = numRows;


                // Расстояние между узлами (по горизонтали и вертикали)
                int nodeSpacing = 10; // Подставьте желаемое расстояние

                // Вычислите количество узлов в одном ряду и одном столбце, чтобы формировать квадрат
                int nodesPerRow = (int)Math.Ceiling(Math.Sqrt(numNodes));
                int nodesPerColumn = (int)Math.Ceiling((double)numNodes / nodesPerRow);

                for (int i = 0; i < numNodes; i++)
                {
                    // Вычислите позицию X и Y для текущего узла
                    int row = i / nodesPerRow;
                    int col = i % nodesPerRow;

                    int nodeX = centerX + col * (30 + nodeSpacing) - ((nodesPerRow - 1) * (30 + nodeSpacing)) / 2;
                    int nodeY = centerY + row * (30 + nodeSpacing) - ((nodesPerColumn - 1) * (30 + nodeSpacing)) / 2;

                    // Проверка выхода узла за пределы панели по осям X и Y
                    if (nodeX < 0 || nodeX + 30 > panelWidth || nodeY < 0 || nodeY + 30 > panelHeight)
                    {
                        // Если узел выходит за оба предела, вернуть в ближайший угол
                        int closestX = nodeX < 0 ? 0 : (nodeX + 30 > panelWidth ? panelWidth - 30 : nodeX);
                        int closestY = nodeY < 0 ? 0 : (nodeY + 30 > panelHeight ? panelHeight - 30 : nodeY);
                        nodeX = closestX;
                        nodeY = closestY;
                    }

                    // Создайте узел в виде прямоугольника с заданными параметрами
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // Цвет кнопки
                        Size = new Size(30, 30), // Размер кнопки
                        Location = new Point(nodeX, nodeY), // Позиция кнопки на панели

                    };

                    int nodeNumber = i;
                    Storage.Data.nodesCoords[nodeNumber, 1] = nodeX + nodeButton.Width / 2;
                    Storage.Data.nodesCoords[nodeNumber, 2] = nodeY + nodeButton.Height / 2;

                    nodeButton.Tag = Storage.Data.nodesCoords[i, 0].ToString();

                    // Добавьте обработчики событий для перемещения узла
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    // Добавьте узел на панель
                    panel2.Controls.Add(nodeButton);
                }

                // Создайте соединения между всеми парами узлов
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
                // Очистите матрицу смежности
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        Storage.Data.nodesConnections[i, j] = 0;
                    }
                }


                // Заполните матрицу смежности на основе списка connections
                foreach (Connection connection in connections)
                {
                    int startNodeId = int.Parse(connection.StartNode.Tag.ToString());
                    int endNodeId = int.Parse(connection.EndNode.Tag.ToString());

                    // Установите значение 1 в матрице смежности для соединенных узлов
                    Storage.Data.nodesConnections[startNodeId, endNodeId] = 1;
                    Storage.Data.nodesConnections[endNodeId, startNodeId] = 1; // Если граф ориентированный, то это нужно только для одного направления
                }
            }



            // Заполните длины соединений
            FillLengthConnections();

            panel2.Invalidate();
        }

        private void FillLengthConnections()
        {
            // Инициализация массива длин соединений
            Storage.Data.lengthConnections = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];


            foreach (Connection connection in connections)
            {
                int startNodeId = int.Parse(connection.StartNode.Tag.ToString());
                int endNodeId = int.Parse(connection.EndNode.Tag.ToString());

                // Получение координаты начального и конечного узла через свойство Location кнопок
                Point startLocation = connection.StartNode.Location;
                Point endLocation = connection.EndNode.Location;

                // Вычисление расстояния между узлами по формуле расстояния между двумя точками
                double length = Math.Sqrt(Math.Pow(endLocation.X - startLocation.X, 2) + Math.Pow(endLocation.Y - startLocation.Y, 2));

                // Заполнение соответствующего элемента массива длин соединений
                Storage.Data.lengthConnections[startNodeId, endNodeId] = length;
                Storage.Data.lengthConnections[endNodeId, startNodeId] = length; // Если граф ориентированный, то это нужно только для одного направления
            }
        }


        // Обработчик события при нажатии кнопки "Смоделировать"
        private void button1_Click_1(object sender, EventArgs e)
        {
            // Проверка выбранной топологии
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите топологию!");
                return;
            }

            // Инициализация данных для полного графа
            Storage.Data.nodesNumber = Storage.Data.nodesNumberNoTopology;
            Storage.Data.nodesCoords = new double[Storage.Data.nodesNumber, 3];
            Storage.Data.nodesConnections = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
            Storage.Data.lengthConnections = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
            Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            // Логирование
            Element_Click(sender);

            // Установка флага полного графа
            checkedFullGraph = true;

            // Копирование координат узлов 
            for (int i = 0; i < Storage.Data.nodesNumberNoTopology; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Storage.Data.nodesCoords[i, j] = Storage.Data.nodesCoordsNoTopology[i, j];
                }
            }

            // Копирование матрицы смежности 
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    Storage.Data.adjacencyMatrix[i, j] = Storage.Data.adjacencyMatrixNoTopology[i, j];
                }
            }

            // Создание элементов графа
            makeButtons();
        }

        // Активная кнопка узла
        private Button activeNodeButton = null;

        // Флаг перетаскивания
        private bool isDragging = false;

        // Нажатая кнопка
        private Button buttonPressed = null;

        // Первый узел для соединения
        private Button firstNodeToConnect = null;

        // Второй узел для соединения
        private Button secondNodeToConnect = null;

        // Флаг нажатия клавиши Ctrl
        private bool isCtrlPressed = false;

        // Флаг нажатия клавиши Shift
        private bool isShiftPressed = false;

        // Флаг нажатия клавиши Alt
        private bool isAltPressed = false;

        // Начальное положение узла
        private Point initialNodeLocation;


        // Обработчик события нажатия кнопки мыши на узле
        private void NodeButton_MouseDown(object sender, MouseEventArgs e)
        {
            // Проверка, что была нажата левая кнопка мыши
            if (e.Button == MouseButtons.Left)
            {
                // Обработка случая, когда нажата клавиша Ctrl
                if (isCtrlPressed)
                {
                    if (firstNodeToConnect == null)
                    {
                        firstNodeToConnect = sender as Button;
                    }
                    else if (secondNodeToConnect == null)
                    {
                        secondNodeToConnect = sender as Button;
                        // Создание соединения между выбранными узлами
                        CreateConnection();
                    }
                }
                // Обработка случая, когда нажата клавиша Shift
                else if (isShiftPressed)
                {
                    // Проверка, что число узлов не станет меньше двух
                    if (Storage.Data.nodesNumber == 2)
                    {
                        MessageBox.Show("Вы не можете оставить меньше двух узлов!");
                        return;
                    }

                    // Проверка, что выбрана топология "Нет"
                    if (comboBox1.SelectedItem.ToString() != "Нет")
                    {
                        MessageBox.Show("Вы не можете удалять узлы из настроенной топологии!");
                        return;
                    }

                    // Если нажатие Shift и нет активной кнопки, сохранение активной кнопки для удаления соединения
                    if (buttonPressed == null)
                    {
                        buttonPressed = sender as Button;
                        // Удаление соединения
                        DeleteConnection();
                    }
                }
                // Обработка случая, когда не нажата клавиша Ctrl или Shift
                else
                {
                    // Установка активной кнопки
                    activeNodeButton = sender as Button;

                    // Сохранение начальных координат узла перед его перемещением
                    initialNodeLocation = activeNodeButton.Location;
                    isDragging = true;
                }
            }
        }





        // Обработчик события перемещения узла
        private void NodeButton_MouseMove(object sender, MouseEventArgs e)
        {
            // Проверка, что идет перемещение и активная кнопка узла установлена
            if (isDragging && activeNodeButton != null)
            {
                // Получение нового положения активной кнопки
                Point newLocation = panel2.PointToClient(MousePosition);
                activeNodeButton.Location = new Point(newLocation.X - activeNodeButton.Width / 2, newLocation.Y - activeNodeButton.Height / 2);

                // Получение тега (номера узла) активной кнопки
                int nodeTag = int.Parse(activeNodeButton.Tag.ToString());

                // Обновление координат этого узла в матрице Storage.Data.nodesCoords
                Storage.Data.nodesCoords[nodeTag, 1] = activeNodeButton.Left + activeNodeButton.Width / 2;
                Storage.Data.nodesCoords[nodeTag, 2] = activeNodeButton.Top + activeNodeButton.Height / 2;

                // Заполнение длин соединений
                FillLengthConnections();

                // Вызов метода для изменения координат элемента
                Element_ChangeCoords(Storage.Data.nodesCoords[nodeTag, 1], Storage.Data.nodesCoords[nodeTag, 2], sender);

                // Обновление панели
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
            int offset = 5; // Смещение внутрь кнопок
            float lineWidth = 3.0f; // Фиксированная толщина линии

            foreach (var connection in connections)
            {
                Button startNode = connection.StartNode;
                Button endNode = connection.EndNode;

                // Получите центральную точку каждого узла
                Point startPoint = new Point(startNode.Left + startNode.Width / 2, startNode.Top + startNode.Height / 2);
                Point endPoint = new Point(endNode.Left + endNode.Width / 2, endNode.Top + endNode.Height / 2);

                // Сместите начальную и конечную точку на offset пикселей внутрь кнопок
                int deltaX = endPoint.X - startPoint.X;
                int deltaY = endPoint.Y - startPoint.Y;
                double length = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                double ratio = (length - offset) / length;

                int lineStartX = startPoint.X + (int)(deltaX * ratio);
                int lineStartY = startPoint.Y + (int)(deltaY * ratio);

                int lineEndX = endPoint.X - (int)(deltaX * ratio);
                int lineEndY = endPoint.Y - (int)(deltaY * ratio);

                // Определите цвет линии на основе свойств IsSaturated и IsUnsaturated
                Color lineColor;
                if (connection.IsSaturated)
                {
                    lineColor = Color.FromArgb(255, 0, 0); // Красный
                }
                else if (connection.IsUnsaturated)
                {
                    lineColor = Color.FromArgb(0, 255, 0); // Зеленый
                }
                else
                {
                    lineColor = Color.FromArgb(0, 0, 0); // Зеленый
                }

                // Нарисуйте линию между узлами с определенным цветом и фиксированной толщиной
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
                // Определите индексы узлов в матрице смежности по их Tag
                int startIndex = int.Parse(firstNodeToConnect.Tag.ToString());
                int endIndex = int.Parse(secondNodeToConnect.Tag.ToString());

                // Создайте соединение и добавьте его в список connections
                Connection connection = new Connection
                {
                    StartNode = firstNodeToConnect,
                    EndNode = secondNodeToConnect
                };
                connections.Add(connection);


                // Обновите матрицу смежности
                Storage.Data.nodesConnections[startIndex, endIndex] = 1;
                Storage.Data.nodesConnections[endIndex, startIndex] = 1; // Если граф ориентированный, то это нужно только для одного направления

                // Сбросьте выбранные узлы
                firstNodeToConnect = null;
                secondNodeToConnect = null;

                FillLengthConnections();
                // Перерисуйте панель для отображения соединения
                panel2.Invalidate();
            }
        }

        private void DeleteConnection()
        {
            if (buttonPressed != null)
            {
                int buttonIndex = int.Parse(buttonPressed.Tag.ToString());

                // Удалите связи с удаляемым узлом в матрице nodesConnections
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    if (Storage.Data.nodesConnections[buttonIndex, i] == 1)
                    {
                        // Установите соединение в 0, чтобы удалить его
                        Storage.Data.nodesConnections[buttonIndex, i] = 0;

                        // Удалите соединение из списка connections, если оно там хранится
                        Connection connectionToRemove = connections.FirstOrDefault(conn => (conn.StartNode == buttonPressed && conn.EndNode == panel2.Controls[i] as Button) || (conn.EndNode == buttonPressed && conn.StartNode == panel2.Controls[i] as Button));
                        if (connectionToRemove != null)
                        {
                            connections.Remove(connectionToRemove);
                        }
                    }
                }

                // Удалите кнопку из контейнера
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


                // Очистите переменную buttonPressed
                buttonPressed = null;



                // Удалите удаленный узел из всех связанных матриц и массивов
                UpdateMatricesAndArraysAfterNodeDeletion(buttonIndex);

                // Перерисуйте панель для отображения изменений
                panel2.Invalidate();
            }
        }


        // Метод для обновления матриц и массивов после удаления узла
        private void UpdateMatricesAndArraysAfterNodeDeletion(int deletedNodeIndex)
        {
            // Уменьшение размера матриц и массивов на 1
            int newSize = Storage.Data.nodesNumber - 1;

            // Обновленные координаты узлов
            double[,] updatedNodesCoords = new double[newSize, 3];

            // Обновленные длины соединений
            double[,] updatedLengthConnections = new double[newSize, newSize];

            // Обновленные соединения между узлами
            int[,] updatedNodesConnections = new int[newSize, newSize];

            // Обновленная матрица смежности
            double[,] updatedAdjacencyMatrix = new double[newSize, newSize];

            int row = 0;
            int col = 0;

            // Цикл по узлам
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                // Пропуск удаленного узла
                if (i == deletedNodeIndex)
                {
                    continue;
                }

                // Обновление координат узла
                if (i > deletedNodeIndex)
                {
                    updatedNodesCoords[row, 0] = Storage.Data.nodesCoords[i, 0] - 1;
                }
                else
                {
                    updatedNodesCoords[row, 0] = Storage.Data.nodesCoords[i, 0];
                }

                // Обновление координат узла (не удаленного)
                if (i != deletedNodeIndex)
                {
                    updatedNodesCoords[row, 1] = Storage.Data.nodesCoords[i, 1];
                    updatedNodesCoords[row, 2] = Storage.Data.nodesCoords[i, 2];

                    col = 0;
                    // Цикл по узлам для обновления соединений
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        // Пропуск удаленного узла
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

            // Обновление данных в хранилище
            Storage.Data.nodesCoords = updatedNodesCoords;
            Storage.Data.nodesConnections = updatedNodesConnections;
            Storage.Data.lengthConnections = updatedLengthConnections;
            Storage.Data.adjacencyMatrix = updatedAdjacencyMatrix;
            Storage.Data.nodesNumber = newSize;
        }




        private void UpdateConnections()
        {
            connections.Clear();
            // Переберите матрицу смежности и создайте соединения на основе её значений
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    // Получите значение в матрице смежности
                    int connectionValue = Storage.Data.nodesConnections[i, j];

                    // Проверьте, что есть соединение (значение 1)
                    if (connectionValue == 1)
                    {
                        // Найдите соответствующие узлы по их Tag
                        string startNodeId = Storage.Data.nodesCoords[i, 0].ToString();
                        string endNodeId = Storage.Data.nodesCoords[j, 0].ToString();

                        // Найдите кнопки, соответствующие узлам
                        Button startNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == startNodeId);
                        Button endNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == endNodeId);

                        if (startNode != null && endNode != null)
                        {
                            // Проверьте, не существует ли уже такого соединения в списке connections
                            Connection existingConnection = connections.FirstOrDefault(conn => (conn.StartNode == startNode && conn.EndNode == endNode) || (conn.StartNode == endNode && conn.EndNode == startNode));

                            if (existingConnection == null)
                            {
                                // Создайте соединение и добавьте его в список connections
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

            // Перерисуйте панель для отображения соединений
            panel2.Invalidate();
        }






        private void UpdateConnectionsFromDataGridView()
        {
            // Переберите матрицу смежности и создайте соединения на основе её значений
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    // Получите значение в матрице смежности
                    int connectionValue = Storage.Data.nodesConnections[i, j];

                    // Проверьте, что есть соединение (значение 1)
                    if (connectionValue == 1)
                    {
                        // Найдите соответствующие узлы по их Tag
                        string startNodeId = Storage.Data.nodesCoords[i, 0].ToString();
                        string endNodeId = Storage.Data.nodesCoords[j, 0].ToString();

                        // Найдите кнопки, соответствующие узлам
                        Button startNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == startNodeId);
                        Button endNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == endNodeId);

                        if (startNode != null && endNode != null)
                        {
                            // Проверьте, не существует ли уже такого соединения в списке connections
                            Connection existingConnection = connections.FirstOrDefault(conn => (conn.StartNode == startNode && conn.EndNode == endNode) || (conn.StartNode == endNode && conn.EndNode == startNode));

                            if (existingConnection == null)
                            {
                                // Создайте соединение и добавьте его в список connections
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

            // Перерисуйте панель для отображения соединений
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
            // Проверка на зажатие клавиши Ctrl
            if (e.Control)
            {
                isCtrlPressed = true;
            }
            // Проверка на зажатие клавиши Shift
            if (e.Shift)
            {
                isShiftPressed = true;
            }

            // Проверка на зажатие клавиши Alt
            if (e.Alt)
            {
                isAltPressed = true;
            }
        }

        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {
            // Проверка на отпускание клавиши Ctrl
            if (!e.Control)
            {
                isCtrlPressed = false;
            }
            // Проверка на зажатие клавиши Shift
            if (!e.Shift)
            {
                isShiftPressed = false;
            }
            // Проверка на зажатие клавиши Alt
            if (!e.Shift)
            {
                isAltPressed = false;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

            // Проверяем, что выбрана вкладка с текстом "Матрица смежности"
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Матрица смежности")
            {
                DataTable adjacencyMatrix = new DataTable();

                // Добавьте столбцы в таблицу для каждой вершины
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    adjacencyMatrix.Columns.Add(i.ToString(), typeof(int));
                }

                // Добавьте строки в таблицу для каждой вершины
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

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Матрица нагрузки")
            {
                DataTable loadMatrix = new DataTable();

                // Добавьте столбцы в таблицу для каждой вершины
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    loadMatrix.Columns.Add(i.ToString(), typeof(int));
                }

                // Добавьте строки в таблицу для каждой вершины
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


            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Матрица длин соединений")
            {
                DataTable lengthMatrix = new DataTable();

                // Добавьте столбцы в таблицу для каждой вершины
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    lengthMatrix.Columns.Add(i.ToString(), typeof(int));
                }

                // Добавьте строки в таблицу для каждой вершины
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

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Средняя задержка")
            {
                DataTable middleLatency = new DataTable();

                // Добавьте столбцы в таблицу для каждой вершины
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    middleLatency.Columns.Add(i.ToString(), typeof(double));
                }

                // Добавьте строки в таблицу для каждой вершины
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



            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Координаты")
            {
                DataTable coordsMatrix = new DataTable();

                coordsMatrix.Columns.Add("Номер узла", typeof(string));
                coordsMatrix.Columns.Add("X", typeof(string));
                coordsMatrix.Columns.Add("Y", typeof(string));

                // Добавьте строки в таблицу для каждой вершины
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

                // Проверьте значение ComboBox1 и установите ReadOnly в зависимости от условия
                if (comboBox1.SelectedItem != null && comboBox1.SelectedItem.ToString() != "Нет")
                {
                    dataGridView4.ReadOnly = true;
                }
                else
                {
                    dataGridView4.ReadOnly = false;
                }

                dataGridView4.DataSource = coordsMatrix;
            }

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Загруженность каналов")
            {
                DataTable sectionMatrix = new DataTable();

                // Добавьте столбцы в таблицу для каждой вершины
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    sectionMatrix.Columns.Add(i.ToString(), typeof(double));
                }

                // Добавьте строки в таблицу для каждой вершины
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

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Суммарная стоимость")
            {

                DataTable devices = new DataTable();

                // Добавьте столбцы в таблицу
                for (int i = 0; i < 5; i++)
                {
                    devices.Columns.Add(i.ToString(), typeof(string));
                }

                // Добавьте строки в таблицу
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    devices.Rows.Add();
                }

                // Заполните таблицу данными из devicesMatrix
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        devices.Rows[i][j] = Storage.Data.devicesMatrix[i, j];
                    }
                }

                dataGridView7.DataSource = devices;

                //// Каналы
                DataTable bandWidth = new DataTable();

                // Добавьте столбцы в таблицу
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

                            // Проверяем, содержится ли значение уже в списке
                            if (!Storage.Data.uniqueValuesList.Contains(valueToAdd))
                            {
                                // Если значения еще нет в списке, добавляем его
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

            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Пропускные способности")
            {

                DataTable bandWidth = new DataTable();

                // Добавьте столбцы в таблицу
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    bandWidth.Columns.Add(i.ToString(), typeof(string));
                }

                // Добавьте строки в таблицу
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    bandWidth.Rows.Add();
                }

                // Заполните таблицу данными из devicesMatrix
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
                return; // Избегаем обработки события для заголовков
            }

            // Получите значение ячейки
            object cellValue = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            // Попробуйте преобразовать значение в Int32
            if (int.TryParse(cellValue.ToString(), out int newValue))
            {
                // Проверьте, что изменяемая ячейка не находится на главной диагонали и значение равно 0 или 1
                if (e.RowIndex == e.ColumnIndex)
                {
                    // Если ячейка на главной диагонали, оставьте только значение 0
                    if (newValue != 0)
                    {
                        // Выведите предупреждение
                        MessageBox.Show("Нельзя изменять значения на главной диагонали. Значение должно быть 0.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Откатите изменение в ячейке
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                }
                else
                {
                    // Если ячейка не на главной диагонали, оставьте только значения 0 или 1
                    if (newValue != 0 && newValue != 1)
                    {
                        // Выведите предупреждение
                        MessageBox.Show("Значение может быть только 0 или 1.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Откатите изменение в ячейке
                        dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                    else
                    {
                        // Обновите соответствующее значение в матрице смежности
                        Storage.Data.nodesConnections[e.RowIndex, e.ColumnIndex] = newValue;

                        // Если значение было изменено вне главной диагонали
                        if (e.RowIndex != e.ColumnIndex)
                        {
                            // Обновите значение в симметричной ячейке
                            dataGridView1.Rows[e.ColumnIndex].Cells[e.RowIndex].Value = newValue;
                            // Обновите соответствующее значение в матрице смежности
                            Storage.Data.nodesConnections[e.ColumnIndex, e.RowIndex] = newValue;

                            // Если установлено значение 0, удалите соединение
                            if (newValue == 0)
                            {
                                // Найдите соединение по узлам и удалите его из списка connections
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
                // Если не удалось преобразовать значение в Int32, выведите предупреждение
                MessageBox.Show("Некорректное значение. Пожалуйста, введите целое число.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Откатите изменение в ячейке
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
            }

            // Вызовите метод обновления соединений
            UpdateConnectionsFromDataGridView();
        }


        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return; // Избегаем обработки события для заголовков
            }

            // Получите значение ячейки
            object cellValue = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            // Попробуйте преобразовать значение в Int32
            if (int.TryParse(cellValue.ToString(), out int newValue))
            {
                // Проверьте, что изменяемая ячейка не находится на главной диагонали и значение равно 0 или 1
                if (e.RowIndex == e.ColumnIndex)
                {
                    // Если ячейка на главной диагонали, оставьте только значение 0
                    if (newValue != 0)
                    {
                        // Выведите предупреждение
                        MessageBox.Show("Нельзя изменять значения на главной диагонали. Значение должно быть 0.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Откатите изменение в ячейке
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                }
                else
                {
                    // Если ячейка не на главной диагонали, оставьте только значения 0 или 1
                    if (newValue < 0)
                    {
                        // Выведите предупреждение
                        MessageBox.Show("Введите корректное значение!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Откатите изменение в ячейке
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                    else
                    {
                        // Обновите соответствующее значение в матрице смежности
                        Storage.Data.adjacencyMatrix[e.RowIndex, e.ColumnIndex] = newValue;

                        // Обновите также значение в соответствующей ячейке dataGridView1
                        dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = newValue;
                    }
                }
            }
            else
            {
                // Если не удалось преобразовать значение в Int32, выведите предупреждение
                MessageBox.Show("Некорректное значение. Пожалуйста, введите целое число.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Откатите изменение в ячейке
                dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
            }
            MatrixBandWidth();
        }


        private void dataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

        }

        private const int XColumnIndex = 1; // Индекс столбца для X координаты (где 0 - это первый столбец)
        private const int YColumnIndex = 2; // Индекс столбца для Y координаты
        private void dataGridView4_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            bool t = true;
            // Проверяем, что изменение произошло в ячейке с координатами (X или Y)
            if (e.ColumnIndex == XColumnIndex || e.ColumnIndex == YColumnIndex)
            {
                // Получаем новое значение из ячейки
                object newValue = dataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;



                // Проверяем, что значение не пустое и корректное
                if (newValue != null && double.TryParse(newValue.ToString(), out double newCoordinate))
                {
                    // Обновляем соответствующую координату в Storage.Data.nodesCoords
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

                    // Перерисовываем узлы на панели
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
                MessageBox.Show("Сначала реализуйте какую-нибудь топологию!");
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                if (isAltPressed)
                {
                    // Увеличьте размер матриц и массивов
                    int newSize = Storage.Data.nodesNumber + 1;
                    // Получите текущие координаты X и Y курсора в точке, где было нажатие
                    int cursorX = e.X;
                    int cursorY = e.Y;

                    // Создайте новую кнопку и добавьте ее на панель
                    Button nodeButton = new Button
                    {
                        BackColor = Color.Black, // Цвет кнопки
                        Size = new Size(30, 30), // Размер кнопки
                        Location = new Point(cursorX, cursorY) // Позиция кнопки на панели
                    };

                    // Добавьте обработчики событий для перемещения кнопок
                    nodeButton.MouseDown += NodeButton_MouseDown;
                    nodeButton.MouseMove += NodeButton_MouseMove;
                    nodeButton.MouseUp += NodeButton_MouseUp;

                    nodeButton.Tag = Storage.Data.nodesNumber.ToString();

                    panel2.Controls.Add(nodeButton);

                    // Обновите матрицы и массивы с учетом новой кнопки
                    int newIndex = Storage.Data.nodesNumber;




                    // Увеличение размера матрицы nodesCoords
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
                                // Устанавливаем значения по умолчанию, если вышли за пределы старой матрицы
                                newNodesCoords[i, j] = 0;
                            }
                        }
                    }
                    Storage.Data.nodesCoords = newNodesCoords;

                    // Увеличение размера матрицы nodesConnections
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
                                // Устанавливаем значения по умолчанию, если вышли за пределы старой матрицы
                                newNodesConnections[i, j] = 0;
                            }
                        }
                    }
                    Storage.Data.nodesConnections = newNodesConnections;

                    // Увеличение размера матрицы lengthConnections
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
                                // Устанавливаем значения по умолчанию, если вышли за пределы старой матрицы
                                newLengthConnections[i, j] = 0;
                            }
                        }
                    }
                    Storage.Data.lengthConnections = newLengthConnections;

                    // Увеличение размера матрицы adjacencyMatrix
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
                                // Устанавливаем значения по умолчанию, если вышли за пределы старой матрицы
                                newAdjacencyMatrix[i, j] = 0;
                            }
                        }
                    }
                    Storage.Data.adjacencyMatrix = newAdjacencyMatrix;




                    // Добавьте новые координаты для новой кнопки
                    Storage.Data.nodesCoords[newIndex, 0] = newIndex; // Используйте уникальный идентификатор (например, индекс) для тега
                    Storage.Data.nodesCoords[newIndex, 1] = nodeButton.Location.X;
                    Storage.Data.nodesCoords[newIndex, 2] = nodeButton.Location.Y;

                    // Обновите переменную с количеством узлов
                    Storage.Data.nodesNumber = newSize;
                }
            }
        }


        // ВМУР
        private void button7_Click_1(object sender, EventArgs e)
        {
            if (Storage.Data.nodesNumber == 0)
            {
                MessageBox.Show("Вы не добавили ни одного узла!");
                return;
            }
            if (textBox2.Text == "" || textBox3.Text == "" || textBox5.Text == "")
            {
                MessageBox.Show("Введены не все значения для возможности запуска ВМУР!");
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
                MessageBox.Show("Для алгоритма ВМУР требуется полносвязный граф!");
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
                    MessageBox.Show("У вас имеется узел, в который(из которого) не передаются данные");
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
                    MessageBox.Show("У вас имеется узел, в который(из которого) не передаются данные");
                    return;
                }
            }

            Storage.Data.middleLatency = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];


            // Средняя задержка по каналам
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
            //Максимальная задержка по каналам
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



            //расчет пропускных способностей
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
            //Вычисление весов ребер

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

        // Метод для выполнения алгоритма Дейкстры
        public void Dijkstra()
        {
            // Инициализация матрицы весов
            for (int i = 0; i < Storage.Data.nodesNumber; i++)
            {
                for (int j = 0; j < Storage.Data.nodesNumber; j++)
                {
                    if (i == j)
                        Storage.Data.weightMatrix[i, j] = double.MaxValue;
                }
            }

            // Инициализация массивов расстояний и посещений
            double[] distance = new double[Storage.Data.nodesNumber];
            bool[] visited = new bool[Storage.Data.nodesNumber];
            Storage.Data.MatrixDij = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];

            // Установка начальной вершины
            distance[Storage.Data.indexStartNode] = double.MaxValue;
            int startNode_2 = Storage.Data.indexStartNode;

            int k = 0;
            while (k < Storage.Data.nodesNumber)
            {
                if (startNode_2 == (Storage.Data.nodesNumber - 1))
                    startNode_2 = 0;

                // Инициализация расстояний и посещений
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
                    // Нахождение вершины с минимальным расстоянием, которая еще не посещена
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

                    // Обновление расстояний
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (!visited[j] && Storage.Data.weightMatrix[u, j] != double.MaxValue && distance[u] != double.MaxValue && (distance[u] + Storage.Data.weightMatrix[u, j] < distance[j]))
                        {
                            distance[j] = distance[u] + Storage.Data.weightMatrix[u, j];
                        }
                    }
                }

                // Заполнение матрицы Dij
                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    Storage.Data.MatrixDij[startNode_2, i] = distance[i];
                }

                startNode_2 += 1;
                k++;
            }

            // Установка значения double.MaxValue в диагональных элементах матрицы Dij
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

            // Обновление матрицы соединений
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

        // Метод для расчета D
        public double countD(double[,] Matrix)
        {
            D = 0;
            // Суммарная стоимость
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
            File.Create("saved.txt").Dispose(); // Создание файла-флага
            MessageBox.Show("Данные сохранены.");
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
            if (e.TabPage.Text == "Средняя задержка")
            {
                if (Storage.Data.middleLatency == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("Сначала нажмите кнопку реализации ВМУР.");
                }
            }
            if (e.TabPage.Text == "Матрица смежности")
            {
                if (Storage.Data.nodesConnections == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("Сначала нажмите кнопку Смоделировать.");
                }
            }
            if (e.TabPage.Text == "Матрица длин соединений")
            {
                if (Storage.Data.lengthConnections == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("Сначала нажмите кнопку Смоделировать.");
                }
            }
            if (e.TabPage.Text == "Координаты")
            {
                if (Storage.Data.nodesCoords == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("Сначала нажмите кнопку Смоделировать.");
                }
            }
            if (e.TabPage.Text == "Загруженность каналов")
            {
                if (Storage.Data.sectionMatrix == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("Сначала нажмите кнопку CSM.");
                }
            }
            if (e.TabPage.Text == "Суммарная стоимость")
            {
                if (Storage.Data.devicesMatrix == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("Сначала нажмите кнопку 'Загрузить данные'.");
                }
                else if (Storage.Data.matrixBandWidth == null)
                {
                    e.Cancel = true;
                    MessageBox.Show("Сначала загрузите матрицу нагрузки.");
                }
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = DialogResult.No;
            // Вывести диалоговое окно с вопросом
            result = MessageBox.Show("Хотите сохранить текущие данные?", "Вопрос", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                        MessageBox.Show("Не все узлы имеют каналы связи!");
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

                // Перерасчет T

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
                    MessageBox.Show("Инициализированы не все значения!");
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
                        // Проверка, что пара индексов ещё не обработана
                        if (!processedPairs[i, j] && !processedPairs[j, i] && j != i && Storage.Data.sectionMatrix[i, j] > max && Storage.Data.sectionMatrix[i, j] != 0)
                        {
                            max = Storage.Data.sectionMatrix[i, j];
                            I = i;
                            J = j;
                        }
                    }

                    // Отмечаем пару индексов как обработанную
                    processedPairs[I, J] = true;
                    processedPairs[J, I] = true;

                    Storage.Data.maxSectionMatrix[I, J] = 1;
                }

                // Вычислить среднюю загрузку всех узлов
                double averageLoad = CalculateAverageLoad();

                // Константы для порогов
                double loadConstant = 0.001;

                for (int i = 0; i < Storage.Data.nodesNumber; i++)
                {
                    for (int j = 0; j < Storage.Data.nodesNumber; j++)
                    {
                        if (Storage.Data.nodesConnections[i, j] == 1)
                        {
                            double load = Storage.Data.sectionMatrix[i, j];

                            // Пороги загрузки
                            double lowerThreshold = averageLoad;

                            // Проверьте, является ли узел загруженным
                            bool isSaturated = Storage.Data.maxSectionMatrix[i, j] == 1;

                            // Проверьте, является ли узел не загруженным
                            bool isUnsaturated = load < lowerThreshold;

                            // Найдите соответствующие узлы по их Tag
                            string startNodeId = Storage.Data.nodesCoords[i, 0].ToString();
                            string endNodeId = Storage.Data.nodesCoords[j, 0].ToString();

                            // Найдите кнопки, соответствующие узлам
                            Button startNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == startNodeId);
                            Button endNode = panel2.Controls.OfType<Button>().FirstOrDefault(button => button.Tag != null && button.Tag.ToString() == endNodeId);

                            if (startNode != null && endNode != null)
                            {
                                // Проверка, было ли уже создано соединение в одном из направлений
                                bool connectionExists = connections.Any(c =>
                                    (c.StartNode == startNode && c.EndNode == endNode) ||
                                    (c.StartNode == endNode && c.EndNode == startNode));

                                // Если соединение еще не создано или isSaturated == true, создайте его
                                if (!connectionExists || isSaturated)
                                {
                                    // Создать новое соединение с цветом в зависимости от насыщенности
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

                // Перерисовать панель, чтобы отобразить новые цвета
                panel2.Invalidate();
            }
            catch (NullReferenceException nullEx)
            {
                MessageBox.Show("Введены не все значения!");
            }
            catch (System.Exception ex) {
                MessageBox.Show("Инициализированы не все значения!");
            }
            
        }

        // Метод для вычисления средней загрузки всех узлов
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
            openFileDialog1.Filter = "Текстовые файлы (*.txt)|*.txt";
            openFileDialog1.Title = "Выберите файл";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Проверяем, что имя файла оканчивается на "_dv.txt"
                    if (Path.GetFileNameWithoutExtension(openFileDialog1.FileName).EndsWith("_dv"))
                    {
                        using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                        {
                            // Читаем первую строку и сравниваем с Storage.Data.nodesNumber
                            string firstLine = sr.ReadLine();
                            if (firstLine != null && firstLine.Trim() == Storage.Data.nodesNumber.ToString())
                            {
                                // Создаем матрицу с учетом 5 столбцов и Storage.Data.nodesNumber строк
                                Storage.Data.devicesMatrix = new string[Storage.Data.nodesNumber, 5];

                                // Читаем остальные строки и заполняем матрицу
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
                                        MessageBox.Show("Некорректный формат данных в файле!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("Выберите конфигурацию для подходящего количества узлов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                    }
                    else
                    {
                        MessageBox.Show("Выберите файл с правильным суффиксом (_dv)!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView8_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
           
            // Временно отсоединяем обработчик события
            dataGridView8.CellValueChanged -= dataGridView8_CellValueChanged;

            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    // Получаем значение ячейки
                    object cellValue = dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                    // Пытаемся преобразовать значение в Int32
                    if (int.TryParse(cellValue?.ToString(), out int newValue))
                    {
                        if (e.RowIndex == e.ColumnIndex)
                        {
                            // Если ячейка на главной диагонали, оставляем только значение 0
                            if (newValue != 0)
                            {
                                MessageBox.Show("Нельзя изменять значения на главной диагонали. Значение должно быть 0.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                // Откатываем изменение в ячейке
                                dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                            }
                        }
                        else
                        {
                            double value = Storage.Data.matrixBandWidth[e.RowIndex, e.ColumnIndex];
                            // Если ячейка не на главной диагонали, оставляем только значения 0 или 1
                            if (newValue < 0)
                            {
                                MessageBox.Show("Введите корректное значение!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                // Откатываем изменение в ячейке
                                dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                            }
                            if (newValue < Storage.Data.adjacencyMatrix[e.RowIndex, e.ColumnIndex])
                            {
                                MessageBox.Show("Введенное значение меньше, чем соответствующее значение " +
                                    " в матрице нагрузки", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                // Откатываем изменение в ячейке
                                dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = value;
                            }
                            else
                            {
                                // Обновляем соответствующее значение в матрице смежности
                                Storage.Data.matrixBandWidth[e.RowIndex, e.ColumnIndex] = newValue;
                                dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = newValue;
                            }
                        }
                    }
                    else
                    {
                        // Если не удалось преобразовать значение в Int32, выводим предупреждение
                        MessageBox.Show("Некорректное значение. Пожалуйста, введите целое число.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        // Откатываем изменение в ячейке
                        dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                }
            }
            finally
            {
                // Переподсоединяем обработчик события
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
                // Временно отсоединяем обработчик события
                dataGridView9.CellValueChanged -= dataGridView9_CellValueChanged;
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                {
                    return; // Избегаем обработки события для заголовков
                }

                // Получите значение ячейки
                object cellValue = dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                // Попробуйте преобразовать значение в Int32
                if (int.TryParse(cellValue.ToString(), out int newValue))
                {
                    if (newValue < 0)
                    {
                        // Выведите предупреждение
                        MessageBox.Show("Пожалуйста, введите корректное значение стоимости!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Откатите изменение в ячейке
                        dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                    }
                    else
                    {
                        // Используйте e.RowIndex для строк и e.ColumnIndex для столбцов
                        dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = newValue;
                    }
                }
                else
                {
                    // Если не удалось преобразовать значение в Int32, выведите предупреждение
                    MessageBox.Show("Некорректное значение. Пожалуйста, введите целое число.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Откатите изменение в ячейке
                    dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                }
            }
            catch
            {

            }
            finally
            {
                // Переподсоединяем обработчик события
                dataGridView9.CellValueChanged += dataGridView9_CellValueChanged;
            }
        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            try {
                if (Storage.Data.lengthConnections == null)
                {
                    MessageBox.Show("Сначала реализуйте топологию!");
                }
                // Обработка dataGridView7
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

                // Обработка dataGridView9
                double totalSum = 0;

                // Предполагаем, что DataGridView9 уже создан и у него есть два столбца.

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
                            // Ищем соответствующее значение в первом столбце DataGridView9
                            DataGridViewRow row = dataGridView9.Rows
                                .Cast<DataGridViewRow>()
                                .FirstOrDefault(r => r.Cells[0].Value != null && Convert.ToDouble(r.Cells[0].Value).Equals(bandWidthValue));

                            if (row != null)
                            {
                                // Находим индекс текущего столбца
                                int columnIndex = dataGridView9.Columns["Column2"].Index;

                                // Получаем значение из второго столбца DataGridView9
                                double coefficient = Convert.ToDouble(row.Cells[columnIndex].Value);

                                // Умножаем на соответствующее значение матрицы lengthConnections
                                totalSum += coefficient * Storage.Data.lengthConnections[i, j];

                            }
                        }
                    }
                }



                // Вывод суммы обоих DataGridView в label9
                double totalSumBothGrids = sumDataGridView7 + Math.Round(totalSum);
                label9.Text = totalSumBothGrids.ToString();
            }
            catch(Exception ex) {
                MessageBox.Show("Введите данные!");
            }
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}