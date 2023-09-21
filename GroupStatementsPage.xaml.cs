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
    public partial class GroupStatementsPage : ContentPage
    {
        private GroupStatementsViewModel ViewModel { get; set; }

        public GroupStatementsPage(GroupStatementsViewModel vm)
        {
            InitializeComponent();

            vm.GroupStatementsPage = this;
            ViewModel = vm;
            BindingContext = ViewModel;
        }

        [Obsolete]
        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var type_id = ViewModel.TypeId;

            var journal = ViewModel.Journal;

            var discipline_id = ViewModel.DisciplineId;

            var educgroup_id = ViewModel.Educgroup_id;

            var edit_enabled = ViewModel.EditEnabled;

            await Navigation.PushAsync(new StatementEditorPage(new StatementEditorViewModel(journal, educgroup_id, discipline_id, type_id, edit_enabled)));
        }
    }
}
