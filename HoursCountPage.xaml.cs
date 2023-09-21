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
    public partial class HoursCountPage : ContentPage
    {
        private HoursCountViewModel ViewModel { get; set; }
        public HoursCountPage(HoursCountViewModel vm)
        {
            InitializeComponent();

            vm.HoursCountPage = this;
            ViewModel = vm;
            BindingContext = ViewModel;
        }

        [Obsolete]
        private async void Button_Clicked_1(object sender, EventArgs e)
        {
            var type_id = ViewModel.TypeId;

            var journal = ViewModel.Journal;

            var educgroup_id = ViewModel.EducgroupAsuId;

            var discipline_id = ViewModel.DisciplineId;

            var edit_enabled = ViewModel.EditEnabled;

            await Navigation.PushAsync(new StatementEditorPage(new StatementEditorViewModel(journal, educgroup_id, discipline_id, type_id, edit_enabled)));
            
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Scrollview.ScrollToAsync(Btn, ScrollToPosition.MakeVisible, true);
        }
    }
}
