using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConiTradeCs.GUI
{
    public class MainViewModel
    {
        ObservableCollection<string> OrderList { get; set; }
        public MainViewModel()
        {
            OrderList = new ObservableCollection<string>();
        }

         
    }
}
