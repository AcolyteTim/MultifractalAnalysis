using LiveCharts.Defaults;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultifractalAnalysis.ViewModel.Support;

namespace MultifractalAnalysis.ViewModel.Chart
{
    public class SpectraChartVM : BaseViewModel
    {
        public SpectraChartVM()
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

        public void SetDataForChart(double[] spectraIntermediateDiffs, double[] spectra)
        {
            Points.Clear();

            ObservablePoint newPoint;
            for (int i = 0; i < spectra.Length; i++)
            {
                newPoint = new ObservablePoint(spectraIntermediateDiffs[i], spectra[i]);
                Points.Add(newPoint);
            }
        }
    }
}
