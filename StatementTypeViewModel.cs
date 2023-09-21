using DvgupsMobile.Models;
using DvgupsMobile.ObservableItems;
using DvgupsMobile.Services.StatementHandlers;
using DvgupsMobile.Views.Jornal.ElectronStatement;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement
{
    public class StatementTypeViewModel : StatementViewModel
    {
        public ICommand Reload { get; set; }
        public ObservableCollection<TypeItem> AllTypeItems { get; set; }
        public ObservableCollection<TypeItem> SelectedTypeItems { get; set; }
        public tbdiscipcourse Journal { get; set; }
        public string Speciality { get; set; }
        public long EducgroupId { get; set; }
        private string text_entered { get; set; }


        public StatementTypeViewModel(tbdiscipcourse journal)
        {
            Animation(Mode.Loading);

            this.Reload = new Command(() => Refresh());

            this.AllTypeItems = new ObservableCollection<TypeItem>();

            this.SelectedTypeItems = new ObservableCollection<TypeItem>();

            this.TextEntered = string.Empty;

            this.Journal = journal;

            Init();
        }

        public StatementTypePage TypePage { get; set; }

        public StatementTypeViewModel()
        {

        }
        private async void Init()
        {
            var educgroup_id = this.Journal.educgroup_id_global;

            var prepared_asuid = await OtherHandler.GetEducgroupAsuId(educgroup_id);

            if (prepared_asuid == null)
            {
                Animation(Mode.Failed);
                return;
            }

            var discipline_id = this.Journal.discipline_id_global;

            var educgroup_preparedid = (long)prepared_asuid;

            var count = await OtherHandler.GetRunnersCount(discipline_id, educgroup_preparedid, 1);

            var text = TextGenerator(count);

            var collection = new ObservableCollection<TypeItem>()
            {
                new TypeItem
                {
                    Id = 1,
                    Name = "Аттестационный лист",
                    Describtion = "Листы по каждому студенту",
                    IsBegunok = true,
                    Text = text
                },

                new TypeItem
                {
                    Id = 2,
                    Name = "Аттестационная ведомость",
                    Describtion = "Документ для текущей сессии",
                    IsBegunok = false,
                    Text = string.Empty
                },

                new TypeItem
                {
                    Id = 3,
                    Name = "Ведомость по группе",
                    Describtion = "Ведомость по всей группе",
                    IsBegunok = false,
                    Text = string.Empty
                }
            };

            foreach (var item in collection)
            {
                this.AllTypeItems.Add(item);
                this.SelectedTypeItems.Add(item);
            }

            Animation(Mode.Loaded);
        }

        private void Refresh()
        {
            Animation(Mode.Loading);

            Init();
        }

        private ObservableCollection<TypeItem> SelectItems(ObservableCollection<TypeItem> collection, string pattern)
        {
            var items = new ObservableCollection<TypeItem>();

            var upper = pattern.ToUpper();

            foreach (var item in collection)
            {
                var name = item.Name.ToUpper();

                if (name.Contains(upper))
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private string TextGenerator(int count)
        {
            var string_value = count.ToString();

            var last_digit = string_value.Last();

            return count == 0
                ? "Нет бегунков"
                : last_digit > 1 && last_digit < 5 ? count + "бегунка" : last_digit == 1 ? count + "бегунок" : count + "бегунков";
        }

        public void ExecuteSearch()
        {
            if (this.AllTypeItems == null)
            {
                return;
            }

            var list = SelectItems(this.AllTypeItems, this.TextEntered);

            this.SelectedTypeItems.Clear();

            foreach (var item in list)
            {
                this.SelectedTypeItems.Add(item);
            }
        }

        public string TextEntered
        {
            get => this.text_entered;
            set
            {
                if (this.text_entered != value)
                {
                    this.text_entered = value;
                    OnPropertyChanged(nameof(this.TextEntered));
                }
            }

        }
    }
}
