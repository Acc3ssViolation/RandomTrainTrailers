using RandomTrainTrailers.Definition;
using System;
using System.Collections.Generic;

namespace RandomTrainTrailers.UI
{
    internal class FilterableFastList<T>
    {
        public UIFastList UIList { get => _uiList; }

        public List<RowData<T>> Data
        {
            get => _data;
            set
            {
                if (_data != value && value != null)
                {
                    _data = value;
                    ApplyFilter();
                }
            }
        }

        private UIFastList _uiList;
        private List<RowData<T>> _data;
        private Predicate<T> _filter;

        public FilterableFastList(UIFastList uiList)
        {
            _uiList = uiList ?? throw new ArgumentNullException(nameof(uiList));
            Data = new List<RowData<T>>();
        }

        public void SetFilter(Predicate<T> predicate)
        {
            _filter = predicate;
            ApplyFilter();
        }

        public void Refresh()
        {
            ApplyFilter();
        }

        public void ApplyFilter()
        {
            if (_filter == null)
            {
                _uiList.rowsData.Clear();
                foreach (var data in Data)
                    _uiList.rowsData.Add(data);
            }
            else
            {
                _uiList.rowsData.Clear();
                foreach (var data in Data)
                    if (_filter(data.Value))
                        _uiList.rowsData.Add(data);
            }

            _uiList.Refresh();
        }

        public void Add(RowData<T> item, bool updateImmediatly = true)
        {
            Data.Add(item);
            if (updateImmediatly)
                ApplyFilter();
        }

        public void Remove(RowData<T> item, bool updateImmediatly = true)
        {
            Data.Remove(item);
            if (updateImmediatly)
                ApplyFilter();
        }

        public List<RowData<T>> GetSelectedRows()
        {
            var result = new List<RowData<T>>();

            foreach (var row in UIList.rowsData)
            {
                var rowData = (RowData<T>)row;
                if (rowData.Selected)
                    result.Add(rowData);
            }

            return result;
        }

        public void SelectAll()
        {
            var allSelected = true;

            foreach (var row in UIList.rowsData)
            {
                var rowData = (RowData)row;
                if (!rowData.Selected)
                {
                    allSelected = false;
                    break;
                }
            }

            foreach (var row in UIList.rowsData)
            {
                var rowData = (RowData)row;
                rowData.Selected = !allSelected;
            }

            UIList.Refresh();
        }

    }
}
