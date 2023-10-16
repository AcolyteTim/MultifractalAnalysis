using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultifractalAnalysis.Model.Data
{
    public class SessionInfo
    {
        private int _id;
        public int ID
        { 
            get => _id;
            set => _id = value;
        }

        private string _time;
        public string Time
        { 
            get => _time;
            set => _time = value;
        }

        private int _threshold;
        public int Threshold
        {
            get => _threshold;
            set => _threshold = value;
        }

        private bool _imageInversion;
        public bool ImageInversion
        {
            get => _imageInversion;
            set => _imageInversion = value;
        }

        private int _minSignificantArea;
        public int MinSignificantArea
        {
            get => _minSignificantArea;
            set => _minSignificantArea = value;
        }

        private int _minRectSize;
        public int MinRectSize
        {
            get => _minRectSize;
            set => _minRectSize = value;
        }

        private int _maxRectSize;
        public int MaxRectSize
        {
            get => _maxRectSize;
            set => _maxRectSize = value;
        }

        private int _variableParameter;
        public int VariableParameter
        {
            get => _variableParameter;
            set => _variableParameter = value;
        }

        private byte[] _uneditedImage;
        public byte[] UneditedImage
        {
            get => _uneditedImage;
            set => _uneditedImage = value;
        }

        private byte[] _editedImage;
        public byte[] EditedImage
        {
            get => _editedImage;
            set => _editedImage = value;
        }


        public SessionInfo(int id, string time, int threshold, bool imgInversion, int minSignArea, int minRectSize, int maxRectSize, int varParam, byte[] uneditedImg, byte[] editedImg) 
        {
            ID = id;
            Time = time;
            Threshold = threshold;
            ImageInversion = imgInversion;
            MinSignificantArea = minSignArea;
            MinRectSize = minRectSize;
            MaxRectSize = maxRectSize;
            VariableParameter = varParam;
            UneditedImage = uneditedImg;
            EditedImage = editedImg;
        }
    }
}
