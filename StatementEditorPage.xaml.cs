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
    public partial class StatementEditorPage : ContentPage
    {
        private StatementEditorViewModel ViewModel { get; set; }
        public StatementEditorPage(StatementEditorViewModel vm)
        {
            InitializeComponent();

            vm.StatementEditorPage = this;
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

        [Obsolete]
        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var item = e.Item as StudentItem;

            if (item.Enabled)
                ViewModel.UpdateItem(e.ItemIndex, ViewModel.TeacherId);
        }
    }
}
