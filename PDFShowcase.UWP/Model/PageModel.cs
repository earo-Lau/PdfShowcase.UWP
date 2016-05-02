using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFShowcase.UWP.Model
{
    public class PageModel : INotifyPropertyChanged
    {
        #region Members
        private int _currentPage = 0;
        private int _pageSize = 0;
        private bool _isShow = false;
        #endregion

        #region Properties

        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                this._currentPage = value;
                this.RaisePropertyChanged("CurrentPage");
            }
        }
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                this._pageSize = value;
                this.RaisePropertyChanged("PageSize");
            }
        }
        public bool IsShow
        {
            get { return this._isShow; }
            set
            {
                this._isShow = value;
                this.RaisePropertyChanged("IsShow");
            }
        }
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null && propertyName != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }


}
