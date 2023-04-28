using System;

namespace RandomTrainTrailers.UI
{
    internal abstract class RowData
    {
        public bool Selected { get; set; }
    }

    internal class RowData<T> : RowData
    {
        public T Value { get; set; }

        public Action<RowData<T>> Delete { get; set; }

        public RowData(T value, Action<RowData<T>> delete)
        {
            Value = value;
            Delete = delete;
        }
    }
}
