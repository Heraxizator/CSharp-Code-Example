using DvgupsMobile.ObservableItems;
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
    public partial class StatementItemsPage : ContentPage
    {
        private StatementItemViewModel ViewModel { get; set; }
        public StatementItemsPage(StatementItemViewModel vm)
        {
            InitializeComponent();

            vm.StatementItemsPage = this;
            ViewModel = vm;
            BindingContext = ViewModel;
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var journal = ViewModel.Journal;

            var statement = e.Item as StatementItem;

            await Navigation.PushAsync(new StatementListPage(new StatementListViewModel(journal, statement)));
        }
    }
}
