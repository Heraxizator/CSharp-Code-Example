using DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DvgupsMobile.Views.Jornal.ElectronStatement
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatementSTNPage : ContentPage
    {
        StatementSTNViewModel ViewModel { get; set; }
        public StatementSTNPage(StatementSTNViewModel vm)
        {
            vm.StatementSTNPage = this;
            ViewModel = vm;
            BindingContext = ViewModel;

            InitializeComponent();
        }
    }
}
