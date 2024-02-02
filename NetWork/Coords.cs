using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetWork
{
    // Форма для ввода координат узлов
    public partial class Coords : Form
    {
        // Индекс для отслеживания текущего узла
        int i = 0;

        // Счетчик свободных узлов
        int counterFreeNodes = Storage.Data.nodesNumber;

        // Конструктор формы
        public Coords()
        {
            InitializeComponent();
        }

        // Обработчик события при нажатии кнопки "Добавить узел"
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Запись координат в матрицу узлов 
                Storage.Data.nodesCoords[i, 0] = i;
                Storage.Data.nodesCoords[i, 1] = double.Parse(textBox2.Text);
                Storage.Data.nodesCoords[i, 2] = double.Parse(textBox3.Text);

                Storage.Data.nodesCoordsNoTopology[i, 0] = i;
                Storage.Data.nodesCoordsNoTopology[i, 1] = double.Parse(textBox2.Text);
                Storage.Data.nodesCoordsNoTopology[i, 2] = double.Parse(textBox3.Text);

                // Уменьшение счетчика свободных узлов
                counterFreeNodes--;
                textBox1.Text = counterFreeNodes.ToString();

                // Если свободные узлы закончились, выполняются дополнительные действия
                if (counterFreeNodes.Equals(0))
                {
                    // Инициализация матриц и завершение добавления узлов
                    Storage.Data.adjacencyMatrix = new double[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
                    Storage.Data.adjacencyMatrixNoTopology = new int[Storage.Data.nodesNumber, Storage.Data.nodesNumber];
                    MatrixBandWidth();
                    button1.Enabled = false;
                    this.Close();
                    MessageBox.Show("Добавление успешно завершено!");
                }
                i++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Введите корректные координаты узла!");
                return;
            }
        }

        // Обработчик события загрузки формы
        private void Coords_Load(object sender, EventArgs e)
        {
            // Установка значения счетчика свободных узлов и скрытие кнопки закрытия формы
            textBox1.Text = counterFreeNodes.ToString();
            this.ControlBox = false;
        }

        // Обработчик события при нажатии кнопки "Отмена"
        private void button2_Click(object sender, EventArgs e)
        {
            // Закрытие формы
            this.Close();
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
                }
            }
        }
    }
}
