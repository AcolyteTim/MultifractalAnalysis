using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OpenCvSharp;

namespace MultifractalAnalysis.Model.Functionality
{
    public class ImageProcessing
    {
        // Неизменяемый список варьируемых параметров Q
        private static readonly int[] _varaibleParameters = { -60, -50, -40, -30, -25, -20, -15, -12, -10, -8, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 8, 10, 12, 15, 20, 25, 30, 40, 50, 60 }; //33 элемента

        // Метод получения внешних контуров изображения
        public static List<List<OpenCvSharp.Point>> GetOuterContours(
            Mat img,
            double threshValue = 24,
            int maxParentIndex = 20,
            int minContourArea = 5)
        {
            // Преобразование в градации серого
            Mat grayImg = new Mat();
            Cv2.CvtColor(img, grayImg, ColorConversionCodes.BGR2GRAY);

            // Увеличение контрастности
            Cv2.EqualizeHist(grayImg, grayImg);

            // Применение порогового фильтра
            Mat thresh = new Mat();
            Cv2.Threshold(grayImg, thresh, threshValue, 255, ThresholdTypes.Binary); 

            // Нахождение контуров
            Cv2.FindContours(thresh, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] hierarchyIndex, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

            grayImg.Dispose();
            thresh.Dispose();

            // Получение внешних контуров
            List<List<OpenCvSharp.Point>> outerContoursList = new List<List<OpenCvSharp.Point>>();

            for (int i = 0; i < hierarchyIndex.Length; i++)
            {
                if (hierarchyIndex[i].Parent <= maxParentIndex)
                {
                    if (Cv2.ContourArea(contours[i]) >= minContourArea)
                    {
                        outerContoursList.Add(contours[i].ToList());
                    }
                }
            }

            return outerContoursList;
        }

        // Расчет количества значимых точек в областях
        public static List<int> RectsCountPixelsStatistics(
            Mat image,
            int rectSize = 5)
        {
            // Преобразование в градации серого
            Mat grayImg = new Mat();
            Cv2.CvtColor(image, grayImg, ColorConversionCodes.BGR2GRAY);

            int rows = grayImg.Rows / rectSize;
            int cols = grayImg.Cols / rectSize;

            List<int> whitePixelsCount = new List<int>();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // Создание нового прямоугольника с координатами ячейки
                    OpenCvSharp.Rect cellRect = new OpenCvSharp.Rect(j * rectSize, i * rectSize, rectSize, rectSize);

                    // Получение матрицы изображения для ячейки
                    Mat cell = new Mat(image, cellRect);

                    // Расчет кол-ва белых пикселей в ячейке
                    int whiteCount = 0;
                    for (int y = 0; y < cell.Rows; y++)
                    {
                        for (int x = 0; x < cell.Cols; x++)
                        {
                            byte pixelValue = cell.Get<byte>(y, x);
                            if (pixelValue != 0)
                            {
                                whiteCount++;
                            }
                        }
                    }

                    // Записать результаты в список
                    whitePixelsCount.Add(whiteCount);
                }
            }

            return whitePixelsCount;
        }

        // Расчет вероятности нахождения закрашенного пикселя в конкретной ячейке
        public static double[] RectsPropability(List<int> rectsInfo)
        {
            var coloredPixelsCount = rectsInfo.Sum();

            double[] arrayOfPropability = new double[rectsInfo.Count()];

            for (int i = 0; i < rectsInfo.Count(); i++)
            {
                arrayOfPropability[i] = (double)rectsInfo[i] / coloredPixelsCount;
            }

            return arrayOfPropability;
        }

        // Расчет статсуммы
        public static double[] RectsStatisticSum(IEnumerable<double> arrayOfPropability, int[] rightVariableParameters)
        {
            double[] rectsStatisticSum = new double[rightVariableParameters.Length];
            int rectsStatisticSumSize = rectsStatisticSum.Length;
            double calculatedValue;

            for (int i = 0; i < rectsStatisticSumSize; i++)
            {
                double sum = 0;
                foreach (var item in arrayOfPropability)
                {
                    if (item == 0) { }
                    else
                    {
                        if (rightVariableParameters[i] == 1)
                        {
                            calculatedValue = item * Math.Log(item);
                            if (Double.IsInfinity(calculatedValue) || Double.IsNaN(calculatedValue))
                            {
                                calculatedValue = 0.0;
                            }
                            sum += calculatedValue;
                        }
                        else
                        {
                            calculatedValue = Math.Pow(item, rightVariableParameters[i]);
                            if (Double.IsInfinity(calculatedValue) || Double.IsNaN(calculatedValue))
                            {
                                calculatedValue = 0.0;
                            }
                            sum += calculatedValue;
                        }
                    }
                }
                // тут проверку из цикла или может еще выше, чтобы в ситуациях с infinity не прибавлять к нему прямо до конца цикла, а скипать
                rectsStatisticSum[i] = sum;
            }

            // наверное стоит включить во внутрь основного цикла
            for (int i = 0; i < rectsStatisticSumSize; i++)
            {
                if (Double.IsInfinity(rectsStatisticSum[i]) || Double.IsNaN(rectsStatisticSum[i]))
                {
                    rectsStatisticSum[i] = 0.0;
                }
            }

            return rectsStatisticSum;
        }

        // Получение массива варьируемых параметров
        public static int[] GetRightVariableParameters(int variableParameter = 40)
        {
            int[] rightVariableParameters;

            if (variableParameter < 60)
            {
                int skipped = 0;
                for (int i = _varaibleParameters.Length - 1; i > 0; i--)
                {
                    if (variableParameter == _varaibleParameters[i] || variableParameter == _varaibleParameters[i - 1])
                    {
                        skipped++;
                        break;
                    }
                    if (variableParameter < _varaibleParameters[i] && variableParameter > _varaibleParameters[i - 1])
                    {
                        break;
                    }
                    skipped++;
                }

                rightVariableParameters = new int[_varaibleParameters.Length - skipped * 2];

                for (int i = 0; i < rightVariableParameters.Length - 2; i++)
                {
                    rightVariableParameters[i + 1] = _varaibleParameters[i + skipped + 1];
                }
                rightVariableParameters[0] = -variableParameter;
                rightVariableParameters[rightVariableParameters.Length - 1] = variableParameter;
            }
            else
            {
                rightVariableParameters = _varaibleParameters;
            }

            return rightVariableParameters;
        }

        // Расчет статсуммы для диапазона 
        public static double[,] RectsStatisticSumInRange( 
            Mat image, 
            int[] sideSizesArray,
            int[] rightVariableParameters
            )
        { 

            int width = sideSizesArray.Length;
            int height = rightVariableParameters.Length;

            double[,] statSumComplete = new double[width, height];
            for (int row = 0; row < width; row++)
            {
                var statistics = RectsCountPixelsStatistics(image, sideSizesArray[row]); 
                var rectsPropability = RectsPropability(statistics);
                var statSum = RectsStatisticSum(rectsPropability, rightVariableParameters);

                for (int col = 0; col < height; col++)
                {
                    statSumComplete[row, col] = statSum[col];
                }
            }

            return statSumComplete;
        }

        // Линейная апроксимация
        public static double LinearApproximation(double[] x, double[] y)
        {
            int n = x.Length;

            // Расчет суммы x, y, x^2, xy
            double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
            for (int i = 0; i < n; i++)
            {
                if (x[i] != 0)
                {
                    sumX += Math.Log(x[i]);
                    sumY += Math.Log(y[i]);
                    sumXX += Math.Log(x[i]) * Math.Log(x[i]);
                    sumXY += Math.Log(x[i]) * Math.Log(y[i]);
                }
            }

            // Расчет коэффициентов линейной регрессии
            double slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);

            return slope;
        }
        public static double LinearApproximationWithoutLog(double[] x, double[] y)
        {
            int n = x.Length;

            // Расчет суммы x, y, x^2, xy
            double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
            for (int i = 0; i < n; i++)
            {
                if (x[i] != 0)
                {
                    sumX += Math.Log(x[i]);
                    sumY += y[i];
                    sumXX += Math.Log(x[i]) * Math.Log(x[i]);
                    sumXY += Math.Log(x[i]) * y[i];
                }
            }

            // Расчет коэффициентов линейной регрессии
            double slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);

            return slope;
        }

        // Расчет экспоненты обобщенной корреляционной функции
        public static double[] GetTauArray(double[,] statSumArray, int[] sideSizesArray)
        {
            double[] tauArray = new double[statSumArray.GetLength(1)];
            double[] sideSizesArrayDouble = sideSizesArray.Select(x => (double)x).ToArray();

            for (int j = 0; j < statSumArray.GetLength(1); j++)
            {
                double[] temporary = new double[statSumArray.GetLength(0)];
                for (int i = 0; i < statSumArray.GetLength(0); i++)
                {
                    temporary[i] = statSumArray[i, j];
                }

                if (j == tauArray.Length / 2 + 1)
                {
                    tauArray[j] = LinearApproximationWithoutLog(sideSizesArrayDouble, temporary);
                }
                else
                {
                    tauArray[j] = LinearApproximation(sideSizesArrayDouble, temporary);
                }
            }

            return tauArray;
        }

        // Получение обобщенных спектров размерностей Реньи (результат инверсивен)
        public static double[] GetRenie(double[] tauArray, int[] variableParameters)
        {
            double[] renieValues = new double[tauArray.Length];
            for (int j = 0; j < tauArray.Length; j++)
            {
                if (j == tauArray.Length / 2 + 1)
                {
                    renieValues[j] = tauArray[j];
                }
                else
                {
                    renieValues[j] = tauArray[j] / (variableParameters[j] - 1);
                }
            }

            return renieValues;
        }

        public static double LinearApproximationForSpectra(double[] x, double[] y)
        {
            int n = x.Length;

            // Расчет суммы x, y, x^2, xy
            double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
            for (int i = 0; i < n; i++)
            {
                if (x[i] != 0)
                {
                    sumX += x[i];
                    sumY += y[i];
                    sumXX += x[i] * x[i];
                    sumXY += x[i] * y[i];
                }
            }

            // Расчет коэффициентов линейной регрессии
            double slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);

            return slope;
        }


        public class SpectrumData
        {
            public SpectrumData(double[] inputIntermediateDiffs, double[] inputSpectrums)
            {
                intermediateDifferences = inputIntermediateDiffs;
                spectrums = inputSpectrums;
            }

            private double[] spectrums;
            public double[] Spectrums
            {
                get { return spectrums; }
                set { spectrums = value; }
            }

            private double[] intermediateDifferences;
            public double[] IntermediateDifferences
            {
                get { return intermediateDifferences; }
                set { intermediateDifferences = value; }
            }
        }

        public static SpectrumData GetSpectraData(double[] trueRenie, int[] variableParameters) ///////////////////////////////////////////////////////////////////////
        {
            double[] spectrums = new double[trueRenie.Length - 1];
            double[] intermediateDifferences = new double[trueRenie.Length - 1];

            for (int i = 0; i < spectrums.Length; i++)
            {
                intermediateDifferences[i] = ((trueRenie[i + 1] * (variableParameters[i + 1] - 1)) - (trueRenie[i] * (variableParameters[i] - 1))) / (variableParameters[i + 1] - variableParameters[i]);
                spectrums[i] = intermediateDifferences[i] * (variableParameters[i + 1]) - (trueRenie[i + 1] * (variableParameters[i + 1] - 1));
            }

            return new SpectrumData(intermediateDifferences, spectrums);
        }

        public static BitmapImage MatToBitmapImage(Mat image)
        {
            byte[] imageData = image.ToBytes();

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageData);
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}