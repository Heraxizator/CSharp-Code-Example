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
    public partial class StatementTypePage : ContentPage
    {
        public StatementTypeViewModel ViewModel { get; set; }
        public StatementTypePage(StatementTypeViewModel vm)
        {
            InitializeComponent();

            vm.TypePage = this;
            ViewModel = vm;
            BindingContext = ViewModel;

        }

        private void BorderlessEntryControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ExecuteSearch();
            }
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var journal = ViewModel.Journal;

            var type_id = e.ItemIndex;

            await Navigation.PushAsync(new HoursCountPage(new HoursCountViewModel(journal, type_id)));
        }
    }
}
