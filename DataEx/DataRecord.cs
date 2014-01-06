using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataEx
{
    public class DataRecord : IDataRecord
    {
        private Dictionary<int, object> _items;
        private List<string> _names;
        private IDataRecord _dataRecord;

        public DataRecord(IDataRecord record)
        {
            InitializeClass();
            LoadFromRecord(record);
        }

        public DataRecord(IDictionary<string, object> values)
        {
            InitializeClass();
            LoadFromDictionary(values);
        }

        private void InitializeClass()
        {
            _items = new Dictionary<int, object>();
            _names = new List<string>();
        }

        private void LoadFromRecord(IDataRecord record)
        {
            _dataRecord = record;
            for (var i = 0; i < record.FieldCount; i++)
            {
                _items.Add(i, record.GetValue(i));
                _names.Add(record.GetName(i));
            }
        }

        private void LoadFromDictionary(IDictionary<string, object> values)
        {
            _names.AddRange(values.Keys);
            var valueList = values.Values.ToList();
            for (int i = 0; i < values.Count; i++)
            {
                _items.Add(i, valueList[i]);
            }
        }

        public string GetName(int i)
        {
            return _names[i];
        }

        public string GetDataTypeName(int i)
        {
            return _dataRecord == null ? string.Empty : _dataRecord.GetDataTypeName(i);
        }

        public Type GetFieldType(int i)
        {
            return GetValue(i).GetType();
        }

        public object GetValue(int i)
        {
            return _items[i];
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            return _names.IndexOf(name);
        }

        public bool GetBoolean(int i)
        {
            return ((bool) _items[i]);
        }

        public byte GetByte(int i)
        {
            return ((byte)_items[i]);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return ((char)_items[i]);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            return ((Guid)_items[i]);
        }

        public short GetInt16(int i)
        {
            return ((short)_items[i]);
        }

        public int GetInt32(int i)
        {
            return ((int)_items[i]);
        }

        public long GetInt64(int i)
        {
            return ((long)_items[i]);
        }

        public float GetFloat(int i)
        {
            return ((float)_items[i]);
        }

        public double GetDouble(int i)
        {
            return ((double)_items[i]);
        }

        public string GetString(int i)
        {
            return ((string)_items[i]);
        }

        public decimal GetDecimal(int i)
        {
            return ((decimal)_items[i]);
        }

        public DateTime GetDateTime(int i)
        {
            return ((DateTime)_items[i]);
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return DBNull.Value.Equals(GetValue(i));
        }

        public int FieldCount
        {
            get { return _names.Count; }
        }

        object IDataRecord.this[int i]
        {
            get { return _items[i]; }
        }

        object IDataRecord.this[string name]
        {
            get { return _items[GetOrdinal(name)]; }
        }
    }
}
