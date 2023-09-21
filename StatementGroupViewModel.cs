using DvgupsMobile.DependencyServices;
using DvgupsMobile.Models;
using DvgupsMobile.ObservableItems;
using DvgupsMobile.Services;
using DvgupsMobile.Services.StatementHandlers;
using DvgupsMobile.Views.Jornal.ElectronStatement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement
{
    public class StatementGroupViewModel : StatementViewModel
    {
        private readonly string title_part = "Ведомость №";
        public ICommand UpdateData { get; set; }
        public ICommand LoadData { get; set; }
        public ObservableCollection<string> MarkStrings { get; set; }
        public List<MarkItem> MarkItems { get; set; }
        public long SessionId { get; set; }
        public long CertificationId { get; set; }
        public long DisciplineId { get; set; }
        public long EducgroupId { get; set; }
        private int point_id { get; set; }
        private string title_string { get; set; }
        private string theme_string { get; set; }
        private int book_number { get; set; }
        private string institute_string { get; set; }
        private string speciality_string { get; set; }
        private string discipline_string { get; set; }
        private string teacher_fio { get; set; }
        private int study_year { get; set; }
        private string group_string { get; set; }
        private int course_number { get; set; }
        private int term_number { get; set; }
        private string mark_string { get; set; }
        private DateTime date_string { get; set; }
        public tbdiscipcourse Journal { get; set; }

        [Obsolete]
        public StatementGroupViewModel(tbdiscipcourse journal, int typespoint_id, long session_id, long discipline_id, long educgroup_id)
        {

            this.MarkStrings = new ObservableCollection<string>();

            this.MarkItems = new List<MarkItem>();

            this.TitleString = this.title_part;

            Animation(Mode.Loading);

            this.UpdateData = new Command(ToUpdateData);

            this.LoadData = new Command(ToLoadData);

            this.Journal = journal;

            this.SessionId = session_id;

            this.EducgroupId = educgroup_id;

            this.DisciplineId = discipline_id;

            this.point_id = typespoint_id;

            Init();
        }

        [Obsolete]
        protected void ToLoadData(object obj)
        {
            Animation(Mode.Loading);

            Init();
        }

        public StatementGroupPage StatementGroupPage { get; set; }

        public StatementGroupViewModel()
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

            this.CertificationId = item.CertificationId;

            this.TitleString = this.title_part + item.CertificationId;

            this.ThemeString = !string.IsNullOrEmpty(item.Theme) ? item.Theme : "Тема не указана";

            this.TermNumber = (item.Course / 2) + item.Course % 2;

            this.BookNumber = item.Book;

            this.InstituteString = item.Institute;

            this.SpecialityString = item.Speciality;

            this.DisciplineString = item.Subject;

            this.TeacherString = item.Teacher;

            this.StudyYear = item.Year;

            this.GroupString = item.Group;

            this.CourseNumber = item.Course;

            this.MarkString = item.Mark;

            this.DateString = item.Date;

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

        public DateTime DateString
        {
            get => this.date_string;
            set
            {
                if (this.date_string != value)
                {
                    this.date_string = value;
                    OnPropertyChanged(nameof(this.DateString));
                }
            }
        }

        public string TitleString
        {
            get => this.title_string;
            set
            {
                if (this.title_string != value)
                {
                    this.title_string = value;
                    OnPropertyChanged(nameof(this.TitleString));
                }
            }
        }

        public string ThemeString
        {
            get => this.theme_string;
            set
            {
                if (this.theme_string != value)
                {
                    this.theme_string = value;
                    OnPropertyChanged(nameof(this.ThemeString));
                }
            }
        }

        public int BookNumber
        {
            get => this.book_number;
            set
            {
                if (this.book_number != value)
                {
                    this.book_number = value;
                    OnPropertyChanged(nameof(this.BookNumber));
                }
            }
        }

        public string InstituteString
        {
            get => this.institute_string;
            set
            {
                if (this.institute_string != value)
                {
                    this.institute_string = value;
                    OnPropertyChanged(nameof(this.InstituteString));
                }
            }
        }

        public string SpecialityString
        {
            get => this.speciality_string;
            set
            {
                if (this.speciality_string != value)
                {
                    this.speciality_string = value;
                    OnPropertyChanged(nameof(this.SpecialityString));
                }
            }
        }

        public string DisciplineString
        {
            get => this.discipline_string;
            set
            {
                if (this.discipline_string != value)
                {
                    this.discipline_string = value;
                    OnPropertyChanged(nameof(this.DisciplineString));
                }
            }
        }

        public string TeacherString
        {
            get => this.teacher_fio;
            set
            {
                if (this.teacher_fio != value)
                {
                    this.teacher_fio = value;
                    OnPropertyChanged(nameof(this.TeacherString));
                }
            }
        }

        public int StudyYear
        {
            get => this.study_year;
            set
            {
                if (this.study_year != value)
                {
                    this.study_year = value;
                    OnPropertyChanged(nameof(this.StudyYear));
                }
            }
        }

        public string GroupString
        {
            get => this.group_string;
            set
            {
                if (this.group_string != value)
                {
                    this.group_string = value;
                    OnPropertyChanged(nameof(this.GroupString));
                }
            }
        }

        public int CourseNumber
        {
            get => this.course_number;
            set
            {
                if (this.course_number != value)
                {
                    this.course_number = value;
                    OnPropertyChanged(nameof(this.CourseNumber));
                }
            }
        }

        public int TermNumber
        {
            get => this.term_number;
            set
            {
                if (this.term_number != value)
                {
                    this.term_number = value;
                    OnPropertyChanged(nameof(this.TermNumber));
                }
            }
        }

        public string MarkString
        {
            get => this.mark_string;
            set
            {
                if (this.mark_string != value)
                {
                    this.mark_string = value;
                    OnPropertyChanged(nameof(this.MarkString));
                }
            }
        }

    }
}
