using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetWork
{
    [Serializable]
    public class StorageData
    {
        // Коэффициент линейной функции
        public double d;

        // Ограничение средней задержки
        public double T;

        // Максимальная задержка
        public double maxLatency;

        // Индекс выбранной топологии
        public int comboBoxSelectedIndex;

        // Максимальная средняя задержка
        public double maxAverageLetency;

        // Количество узлов
        public int nodesNumber;

        // Матрица устройств
        public string[,] devicesMatrix;

        // Матрица координат
        public double[,] nodesCoords;

        // Матрица средних задержек
        public double[,] middleLatency;

        // Матрица длин соединений
        public double[,] lengthConnections;

        // Матрица пропускных способностей
        public double[,] matrixBandWidth;

        // Матрица для алгоритма Дейкстры
        public double[,] MatrixDij;

        // Матрица весов
        public double[,] weightMatrix;

        // Матрица нагрузки
        public double[,] adjacencyMatrix;

        // Матрица без топологии для матрицы смежности
        public int[,] adjacencyMatrixNoTopology;

        // Матрица смежности
        public int[,] nodesConnections;

        // Значения пропускных способностей
        public List<int> bandWidth = new List<int> { 100, 200, 300, 500, 1000, 1500, 2000 };

        // Список уникальных значений
        public List<double> uniqueValuesList = new List<double>();

        // Выбранная топология
        public string selectedTopology;

        public double[,] nodesCoordsNoTopology;

        public int nodesNumberNoTopology;

        // Индекс точки старта для ВМУР
        public int indexStartNode;

        // Матрица загруженности каналов (CSM)
        public double[,] sectionMatrix;

        // Максимальная матрица загруженности каналов
        public double[,] maxSectionMatrix;
    }

    // Статический класс для хранения и управления данными
    public static class Storage
    {
        // Свойство для доступа к сохраненным данным
        public static StorageData Data { get; set; }

        // Свойство для проверки, сохранены ли данные
        public static bool IsDataSaved
        {
            get
            {
                string filePath = "saved.txt";

                // Проверка наличия файла
                if (File.Exists(filePath))
                {
                    return true;
                }
                return false;
            }
        }

        // Метод для сохранения данных в файл
        public static void SaveData(string fileName)
        {
            try
            {
                // Создание потока файла
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    // Использование BinaryFormatter для сериализации
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, Data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при сохранении данных: " + e.Message);
            }
        }

        // Метод для загрузки данных из файла
        public static void LoadData(string fileName)
        {
            try
            {
                // Открытие потока файла
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    // Использование BinaryFormatter для десериализации
                    IFormatter formatter = new BinaryFormatter();
                    Data = (StorageData)formatter.Deserialize(fs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при загрузке данных: " + e.Message);
            }
        }
    }
}
