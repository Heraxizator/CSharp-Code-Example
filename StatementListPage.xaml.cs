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
    public partial class StatementListPage : ContentPage
    {
        private StatementListViewModel ViewModel { get; set; }

        public StatementListPage(StatementListViewModel vm)
        {
            InitializeComponent();

            vm.StatementListPage = this;
            ViewModel = vm;
            BindingContext = ViewModel;
        }
    }
}
