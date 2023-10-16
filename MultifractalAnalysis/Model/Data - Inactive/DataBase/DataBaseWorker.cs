using MultifractalAnalysis.Model.Functionality;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace MultifractalAnalysis.Model.Data.DataBase
{
    public class DataBaseWorker
    {
        public DataBaseWorker(string uid, string password)
        {
            _dbCon = new DBConnection(uid, password);
        }

        private DBConnection _dbCon;

        public bool TestConnection()
        {
            try
            {
                _dbCon.OpenConnetion();
                _dbCon.CloseConnetion();
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public List<SessionInfo> LoadSessionDataFromDataBase()
        {
            try
            {
                _dbCon.OpenConnetion();

                string query = @"SELECT pd.pd_id,  pd.pd_time, ps.ps_thresholdValue, ps.ps_imageInversion, ps.ps_minSignificantArea, ps.ps_minRectSize,
                                    ps.ps_maxRectSize, ps.ps_variableParameter, ui.ui_image, ei.ei_image FROM ProcessingData pd 
                                    INNER JOIN ProcessingSettings ps ON ps.ps_id = pd.pd_id
                                    INNER JOIN UneditedImage ui ON ui.ui_id = pd.pd_id
                                    INNER JOIN EditedImage ei ON ei.ei_id = pd.pd_id";

                var cmd = new MySqlCommand(query, _dbCon.GetConnection());

                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                if (dataTable.Rows.Count > 0)
                {
                    List<SessionInfo> sessionInfoList = new List<SessionInfo>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        int ID = Convert.ToInt32(row["pd_id"]);
                        string time = Convert.ToDateTime(row["pd_time"]).ToString("dd-MM-yyyy_HH-mm-ss");
                        int threshold = Convert.ToInt32(row["ps_thresholdValue"]);
                        bool imageInversion = Convert.ToBoolean(row["ps_imageInversion"]);
                        int minSignificantArea = Convert.ToInt32(row["ps_minSignificantArea"]);
                        int minRectSize = Convert.ToInt32(row["ps_minRectSize"]);
                        int maxRectSize = Convert.ToInt32(row["ps_maxRectSize"]);
                        int variableParameter = Convert.ToInt32(row["ps_variableParameter"]);
                        byte[] uneditedImage = (byte[])row["ui_image"];
                        byte[] editedImage = (byte[])row["ei_image"];

                        var session = new SessionInfo(ID,time, threshold, imageInversion, minSignificantArea, minRectSize, maxRectSize, variableParameter, uneditedImage, editedImage);
                        sessionInfoList.Add(session);
                    }
                      
                    _dbCon.CloseConnetion();

                    return sessionInfoList;
                }
                else
                {
                    _dbCon.CloseConnetion();
                    return null;
                }
               
            }
            catch (Exception ex) 
            {
                _dbCon.CloseConnetion();
                return null; 
            }
        }

        public void InsertUneditedImage(BitmapImage img)
        {
            try
            {
                if (img == null) { return; }

                _dbCon.OpenConnetion();

                byte[] data;
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }

                using (var cmd = new MySqlCommand("INSERT INTO `UneditedImage` SET ui_image = @image", _dbCon.GetConnection()))
                {
                    cmd.Parameters.Add("@image", MySqlDbType.LongBlob).Value = data;
                    cmd.ExecuteNonQuery();
                }

                _dbCon.CloseConnetion();
            }
            catch(Exception ex) 
            {
                _dbCon.CloseConnetion();
            }
        }

        public void InsertEditedImage(BitmapImage img)
        {
            if(img == null) { return; }
            try
            {
                _dbCon.OpenConnetion();

                byte[] data;
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }

                using (var cmd = new MySqlCommand("INSERT INTO `EditedImage` SET ei_image = @image", _dbCon.GetConnection()))
                {
                    cmd.Parameters.Add("@image", MySqlDbType.LongBlob).Value = data;
                    cmd.ExecuteNonQuery();
                }

                _dbCon.CloseConnetion();
            }
            catch(Exception ex) { _dbCon.CloseConnetion(); }
        }

        public void InsertSettings(AppSettings appSettings)
        {
            if (appSettings == null) { return; }
            try
            {
                _dbCon.OpenConnetion();

                using (var cmd = new MySqlCommand("INSERT INTO ProcessingSettings (ps_thresholdValue," +
                    " ps_imageInversion, ps_minSignificantArea, ps_minRectSize, ps_maxRectSize, ps_variableParameter) " +
                    "VALUES (@thresholdValue, @imageInversion, @minSignificantArea," +
                    " @minRectSize, @maxRectSize, @variableParameter)", _dbCon.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@thresholdValue", Convert.ToInt16(appSettings.ThresholdValueSetting));
                    cmd.Parameters.AddWithValue("@imageInversion", Convert.ToBoolean(appSettings.ImageInversionSetting));
                    cmd.Parameters.AddWithValue("@minSignificantArea", Convert.ToByte(appSettings.MinSignificantAreaSetting));
                    cmd.Parameters.AddWithValue("@minRectSize", Convert.ToByte(appSettings.MinRectSizeSetting));
                    cmd.Parameters.AddWithValue("@maxRectSize", Convert.ToByte(appSettings.MaxRectSizeSetting));
                    cmd.Parameters.AddWithValue("@variableParameter", Convert.ToByte(appSettings.VariableParameterSetting));
                    cmd.ExecuteNonQuery();
                }

                _dbCon.CloseConnetion();
            }
            catch(Exception ex) { _dbCon.CloseConnetion(); }
        }

        public void InsertMainData(string time, string serializedArrays)
        {
            if (String.IsNullOrEmpty(serializedArrays)) { return; }
            try
            {
                _dbCon.OpenConnetion();

                using (var cmd = new MySqlCommand("INSERT INTO ProcessingData (pd_time, ps_id, ui_id, ei_id, pd_arrays)" +
                    " VALUES (@time, LAST_INSERT_ID()+1, LAST_INSERT_ID()+1, LAST_INSERT_ID()+1, @arrays)", _dbCon.GetConnection()))
                {
                    // Параметры запроса         
                    cmd.Parameters.AddWithValue("@time", DateTime.ParseExact(time, "dd-MM-yyyy_HH-mm-ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@arrays", serializedArrays);

                    // Выполняем запрос
                    cmd.ExecuteNonQuery();
                }

                _dbCon.CloseConnetion();
            }
            catch (Exception ex) { _dbCon.CloseConnetion(); }
        }
    }
}
