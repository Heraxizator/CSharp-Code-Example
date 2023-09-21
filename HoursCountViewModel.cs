using DvgupsMobile.DependencyServices;
using DvgupsMobile.Models;
using DvgupsMobile.Services;
using DvgupsMobile.Services.StatementHandlers;
using DvgupsMobile.Views.Jornal.ElectronStatement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement
{
    public class HoursCountViewModel : StatementViewModel
    {
        public int TypeId { get; set; }
        public int CRTStatementId { get; set; }
        public ICommand UpdateData { get; set; }
        public ICommand LoadData { get; set; }
        public tbdiscipcourse Journal { get; set; }
        public long EducgroupAsuId { get; set; }
        public long DisciplineId { get; set; }
        public bool EditEnabled { get; set; }
        private string hours_count { get; set; }
        private string credits_count { get; set; }

        [Obsolete]
        public HoursCountViewModel(tbdiscipcourse journal, int type_id)
        {
            Animation(Mode.Loading);

            SetExecuting(false);

            this.TypeId = type_id;

            this.Journal = journal;

            this.CRTStatementId = -1;

            this.UpdateData = new Command(ToUpdateData);

            this.LoadData = new Command(ToLoadData);

            Init();
        }

        public HoursCountViewModel()
        {

        }

        public HoursCountPage HoursCountPage { get; set; }

        [Obsolete]
        private async void Init()
        {
            var educgroup_id = this.Journal.educgroup_id_global;

            var prepared_asuid = await OtherHandler.GetEducgroupAsuId(educgroup_id);

            if (prepared_asuid == null)
            {
                Animation(Mode.Failed);
                return;
            }

            var educgroup_asuid = (long)prepared_asuid;

            var discipline_id = this.Journal.discipline_id_global;

            var item = await GetSTNList(educgroup_asuid, discipline_id);

            if (item == null)
            {
                Animation(Mode.Failed);
                return;
            }

            var hours_value = item.CERTIFICATIONSTATEMENT_HOURS;

            var credits_value = item.CERTIFICATIONSTATEMENT_INTENSITY;

            this.DisciplineId = discipline_id;

            this.EducgroupAsuId = educgroup_asuid;

            this.CRTStatementId = item.CERTIFICATIONSTATEMENT_ID;

            if (hours_value == null || credits_value == null)
            {
                this.HoursCountEntered = string.Empty;

                this.CreditsCountEntered = string.Empty;
            }

            else
            {
                this.HoursCountEntered = hours_value.ToString();

                this.CreditsCountEntered = credits_value.ToString();
            }

            this.Period = DateTime.Now;

            var accaunting_types = await TypeAccountingHandler.GetTypes();

            var type = accaunting_types.FirstOrDefault(x => x.Id == item.TYPEACCOUNTING_ID);

            this.AccountingType = type != null ? type.FullName : "Нет данных";

            this.Number = item.RECORDBOOK_NUMBER.ToString();

            this.Discipline = item.STUDDISCIPLINE_NAME;

            var admuser = await AuthHandler.GetCurrentAdmuserAsync();

            this.Teacher = admuser.admuser_fio;

            this.Year = item.STUDYEAR_ID.ToString();

            this.Term = item.SEMESTER_NUM.ToString();

            this.Group = item.EDUCGROUP_NAME;

            this.Speciality = item.SPECIALITY_NAME;

            this.Institute = item.FACULTY_FULLNAME;

            this.Course = ((item.SEMESTER_NUM / 2) + (item.SEMESTER_NUM % 2)).ToString();

            this.CourseWork = !string.IsNullOrEmpty(item.STUDENTSESSION_COURSEWORK_NAME) ? item.STUDENTSESSION_COURSEWORK_NAME : "Не указано";

            var valid_date = item.CERTIFICATIONSTATEMENT_VALIDUNTIL;

            var isvalid = valid_date == null || valid_date < DateTime.Now;

            var studyear = DateTime.Now.Year.ToString();

            var yearcount = studyear.Length;

            var studeyear_parse = string.Empty + studyear[yearcount - 2] + studyear[yearcount - 1];

            var studyear_id = int.Parse(studeyear_parse);

            var lecturer_id = await TeacherHandler.GetSessionTeacher(discipline_id, educgroup_asuid, studyear_id);

            var teacher_id = await TeacherHandler.GetTeacherId(admuser.admuser_id);

            var islecturer = teacher_id == lecturer_id;

            this.EditEnabled = item.CERTIFICATIONSTATEMENT_STATE == "Открыта"; // && isvalid && islecturer;

            SetMode(this.TypeId);

            Animation(Mode.Loaded);
 
        }

        private void SetExecuting(bool running)
        {
            if (running)
            {
                this.Executing = true;
                this.Sleeping = false;
                this.Enabled = false;
            }

            else
            {
                this.Executing = false;
                this.Sleeping = true;
                this.Enabled = true;
            }
        }

        private void SetMode(int type_id)
        {
            switch (type_id)
            {
                case 0:
                    this.ListVisible = true;
                    this.StnVisible = false;
                    this.GroupVisible = false;
                    break;
                case 1:
                    this.ListVisible = false;
                    this.StnVisible = true;
                    this.GroupVisible = false;
                    break;
                case 2:
                    this.ListVisible = false; ;
                    this.StnVisible = false;
                    this.GroupVisible = true;
                    break;
            }
        }

        private int ToInt(string value)
        {
            return int.Parse(value);
        }

        private async void ToUpdateData(object obj)
        {
            var Toast = DependencyService.Get<IToastService>();

            if (!this.EditEnabled)
            {
                Toast.LongAlert("Редактирование недоступно");
                return;
            }

            SetExecuting(true);

            if (!ConnectivityHandler.Connected())
            {
                Toast.ShortAlert("Нет подключения к интернету");
                SetExecuting(false);
                return;
            }

            if (string.IsNullOrEmpty(this.CreditsCountEntered) || string.IsNullOrEmpty(this.HoursCountEntered) || this.CRTStatementId == -1)
            {
                SetExecuting(false);
                return;
            }

            var result = await UpdateHoursAndCredits(ToInt(this.CreditsCountEntered), ToInt(this.HoursCountEntered), this.CRTStatementId);

            if (result)
            {
                Toast.ShortAlert("Сохранено");
            }

            else
            {
                Toast.LongAlert("Не удалось отправить данные. Попробуйте ещё раз");
            }

            SetExecuting(false);
        }

        [Obsolete]
        private void ToLoadData(object obj)
        {
            Animation(Mode.Loading);

            Init();
        }

        private async Task<bool> UpdateHoursAndCredits(int intensity, int hours, int statement_id)
        {
            var json_object = new
            {
                statement_id,
                intensity,
                hours
            };

            try
            {
                var json_prepared = JsonConvert.SerializeObject(json_object);

                var json_out = await ConnectivityHandler.POST_Async("UpdateHourseAndIntensity", json_prepared);

                return !string.IsNullOrEmpty(json_out);
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private async Task<STNListItem> GetSTNList(long educgroup_id, long discipline_id)
        {
            var json_object = new
            {
                discipline_id,
                educgroup_id
            };

            try
            {
                var json_prepared = JsonConvert.SerializeObject(json_object);

                var json_out = await ConnectivityHandler.POST_Async("GetSTNList", json_prepared);

                if (string.IsNullOrEmpty(json_out))
                {
                    return null;
                }

                var data = JsonConvert.DeserializeObject<CertificationData>(json_out);

                var lists = data.STNLists;

                if (lists.Count == 0)
                {
                    return null;
                }

                var list = lists.First();

                return list;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        #region custom classes
        public class CertificationData
        {
            public IList<STNListItem> STNLists { get; set; }
        }

        public class STNListItem
        {
            public int CERTIFICATIONSTATEMENT_ID { get; set; }
            public int? CERTIFICATIONSTATEMENT_HOURS { get; set; }
            public int? CERTIFICATIONSTATEMENT_INTENSITY { get; set; }
            public string CERTIFICATIONSTATEMENT_STATE { get; set; }
            public int CERTIFICATIONSTATEMENT_ISBEGUNOK { get; set; }
            public DateTime? CERTIFICATIONSTATEMENT_VALIDUNTIL { get; set; }
            public string EDUCGROUP_NAME { get; set; }
            public string SPECIALITY_NAME { get; set; }
            public string FACULTY_FULLNAME { get; set; }
            public int SEMESTER_NUM { get; set; }
            public int TYPEACCOUNTING_ID { get; set; }
            public int STUDYEAR_ID { get; set; }
            public string STUDENTSESSION_COURSEWORK_NAME { get; set; }
            public int RECORDBOOK_NUMBER { get; set; }
            public string STUDDISCIPLINE_NAME { get; set; }
        }

        #endregion

        #region private properties
        private DateTime date_value { get; set; }
        private DateTime period_value { get; set; }
        private string title_value { get; set; }
        private string accounting_type { get; set; }
        private string book_number { get; set; }
        private string discipline_name { get; set; }
        private string teacher_fio { get; set; }
        private string year_value { get; set; }
        private string term_value { get; set; }
        private string group_value { get; set; }
        private string institute { get; set; }
        private string speciality { get; set; }
        private string coursework { get; set; }
        private string course { get; set; }
        private bool list_visible { get; set; }
        private bool stn_visible { get; set; }
        private bool group_visisble { get; set; }
        private bool executing { get; set; }
        private bool sleeping { get; set; }
        private bool enabled { get; set; }
        #endregion

        #region public properties
        public bool ListVisible
        {
            get => this.list_visible;
            set
            {
                if (this.list_visible != value)
                {
                    this.list_visible = value;
                    OnPropertyChanged(nameof(this.ListVisible));
                }
            }
        }

        public bool StnVisible
        {
            get => this.stn_visible;
            set
            {
                this.stn_visible = value;
                OnPropertyChanged(nameof(this.StnVisible));
            }
        }

        public bool GroupVisible
        {
            get => this.group_visisble;
            set
            {
                this.group_visisble = value;
                OnPropertyChanged(nameof(this.GroupVisible));
            }
        }

        public bool Enabled
        {
            get => this.enabled;
            set
            {
                if (this.enabled != value)
                {
                    this.enabled = value;
                    OnPropertyChanged(nameof(this.Enabled));
                }
            }
        }

        public bool Sleeping
        {
            get => this.sleeping;
            set
            {
                if (this.sleeping != value)
                {
                    this.sleeping = value;
                    OnPropertyChanged(nameof(this.Sleeping));
                }
            }
        }

        public bool Executing
        {
            get => this.executing;
            set
            {
                if (this.executing != value)
                {
                    this.executing = value;
                    OnPropertyChanged(nameof(this.Executing));
                }
            }
        }

        public string HoursCountEntered
        {
            get => this.hours_count;
            set
            {
                if (this.hours_count != value)
                {
                    this.hours_count = value;
                    OnPropertyChanged(nameof(this.HoursCountEntered));
                }
            }
        }

        public string CreditsCountEntered
        {
            get => this.credits_count;
            set
            {
                if (this.credits_count != value)
                {
                    this.credits_count = value;
                    OnPropertyChanged(nameof(this.CreditsCountEntered));
                }
            }
        }

        public DateTime Period
        {
            get => this.period_value;
            set
            {
                if (this.period_value != value)
                {
                    this.period_value = value;
                    OnPropertyChanged(nameof(this.Period));
                }
            }
        }

        public string AccountingType
        {
            get => this.accounting_type;
            set
            {
                if (this.accounting_type != value)
                {
                    this.accounting_type = value;
                    OnPropertyChanged(nameof(this.AccountingType));
                }
            }
        }

        public string Number
        {
            get => this.book_number;
            set
            {
                if (this.book_number != value)
                {
                    this.book_number = value;
                    OnPropertyChanged(nameof(this.Number));
                }
            }
        }

        public string Discipline
        {
            get => this.discipline_name;
            set
            {
                if (this.discipline_name != value)
                {
                    this.discipline_name = value;
                    OnPropertyChanged(nameof(this.Discipline));
                }
            }
        }

        public string Teacher
        {
            get => this.teacher_fio;
            set
            {
                if (this.teacher_fio != value)
                {
                    this.teacher_fio = value;
                    OnPropertyChanged(nameof(this.Teacher));
                }
            }
        }

        public string Year
        {
            get => this.year_value;
            set
            {
                if (this.year_value != value)
                {
                    this.year_value = value;
                    OnPropertyChanged(nameof(this.Year));
                }
            }
        }

        public string Term
        {
            get => this.term_value;
            set
            {
                if (this.term_value != value)
                {
                    this.term_value = value;
                    OnPropertyChanged(nameof(this.Term));
                }
            }
        }

        public string Group
        {
            get => this.group_value;
            set
            {
                if (this.group_value != value)
                {
                    this.group_value = value;
                    OnPropertyChanged(nameof(this.Group));
                }
            }
        }

        public string Speciality
        {
            get => this.speciality;
            set
            {
                if (this.speciality != value)
                {
                    this.speciality = value;
                    OnPropertyChanged(nameof(this.Speciality));
                }
            }
        }

        public string Institute
        {
            get => this.institute;
            set
            {
                if (this.institute != value)
                {
                    this.institute = value;
                    OnPropertyChanged(nameof(this.Institute));
                }
            }
        }

        public string CourseWork
        {
            get => this.coursework;
            set
            {
                if (this.coursework != value)
                {
                    this.coursework = value;
                    OnPropertyChanged(nameof(this.CourseWork));
                }
            }
        }

        public string Course
        {
            get => this.course;
            set
            {
                if (this.course != value)
                {
                    this.course = value;
                    OnPropertyChanged(nameof(this.Course));
                }
            }
        }

        #endregion
    }
}
