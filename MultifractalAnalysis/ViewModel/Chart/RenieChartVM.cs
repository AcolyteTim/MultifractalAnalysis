using LiveCharts;
using LiveCharts.Defaults;
using MultifractalAnalysis.View.Chart;
using MultifractalAnalysis.ViewModel.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultifractalAnalysis.ViewModel.Chart
{
    public class RenieChartVM : BaseViewModel
    {
        public RenieChartVM()
        {
            _points = new ChartValues<ObservablePoint>();
        }

        // Значение точек
        private ChartValues<ObservablePoint> _points;
        public ChartValues<ObservablePoint> Points 
        { 
            get => _points;
            set => Set(ref _points, value);
        }
        
        // "Заголовки" оси OX
        private string[] _labels;
        public string[] Labels
        {
            get => _labels;
            set => Set(ref _labels, value);
        }

        public void SetDataForChart(int[] variableParameters, double[] trueRenie)
        {
            Points.Clear();
            Labels = new string[variableParameters.Length];
            ObservablePoint newPoint;

            for (int i = 0; i < trueRenie.Length; i++)
            {
                newPoint = new ObservablePoint(i, trueRenie[i]);
                Points.Add(newPoint);

                Labels[i] = variableParameters[i].ToString();
            }
        }
    }
}
