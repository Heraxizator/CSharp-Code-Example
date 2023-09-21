using DvgupsMobile.DependencyServices;
using DvgupsMobile.ObservableItems;
using DvgupsMobile.Services;
using DvgupsMobile.Services.StatementHandlers;
using DvgupsMobile.Views.Jornal.ElectronStatement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement
{
    public class StatementSTNViewModel : StatementGroupViewModel
    {
        private readonly string title_part = "Аттестационная ведомость #";
        public new ICommand UpdateData { get; set; }
        public new ICommand LoadData { get; set; }
        private int point_id { get; set; }
        private int studyyear_number { get; set; }
        private string semestr_number { get; set; }
        private string form_string { get; set; }

        [Obsolete]
        public StatementSTNViewModel(int typespoint_id, long session_id, long discipline_id, long educgroup_id)
        {
            this.TitleString = this.title_part + session_id;

            Animation(Mode.Loading);

            this.LoadData = new Command(ToLoadData);

            this.UpdateData = new Command(ToUpdateData);

            this.MarkItems = new List<MarkItem>();

            this.MarkStrings = new ObservableCollection<string>();

            this.point_id = typespoint_id;

            this.SessionId = session_id;

            this.DisciplineId = discipline_id;

            this.EducgroupId = educgroup_id;

            Init();
        }
        public StatementSTNPage StatementSTNPage { get; set; }

        public StatementSTNViewModel()
        {

        }

        [Obsolete]
        private async void Init()
        {
            var type_id = this.point_id != 1 ? 1 : 4;

            var mark_bags = await MarkHandler.GetMarks(x => x == type_id);

            foreach (var mark_item in mark_bags)
            {
                this.MarkItems.Add(mark_item);
            }

            var item = await StatementHandler.GetStatementData(mark_bags, this.SessionId, this.DisciplineId, this.EducgroupId);

            if (item == null)
            {
                Animation(Mode.Failed);
                return;
            }

            foreach (var mark in mark_bags)
            {
                this.MarkStrings.Add(mark.LongName);
            }

            var types = await TypeAccountingHandler.GetTypes();

            var type_item = types.FirstOrDefault(x => x.Id == this.point_id);

            var type_string = type_item != null ? type_item.FullName : string.Empty;

            this.CertificationId = item.CertificationId;

            this.TitleString = this.title_part + item.CertificationId;

            this.BookNumber = item.Book;

            this.InstituteString = item.Institute;

            this.SpecialityString = item.Speciality;

            this.DisciplineString = item.Subject;

            this.TeacherString = item.Teacher;

            this.StudyearNumber = item.Year;

            this.GroupString = item.Group;

            this.CourseNumber = item.Course;

            this.TermNumber = item.Term;

            this.MarkString = item.Mark;

            this.DateString = item.Date;

            this.FormString = type_string;

            Animation(Mode.Loaded);
        }

        private async void ToUpdateData(object obj)
        {
            var mark_item = this.MarkItems.Find(x => x.LongName == this.MarkString);

            var mark_id = mark_item.Id;

            var result = await UpdateStatementData(this.SessionId, this.DateString.ToString(), mark_id);

            if (result)
            {
                DependencyService.Get<IToastService>().ShortAlert("Сохранено");

                MessagingCenter.Send(this, "changed");
            }

            else
            {
                DependencyService.Get<IToastService>().ShortAlert("Не удалось отправить данные");
            }
        }

        private async Task<bool> UpdateStatementData(long studentsession_id, string stndate, long stnmark)
        {
            var json_object = new
            {
                studentsession_id,
                stndate,
                stnmark
            };

            try
            {
                var json_prepared = JsonConvert.SerializeObject(json_object);

                var json_out = await ConnectivityHandler.POST_Async("UpdateStdCrtStn", json_prepared);

                return !string.IsNullOrEmpty(json_out);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public int StudyearNumber
        {
            get => this.studyyear_number;
            set
            {
                if (this.studyyear_number != value)
                {
                    this.studyyear_number = value;
                    OnPropertyChanged(nameof(this.StudyearNumber));
                }
            }
        }

        public string SemestrNumber
        {
            get => this.semestr_number;
            set
            {
                if (this.semestr_number != value)
                {
                    this.semestr_number = value;
                    OnPropertyChanged(nameof(this.SemestrNumber));
                }
            }
        }

        public string FormString
        {
            get => this.form_string;
            set
            {
                if (this.form_string != value)
                {
                    this.form_string = value;
                    OnPropertyChanged(nameof(this.FormString));
                }
            }
        }
    }
}
