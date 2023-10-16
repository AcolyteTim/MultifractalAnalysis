using Microsoft.Win32; // FileDialogs
using OpenCvSharp;
using System.Windows; // Для открытия окон дополнительных
using System; // Окружение, Convert, типы с большой буквы которые
using System.Collections.Generic; // List'ы
using System.IO; // Directory, File
using System.Linq; // array.ToList (в моем случае только для этого)
using System.Windows.Media.Imaging; // BitmapImage
using MultifractalAnalysis.ViewModel.Chart;
using MultifractalAnalysis.View;
using MultifractalAnalysis.Model.Functionality;
using MultifractalAnalysis.ViewModel.Support;
using static MultifractalAnalysis.Model.Functionality.ExcelFileCreation;
using static MultifractalAnalysis.Model.Functionality.ImageProcessing;
using Microsoft.Office.Interop.Excel;

namespace MultifractalAnalysis.ViewModel
{
    public class ImageProcessingWindowVM : BaseViewModel
    {
        public ImageProcessingWindowVM()
        {
            // ViewModels для графиков
            RenieChartVM = new RenieChartVM();
            SpectraChartVM = new SpectraChartVM();

            // Значения по умолчанию
            _uneditedImage = null;
            _editedImage = null;
            _imagesFullNames = new List<string>();
            _imagesShortNames = String.Empty;
            _maxTextBoxSize = 205;

            _renieValues = null;
            _spectraIntermediateDiffs = null;
            _spectra = null;
            _renieOn0 = 0;
            _renieOn1 = 0;
            _renieOn2 = 0;
            _renieOnMin = 0;
            _renieOnMax = 0;
            _timeStamp = "";
            _processingBtnIsEnabled = false;
            _saveBtnIsEnabled= false;
            _settingsVisibility = false;
            _settingsVisibilityBtnText = "Раскрыть параметры";

            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) +
                "\\Мультифрактальный анализ неоднородных структур\\Settings\\Default\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _defaultSettingsFilePath = path + _settingsFilename;

            Settings = new AppSettings();

            //Проверка наличия файла настроек
            if (File.Exists(_defaultSettingsFilePath))
            {
                // Если существует - чтение файла
                try
                {
                    GetSettingsValuesFromFile(_defaultSettingsFilePath);
                }
                catch
                {
                    SetDefaultSettings();
                    SetSettingsValues(Settings);
                    Settings.Save(DefaultSettingsFilePath);
                }
            }
            else
            {
                // Иначе - создание файла со стандартными параметрами
                SetDefaultSettings();
                SetSettingsValues(Settings);
                Settings.Save(DefaultSettingsFilePath);
            }
        }

        protected override void OnPropertyChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnPropertyChanged(propertyName, oldValue, newValue);

            if (propertyName == nameof(ImagesFullNames))
            {
                if (ImagesFullNames != null && ImagesFullNames.Count > 0)
                {
                    ImagesShortNames = String.Empty;
                    for (int i = 0; i < ImagesFullNames.Count; i++)
                    {
                        ImagesShortNames += System.IO.Path.GetFileName(ImagesFullNames[i]);
                        if (i < ImagesFullNames.Count - 1) 
                        { 
                            ImagesShortNames += "\n"; 
                        }
                    }
                    ProcessingBtnIsEnabled = true;
                }
                else
                {
                    ImagesShortNames = "";
                    ProcessingBtnIsEnabled = false;
                }
            }

            if (propertyName == nameof(SettingsVisibility))
            {
                if (_settingsVisibility == true)
                {
                    MaxTextBoxSize = 356;
                }
                else
                {
                    MaxTextBoxSize = 205;
                }
            }
        }

        // Установка стандартных параметров динамических настроек приложения
        private void SetDefaultSettings()
        {
            // Значения по умолчанию
            _standartExcelSavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Мультифрактальный анализ неоднородных структур";
            _thresholdValue = 24;
            _isGoingToBeInverted = false;
            _minSignificantArea = 5;
            _rectsSizes = "2-4-8-16";
            _variableParameter = 40;
        }

        // Переменная для сохранения времени обработки изображения
        private string _timeStamp;

        // Постоянное имя файла настоек
        private const string _settingsFilename = "IMA_settings.xml"; // inhomogeneities multifractal analysis
        // Путь до файла настроек
        private string _defaultSettingsFilePath;
        public string DefaultSettingsFilePath
        { 
            get => _defaultSettingsFilePath;
            set => _defaultSettingsFilePath = value;
        }

        // Класс настроек
        public AppSettings Settings { get; private set; }
        private void GetSettingsValuesFromFile(String path)
        {
            Settings = Settings.Read(path);
            StandartExcelSavePath = Settings.StandartExcelSavePathSetting;
            ThresholdValue = Convert.ToInt32(Settings.ThresholdValueSetting);
            IsGoingToBeInverted = Convert.ToBoolean(Settings.ImageInversionSetting);
            MinSignificantArea = Convert.ToInt32(Settings.MinSignificantAreaSetting);
            RectsSizes = Settings.RectsSizesSetting;                                    // Форматирование тут не забыть
            VariableParameter = Convert.ToInt32(Settings.VariableParameterSetting);
        }
        private void SetSettingsValues(AppSettings appSettings)
        {
            appSettings.StandartExcelSavePathSetting = _standartExcelSavePath;
            appSettings.ThresholdValueSetting = _thresholdValue.ToString();
            appSettings.ImageInversionSetting = _isGoingToBeInverted.ToString();
            appSettings.MinSignificantAreaSetting = _minSignificantArea.ToString();
            appSettings.RectsSizesSetting = RectsSizes;                                  // Форматирование тут не забыть
            appSettings.VariableParameterSetting = _variableParameter.ToString();
        }

        // ViewModels графиков (отдельные user-control)
        private RenieChartVM _renieChartVM;
        public RenieChartVM RenieChartVM 
        {
            get => _renieChartVM;
            set => Set(ref _renieChartVM, value);
        }

        private SpectraChartVM _spectraChartVM;
        public SpectraChartVM SpectraChartVM
        {
            get => _spectraChartVM;
            set => Set(ref _spectraChartVM, value);
        }

        // Изображения
        private BitmapImage _uneditedImage;
        public BitmapImage UneditedImage
        {
            get => _uneditedImage;
            set => Set(ref _uneditedImage, value);
        }

        private BitmapImage _editedImage;
        public BitmapImage EditedImage
        {
            get => _editedImage;
            set => Set(ref _editedImage, value);
        }

        // Изображение (путь/имя) для обработки
        private List<string> _imagesFullNames;
        public List<string> ImagesFullNames
        {
            get => _imagesFullNames;
            set => Set(ref _imagesFullNames, value);
        }

        private string _imagesShortNames;
        public string ImagesShortNames
        {
            get => _imagesShortNames;
            set => Set(ref _imagesShortNames, value);
        }

        // Переменная - максимальный размер полей наименования изображения и пути сохранения отчетов
        private int _maxTextBoxSize;
        public int MaxTextBoxSize
        {
            get => _maxTextBoxSize;
            set => Set(ref _maxTextBoxSize, value);
        }

        // Стандартный путь для сохранения 
        private string _standartExcelSavePath;
        public string StandartExcelSavePath
        {
            get => _standartExcelSavePath;
            set => Set(ref _standartExcelSavePath, value);
        }

        // Пороговое значение для преобразования изображения в черно-белый формат
        private int _thresholdValue;
        public int ThresholdValue
        {
            get => _thresholdValue;
            set
            {
                if (value < 1)
                {
                    Set(ref _thresholdValue, 1); return;
                }
                if (value > 255)
                {
                    Set(ref _thresholdValue, 255); return;
                }

                Set(ref _thresholdValue, value);
            }
        }

        // Параметр для инвертирования ЧБ изображения
        private bool _isGoingToBeInverted;
        public bool IsGoingToBeInverted
        {
            get => _isGoingToBeInverted;
            set => Set(ref _isGoingToBeInverted, value);
        }

        // Минимальная площадь для распознавания неоднородности
        private int _minSignificantArea;
        public int MinSignificantArea
        {
            get => _minSignificantArea;
            set
            {
                if (value < 1)
                {
                    Set(ref _minSignificantArea, 1); return;
                }
                if (value > 25)
                {
                    Set(ref _minSignificantArea, 25); return;
                }

                Set(ref _minSignificantArea, value);
            }
        }

        // Значения мер (размеров ячеек) для расчетов
        private string _rectsSizes;
        public string RectsSizes
        {
            get => _rectsSizes;
            set => Set(ref _rectsSizes ,value);
        }

        // Варьируемый параметр для рассчетов
        private int _variableParameter;
        public int VariableParameter
        {
            get => _variableParameter;
            set
            {
                if (value < 1)
                {
                    Set(ref _variableParameter, 1); return;
                }
                if (value > 100)
                {
                    Set(ref _variableParameter, 100); return;
                }

                Set(ref _variableParameter, value);
            }
        }

        // Массив варьируемых параметров Q данных для анализа
        private int[] _variableParameters;
        public int[] VariableParameters
        {
            get => _variableParameters;
            set => _variableParameters = value;
        }

        // Массивы рассчитанных данных для сохранения
        private double[] _renieValues;
        public double[] RenieValues
        {
            get => _renieValues;
            set => _renieValues = value;
        }

        private double[] _spectraIntermediateDiffs;
        public double[] SpectraIntermediateDiffs
        {
            get => _spectraIntermediateDiffs;
            set => _spectraIntermediateDiffs = value;
        }

        private double[] _spectra;
        public double[] Spectra
        {
            get => _spectra;
            set => _spectra = value;
        }

        // Cписки массивов данных для сохранения, для загрузки в excelFile
        private List<double[]> _renieArrays;
        private List<double[]> _spectraIntermediateDiffsArrays;
        private List<double[]> _spectraArrays;

        // Данные расчета обобщенных спектров размерностей Реньи
        // Значения массива для определенных значений варьируемого параметра
        private double _renieOn0;
        public double RenieOn0
        {
            get => _renieOn0;
            set => Set(ref _renieOn0, value);
        }

        private double _renieOn1;
        public double RenieOn1
        {
            get => _renieOn1;
            set => Set(ref _renieOn1, value);
        }

        private double _renieOn2;
        public double RenieOn2
        {
            get => _renieOn2;
            set => Set(ref _renieOn2, value);
        }

        private double _renieOnMin;
        public double RenieOnMin
        {
            get => _renieOnMin;
            set => Set(ref _renieOnMin, value);
        }

        private double _renieOnMax;
        public double RenieOnMax 
        {
            get => _renieOnMax;
            set => Set(ref _renieOnMax, value);
        }

        // Переменная отвечающая за активность кнопки запуска
        private bool _processingBtnIsEnabled;
        public bool ProcessingBtnIsEnabled
        {
            get => _processingBtnIsEnabled;
            set => Set(ref _processingBtnIsEnabled, value);
        }

        // Переменная отвечающая за активность кнопок сохранения данных
        private bool _saveBtnIsEnabled;
        public bool SaveBtnIsEnabled
        {
            get => _saveBtnIsEnabled;
            set => Set(ref _saveBtnIsEnabled, value);
        }

        // Переменная отвечающая за отображение параметров
        private bool _settingsVisibility;
        public bool SettingsVisibility
        {
            get => _settingsVisibility;
            set => Set(ref _settingsVisibility, value);
        }

        private string _settingsVisibilityBtnText;
        public string SettingsVisibilityBtnText
        {
            get => _settingsVisibilityBtnText;
            set => Set(ref _settingsVisibilityBtnText, value);
        }

        // Команда выбора изображения
        private RelayCommand _pickImage;
        public RelayCommand PickImage
        {
            get
            {
                return _pickImage ?? new RelayCommand(obj =>
                {
                    PickImageFile();
                }
                );
            }
        }
        private void PickImageFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файл изображения (*.jpg, *.jpeg, *.png, *.bmp, *.tif) | *.jpg; *.jpeg; *.png; *.bmp; *.tif";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            ImagesFullNames = openFileDialog.FileNames.ToList();
        }

        // Команда обработки изображения и расчетов
        private RelayCommand _runImageAnalysis;
        public RelayCommand RunImageAnalysis
        {
            get
            {
                return _runImageAnalysis ?? new RelayCommand(obj =>
                {
                    RunAnalysis();
                }
                );
            }
        }
        // Запуск цикла обработки
        private void RunAnalysis()
        {
            _renieArrays = new List<double[]>();
            _spectraIntermediateDiffsArrays = new List<double[]>();
            _spectraArrays = new List<double[]>();

            bool isFirst = true;
            for (int i = 0; i < ImagesFullNames.Count; i++)
            {
                ImageProcessing(i, isFirst);
                if (i == 0)
                {
                    // Отображение данных первого изображения
                    DisplayData();
                    isFirst = false;
                }

                // Наполнение данными для создания Excel-файла
                _renieArrays.Add(_renieValues);
                _spectraIntermediateDiffsArrays.Add(_spectraIntermediateDiffs);
                _spectraArrays.Add(_spectra);
            }

            // Штамп времени обработки (по окончанию обработки)
            _timeStamp = GetCurrentTime();
        }

        // Анализ одного изображения
        private void ImageProcessing(int index, bool isFirst)
        {
            if (String.IsNullOrWhiteSpace(ImagesFullNames[index])) { return; }

            // Создание матрицы изображения
            Mat image = Cv2.ImRead(ImagesFullNames[index]);

            // Получение данных о внешних контурах
            List<List<OpenCvSharp.Point>> contours = GetOuterContours(image, ThresholdValue, minContourArea: MinSignificantArea);

            // Создание матричного изображения для статистических расчетов
            Mat imageNew = new Mat(image.Height, image.Width, MatType.CV_8UC3, new Scalar(255, 255, 255));
            Cv2.FillPoly(imageNew, contours, new Scalar(0, 0, 0));

            if (IsGoingToBeInverted == true)
            {
                imageNew = ~imageNew;
            }

            int[] rectSizesInt = ConvertRectSizesToArray(RectsSizes);

            // Статистические расчеты
            _variableParameters = GetRightVariableParameters(VariableParameter);
            double[,] statSumArray = RectsStatisticSumInRange(imageNew, rectSizesInt, _variableParameters);
            double[] tauArray = GetTauArray(statSumArray, rectSizesInt);
            _renieValues = GetRenie(tauArray, _variableParameters);
            SpectrumData spectraData = GetSpectraData(_renieValues, _variableParameters);
            _spectraIntermediateDiffs = spectraData.IntermediateDifferences;
            _spectra = spectraData.Spectrums;

            DoPrecisionOfDoubleArray(ref _renieValues, 3);
            DoPrecisionOfDoubleArray(ref _spectraIntermediateDiffs, 3);
            DoPrecisionOfDoubleArray(ref _spectra, 3);


            // Отображение вариантов изображения до и после обработки (для первого изображения)
            if (isFirst == true)
            {
                UneditedImage = MatToBitmapImage(image);
                EditedImage = MatToBitmapImage(imageNew);
            }

            // Освобождение ресурсов
            image.Dispose();
            imageNew.Dispose();

        }

        // Отображения данных по текущему изображению
        private void DisplayData()
        {
            // Отображение основных значений обобщенных спектров размерностей Реньи
            RenieOn0 = _renieValues[(_renieValues.Length - 1) / 2];
            RenieOn1 = _renieValues[((_renieValues.Length - 1) / 2) + 1];
            RenieOn2 = _renieValues[((_renieValues.Length - 1) / 2) + 2];
            RenieOnMin = _renieValues[0];
            RenieOnMax = _renieValues[_renieValues.Length - 1];

            // Передача значений для графиков
            RenieChartVM.SetDataForChart(_variableParameters, _renieValues);
            SpectraChartVM.SetDataForChart(_spectraIntermediateDiffs, _spectra);

            // Активация кнопок сохранения отчета
            SaveBtnIsEnabled = true;
        }

        // Команда сохранения данных в excel-файле
        private RelayCommand _saveToExcelFileStandartWay;
        public RelayCommand SaveToExcelFileStandartWay
        {
            get
            {
                return _saveToExcelFileStandartWay ?? new RelayCommand(obj =>
                {
                    SaveToExcelStandartWay();
                }
                );
            }
        }
        private void SaveToExcelStandartWay()
        {
            if (_renieValues == null || _spectraIntermediateDiffs == null || _spectra == null) { return; }

            if (!Directory.Exists(_standartExcelSavePath))
            {
                Directory.CreateDirectory(_standartExcelSavePath);
            }

            string path = _standartExcelSavePath + "\\" + _timeStamp;

            AppSettings settings = new AppSettings();
            SetSettingsValues(settings);

            CreateExcelFileWithExactWay(_variableParameters, _renieArrays, _spectraIntermediateDiffsArrays, _spectraArrays, settings, path);
        }

        private RelayCommand _saveToExcelFilePickWay;
        public RelayCommand SaveToExcelFilePickWay
        {
            get
            {
                return _saveToExcelFilePickWay ?? new RelayCommand(obj =>
                {
                    SaveToExcelPickWay();
                }
                );
            }
        }
        private void SaveToExcelPickWay()
        {
            if (_renieArrays.Count == 0 || _spectraIntermediateDiffsArrays.Count == 0 || _spectraArrays.Count == 0) { return; }

            string fileName = _timeStamp;

            AppSettings settings = new AppSettings();
            SetSettingsValues(settings);

            CreateExcelFile(_variableParameters, _renieArrays, _spectraIntermediateDiffsArrays, _spectraArrays, settings, fileName);
        }

        // Команда изменения директории для сохранения файлов по умолчанию
        private RelayCommand _changeExcelFileStandartWay;
        public RelayCommand ChangeExcelFileStandartWay
        {
            get
            {
                return _changeExcelFileStandartWay ?? new RelayCommand(obj =>
                {
                    PickExcelFileStandartWay();
                }
                );
            }
        }
        private void PickExcelFileStandartWay()
        {
            var dialog = new SaveFileDialog();
            dialog.InitialDirectory = _standartExcelSavePath;
            dialog.RestoreDirectory = true;
            dialog.Title = "Выберите путь для сохранения";
            dialog.Filter = "Путь|*.this.directory";        // Для избежания отображения файлов
            dialog.FileName = "Select";                     // "select.this.directory"
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                // Удаление "ненастоящей" части пути
                path = path.Replace("\\Select.this.directory", "");
                path = path.Replace(".this.directory", "");
                // Если пользователь изменил имя файла, создание новой директории
                if (Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                StandartExcelSavePath = path;
            }
        }

        // Команда открытия/скрытия параметров
        private RelayCommand _changeSettingsVisibility;
        public RelayCommand ChangeSettingsVisibility
        {
            get
            {
                return _changeSettingsVisibility ?? new RelayCommand(obj =>
                {
                    ReverseSettingsVisibility();
                }
                );
            }
        }
        private void ReverseSettingsVisibility()
        {
            SettingsVisibility = !SettingsVisibility;
            if( _settingsVisibility == true)
            {
                SettingsVisibilityBtnText = "Скрыть параметры";
            }
            if (_settingsVisibility == false)
            {
                SettingsVisibilityBtnText = "Раскрыть параметры";
            }
        }

        // Команда сохранения параметров приложения
        private RelayCommand _saveApplicationSettings;
        public RelayCommand SaveApplicationSettings
        {
            get
            {
                return _saveApplicationSettings ?? new RelayCommand(obj =>
                {
                    SaveAppSettings();
                }
                );
            }
        }
        private void SaveAppSettings()
        {
            SetSettingsValues(Settings);
            Settings.Save(_defaultSettingsFilePath);
        }

        // Команда загрузки параметров из файла
        private RelayCommand _loadApplicationSettingsFromFile;
        public RelayCommand LoadApplicationSettingsFromFile
        {
            get
            {
                return _loadApplicationSettingsFromFile ?? new RelayCommand(obj =>
                {
                    LoadAppSettingsFromFile();
                }
                );
            }
        }
        private void LoadAppSettingsFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Файл настроек (*.xml) | *.xml";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != true)
            {
                ImagesFullNames = null;
                return;
            }

            GetSettingsValuesFromFile(openFileDialog.FileName);
        }

        // Команда сохранения параметров в файл
        private RelayCommand _saveApplicationSettingsToFile;
        public RelayCommand SaveApplicationSettingsToFile
        {
            get
            {
                return _saveApplicationSettingsToFile ?? new RelayCommand(obj =>
                {
                    SaveAppSettingsToFile();
                }
                );
            }
        }
        private void SaveAppSettingsToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Файлы настроек (*.xml)|*.xml|Все файлы (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = _settingsFilename;

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            SetSettingsValues(Settings);
            Settings.Save(saveFileDialog.FileName);   
        }

        // Вспомогательные методы //
        private string GetCurrentTime()
        {
            return DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
        }

        private int[] ConvertRectSizesToArray(string str)
        {
            try
            {
                return Array.ConvertAll(str.Split(new char[] { ' ', ',', '-', ';', '_', ':', '.' }, StringSplitOptions.RemoveEmptyEntries), x => int.Parse(x));
            }
            catch
            {
                RectsSizes = "Ошибка конвертации: исп. 2-4-8-16";
                return new int[] { 2, 4, 8, 16 };
            }
        }

        private double[] DoPrecisionOfDoubleArray(ref double[] x, int precision) 
        {
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = Math.Round(x[i], precision);
            }

            return x;
        }

        // Метод открытия окон
        private void SetWindowPostionAndOpen(System.Windows.Window window)
        {
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }

    }
}
