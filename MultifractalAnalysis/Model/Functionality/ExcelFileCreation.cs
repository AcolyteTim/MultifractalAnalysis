using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MultifractalAnalysis.Model.Functionality
{
    public class ExcelFileCreation
    { 
        public static void CreateExcelFile(int[]variableParameters, List<double[]> trueRenieArrays, List<double[]> intermediateDifferencesArrays, List<double[]> spectrumsArrays, AppSettings appSettings, string fileName)
        {
            try
            {
                // Выбор файла для сохранения
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = fileName;

                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }

                var filePath = saveFileDialog.FileName;

                CreateExcelFileWithExactWay(variableParameters, trueRenieArrays, intermediateDifferencesArrays, spectrumsArrays, appSettings, filePath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } 

        public static void CreateExcelFileWithExactWay(int[] variableParameters, List<double[]> trueRenieArrays, List<double[]> intermediateDifferencesArrays, List<double[]> spectrumsArrays, AppSettings appSettings, string filePath)
        {
            try
            {
                // Создание нового объекта Excel
                var excelApp = new Application();
                excelApp.Visible = false;
                // Создание новой рабочей книги Excel
                var workbook = excelApp.Workbooks.Add();

                // Выбор текущего листа для внесения общих данных
                var worksheet = (Worksheet)workbook.ActiveSheet;
                worksheet.Name = "Общий";

                // Длина массива значений Реньи для координации
                int height = trueRenieArrays[0].Length;

                // Создание заголовков для общего листа
                worksheet.Cells[2, 1] = "Dq(0)";
                worksheet.Cells[3, 1] = "Dq(1)";
                worksheet.Cells[4, 1] = "Dq(2)";
                worksheet.Cells[5, 1] = $"Dmin q({variableParameters[0] / 2})";
                worksheet.Cells[6, 1] = $"Dmax q({(variableParameters[variableParameters.Length-1]) / 2})";

                // Установка ширины столбца заголовков
                var rangeToChange = worksheet.Range["A:A"];
                rangeToChange.ColumnWidth = 51;
                // Выравнивание по правому краю данных заголовков
                rangeToChange = worksheet.Range["A2:A6"];
                rangeToChange.HorizontalAlignment = XlHAlign.xlHAlignRight;

                // Внесение информации об использованных параметрах
                worksheet.Cells[8, 1] = "Параметры";
                worksheet.Cells[9, 1] = "Чувстительность преобразования в ЧБ:";
                worksheet.Cells[9, 2] = appSettings.ThresholdValueSetting;
                worksheet.Cells[10, 1] = "Изображение инвертировано:";
                worksheet.Cells[10, 2] = appSettings.ImageInversionSetting;
                worksheet.Cells[11, 1] = "Фильтрация неоднородностей (в пикселях):";
                worksheet.Cells[11, 2] = appSettings.MinSignificantAreaSetting;
                worksheet.Cells[12, 1] = "Выбранная мера (размеры ячеек) для обработки:";
                worksheet.Cells[12, 2] = appSettings.RectsSizesSetting;                               
                worksheet.Cells[13, 1] = "Предельные значения для варьируемого параметра q:";
                worksheet.Cells[13, 2] = appSettings.VariableParameterSetting;

                // Жирный шрифт и выравнивание по центру для заголовка "Параметры"
                rangeToChange = worksheet.Range["A8:A8"];
                rangeToChange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                rangeToChange.Font.Bold = true;

                // Заполнение общего листа данными Dq
                for (int i = 0; i < trueRenieArrays.Count; i++)
                {
                    double[] trueRenie = trueRenieArrays[i];

                    worksheet.Cells[1, i + 2] = i + 1;
                    worksheet.Cells[2, i + 2] = trueRenie[((trueRenie.Length - 1) / 2)];
                    worksheet.Cells[3, i + 2] = trueRenie[((trueRenie.Length - 1) / 2) + 1];
                    worksheet.Cells[4, i + 2] = trueRenie[((trueRenie.Length - 1) / 2) + 2];
                    worksheet.Cells[5, i + 2] = trueRenie[0];
                    worksheet.Cells[6, i + 2] = trueRenie[trueRenie.Length - 1];
                }


                for (int i = 2; i <= 6; i++) // i - строки таблицы Excel
                {
                    // Получение диапазона строки, преобразование в массив
                    Microsoft.Office.Interop.Excel.Range rowRange = worksheet.Range[$"B{i}:{GetColumnName(trueRenieArrays.Count + 1)}{i}"];
                    try
                    {
                        Array rowValues = (Array)rowRange.Value;

                        double sum = 0;
                        double min = double.MaxValue;
                        double max = double.MinValue;

                        foreach (var cellValue in rowValues)
                        {
                            // Пропуск пустых ячеек
                            if (cellValue == null || cellValue is DBNull)
                            {
                                continue;
                            }

                            double value = Convert.ToDouble(cellValue);

                            // Вычисление суммы
                            sum += value;

                            // Обновление минимального и максимального значения
                            if (value < min)
                                min = value;

                            if (value > max)
                                max = value;
                        }

                        worksheet.Cells[i, trueRenieArrays.Count + 2] = sum / trueRenieArrays.Count;
                        worksheet.Cells[i, trueRenieArrays.Count + 3] = max - min;
                    }
                    catch
                    {
                        worksheet.Cells[i, trueRenieArrays.Count + 2] = Convert.ToDouble(rowRange.Value);
                        worksheet.Cells[i, trueRenieArrays.Count + 3] = 0;
                    }
                }

                // Заголовки к данным значениям
                worksheet.Cells[1, trueRenieArrays.Count + 2] = "Среднее";
                worksheet.Cells[1, trueRenieArrays.Count + 3] = "MAX - MIN";

                // Выравнивание заголовков по центру
                rangeToChange = worksheet.Range["1:1"];
                rangeToChange.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                // Изменение ширины столбцов со значениями 
                rangeToChange = worksheet.Range["B1:Z1"];
                rangeToChange.ColumnWidth = 15;

                // Запись на отдельные листы подробной информации и графиков
                for (int k = 0; k < trueRenieArrays.Count; k++)
                {
                    double[] trueRenie = trueRenieArrays[k];
                    double[] intermediateDifferences = intermediateDifferencesArrays[k];
                    double[] spectrums = spectrumsArrays[k];

                    // Создание нового листа
                    worksheet = (Worksheet)workbook.Sheets.Add(After: workbook.Sheets[workbook.Sheets.Count]);
                    worksheet.Name = $"Изображение {k + 1}";

                    // Запись заголовков на лист
                    worksheet.Cells[1, 1] = "q";
                    worksheet.Cells[1, 2] = "Dq";
                    worksheet.Cells[1, 4] = "a";
                    worksheet.Cells[1, 5] = "f(a)";
                    worksheet.Cells[2, 4] = "-";
                    worksheet.Cells[2, 5] = "-";

                    // Запись варьируемого параметра и обобщенных спектров размерностей Реньи в ячейки листа
                    for (int i = 2; i <= height + 1; i++)
                    {
                        worksheet.Cells[i, 1] = variableParameters[i-2];
                        worksheet.Cells[i, 2] = trueRenie[i - 2];
                    }

                    // Запись важных данных отдельно для удобства:
                    worksheet.Cells[25, 9] = "Dq(0)";
                    worksheet.Cells[25, 10] = trueRenie[((trueRenie.Length - 1) / 2)];
                    worksheet.Cells[26, 9] = "Dq(1)";
                    worksheet.Cells[26, 10] = trueRenie[((trueRenie.Length - 1) / 2) + 1];
                    worksheet.Cells[27, 9] = "Dq(2)";
                    worksheet.Cells[27, 10] = trueRenie[((trueRenie.Length - 1) / 2) + 2];
                    worksheet.Cells[28, 9] = $"Dmin q({variableParameters[0] / 2})";
                    worksheet.Cells[28, 10] = trueRenie[0];
                    worksheet.Cells[29, 9] = $"Dmax q({variableParameters[variableParameters.Length - 1] / 2})";
                    worksheet.Cells[29, 10] = trueRenie[trueRenie.Length - 1];

                    // Запись данных спектров
                    for (int i = 3; i <= height+1; i++)
                    {
                        worksheet.Cells[i, 4] = intermediateDifferences[i - 3];
                        worksheet.Cells[i, 5] = spectrums[i - 3];
                    }

                    // Выравнивание заголовков по центру
                    rangeToChange = worksheet.Range["A1:A1"];
                    rangeToChange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    rangeToChange = worksheet.Range["B1:B1"];
                    rangeToChange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    rangeToChange = worksheet.Range["D1:D2"];
                    rangeToChange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    rangeToChange = worksheet.Range["E1:E2"];
                    rangeToChange.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    rangeToChange = worksheet.Range["I25:I29"];
                    rangeToChange.HorizontalAlignment = XlHAlign.xlHAlignCenter;

                    // Установка удобной ширины столбцов B, F, D, E, I, J
                    rangeToChange = worksheet.Range["B:B"];
                    rangeToChange.ColumnWidth = 15;
                    rangeToChange = worksheet.Range["F:F"];
                    rangeToChange.ColumnWidth = 12;
                    rangeToChange = worksheet.Range["D:D"];
                    rangeToChange.ColumnWidth = 15;
                    rangeToChange = worksheet.Range["E:E"];
                    rangeToChange.ColumnWidth = 15;
                    rangeToChange = worksheet.Range["I:I"];
                    rangeToChange.ColumnWidth = 12;
                    rangeToChange = worksheet.Range["J:J"];
                    rangeToChange.ColumnWidth = 15;

                    // Создание графиков с использованием записанных данных
                    var chartObjects = (ChartObjects)worksheet.ChartObjects(Type.Missing);

                    // График обобщенных спектров Реньи
                    var chartObject = chartObjects.Add(350, 10, 500, 300);
                    var chart = chartObject.Chart;

                    // Данные для построения
                    SeriesCollection seriesCollection = (SeriesCollection)chart.SeriesCollection(Type.Missing);
                    Series series = seriesCollection.NewSeries();
                    series.XValues = worksheet.Range[$"A2:A{height + 1}"];
                    series.Values = worksheet.Range[$"B2:B{height + 1}"];

                    // Выбор типа диаграммы и скрытие легенды
                    chart.ChartType = XlChartType.xlXYScatterSmooth;
                    chart.HasLegend = false;

                    // Добавление сетки 
                    var xAxis = (Axis)chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary);
                    var yAxis = (Axis)chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary);
                    chart.Axes(XlAxisType.xlCategory).HasMajorGridlines = true;                 // Для оси X
                    chart.Axes(XlAxisType.xlValue).HasMajorGridlines = true;                    // Для оси Y

                    // График обобщенных спектров Реньи
                    var chartObject1 = chartObjects.Add(900, 10, 350, 300);
                    chart = chartObject1.Chart;

                    // Данные для построения
                    seriesCollection = (SeriesCollection)chart.SeriesCollection(Type.Missing);
                    series = seriesCollection.NewSeries();
                    series.XValues = worksheet.Range[$"D3:D{height+1}"];
                    series.Values = worksheet.Range[$"E3:E{height+1}"];

                    // Выбор типа диаграммы и скрытие легенды
                    chart.ChartType = XlChartType.xlXYScatterSmooth;
                    chart.HasLegend = false;

                    // Добавление сетки 
                    xAxis = (Axis)chart.Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary);
                    yAxis = (Axis)chart.Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary);
                    chart.Axes(XlAxisType.xlCategory).HasMajorGridlines = true;                 // Для оси X
                    chart.Axes(XlAxisType.xlValue).HasMajorGridlines = true;                    // Для оси Y

                    if (k == trueRenieArrays.Count - 1)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                    }
                }

                // Сохранение и закрытие рабочей книги Excel, выход из приложения, высвобождение ресурсов
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static string GetColumnName(int columnNumber)
        {
            StringBuilder columnName = new StringBuilder();

            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                char columnChar = (char)('A' + remainder);
                columnName.Insert(0, columnChar);
                columnNumber = (columnNumber - 1) / 26;
            }

            return columnName.ToString();
        }

    }
}
