using System;

namespace RandomTrainTrailers.UI
{
    internal class RowData<T>
    {
        public bool Selected { get; set; }
        public T Value { get; set; }

        public Action<RowData<T>> Delete { get; set; }

        public RowData(T value, Action<RowData<T>> delete)
        {
            Value = value;
            Delete = delete ?? throw new ArgumentNullException(nameof(delete));
        }
    }
}
