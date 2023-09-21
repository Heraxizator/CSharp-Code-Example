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
using static DvgupsMobile.Services.StatementHandlers.OtherHandler;

namespace DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement
{
    public class StatementListViewModel : StatementViewModel
    {
        public List<MarkItem> MarkItems { get; set; }
        public ObservableCollection<string> MarkValues { get; set; }
        public tbdiscipcourse Journal { get; set; }
        public StatementItem StatementItem { get; set; }
        public ICommand UpdateData { get; set; }
        public ICommand LoadData { get; set; }
        private DateTime date_value { get; set; }
        private DateTime period_value { get; set; }
        private string title_value { get; set; }
        private string accounting_type { get; set; }
        private string book_number { get; set; }
        private string discipline_name { get; set; }
        private string teacher_fio { get; set; }
        private string student_fio { get; set; }
        private string year_value { get; set; }
        private string term_value { get; set; }
        private string group_value { get; set; }
        private string mark_value { get; set; }

        public StatementListViewModel(tbdiscipcourse journal, StatementItem statement)
        {
            this.Title = "Аттестационный лист №" + statement.SessionId;

            this.MarkValues = new ObservableCollection<string>();

            this.MarkItems = new List<MarkItem>();

            this.DateValue = statement.Date;

            Animation(Mode.Loading);

            this.StatementItem = statement;

            this.Journal = journal;

            this.UpdateData = new Command(ToUpdateData);

            this.LoadData = new Command(ToLoadData);

            Init();
        }

        private void ToLoadData(object obj)
        {
            Animation(Mode.Loading);

            Init();
        }

        private async void ToUpdateData(object obj)
        {
            var mark_item = this.MarkItems.Find(x => x.LongName == this.MarkValue);

            var mark_id = mark_item.Id;

            var result = await UpdateStatementData(this.StatementItem.SessionId, this.DateValue.ToString(), mark_id);

            if (result)
            {
                DependencyService.Get<IToastService>().ShortAlert("Данные приняты в обработку");

                MessagingCenter.Send(this, "changed");
            }

            else
            {
                DependencyService.Get<IToastService>().ShortAlert("Не удалось отправить данные");
            }
        }

        public StatementListPage StatementListPage { get; set; }

        public StatementListViewModel()
        {

        }

        private async void Init()
        {
            var type_id = this.StatementItem.Type_id != 1 ? 1 : 4;

            var mark_items = await MarkHandler.GetMarks(x => x == type_id);

            foreach (var mark_item in mark_items)
            {
                this.MarkItems.Add(mark_item);
            }

            var type_items = await TypeAccountingHandler.GetTypes();

            var group_name = this.Journal.educgroup_name_default;

            var statement_item = await GetStatementData(type_items, mark_items, this.StatementItem, group_name);

            if (statement_item == null)
            {
                Animation(Mode.Failed);
                return;
            }

            var mark_strings = MarkHandler.GetMarksStrings(mark_items, true);

            foreach (var mark_string in mark_strings)
            {
                this.MarkValues.Add(mark_string);
            }

            this.AccountingType = !string.IsNullOrEmpty(statement_item.AccountingType) ? statement_item.AccountingType : "Форма не указана";

            this.Discipline = !string.IsNullOrEmpty(statement_item.Discipline) ? statement_item.Discipline : "Дисциплина не указана";

            this.Teacher = !string.IsNullOrEmpty(statement_item.TeacherFIO) ? statement_item.TeacherFIO : "Преподаватель не указан";

            this.Year = statement_item.Year != 0 ? statement_item.Year.ToString() : "Год не указан";

            this.Term = statement_item.Term != 0 ? statement_item.Term.ToString() : "Семестр не указан";

            this.DateValue = statement_item.Date != null ? statement_item.Date : DateTime.Now;

            this.Period = statement_item.Period != null ? statement_item.Period : DateTime.Now;

            this.Number = statement_item.Number;

            this.Student = statement_item.StudentFIO;

            this.Group = statement_item.Group;

            this.MarkValue = statement_item.Mark;

            Animation(Mode.Loaded);
        }

        private Task<StatementListItem> GetStatementData(List<TypeAccountingItem> types, List<MarkItem> marks, StatementItem statement, string group_name)
        {
            try
            {
                var item = new StatementListItem();

                var studentsession_id = statement.SessionId;

                item.Id = studentsession_id;

                item.Group = group_name;

                item.StudentFIO = statement.Fio;

                return Task.Run(async () =>
                {
                    var json_out = await ConnectivityHandler.POST_Async("GetStatementList", studentsession_id.ToString());

                    if (string.IsNullOrEmpty(json_out))
                    {
                        return null;
                    }

                    var data = JsonConvert.DeserializeObject<Data>(json_out);

                    var stnlists = data.STNLists;

                    var stnbooks = data.Books;

                    var stnperiods = data.STNPeriods;

                    if (stnlists.Count != 0)
                    {
                        var stnitem = stnlists[0];

                        var typeaccounting_id = stnitem.TYPEACCOUNTING_ID;

                        var typeacconting_item = types.Find(x => x.Id == typeaccounting_id);

                        var typeaccounting_name = typeacconting_item.FullName;

                        item.AccountingType = typeaccounting_name;

                        var mark_parse = stnitem.MARK_ID;

                        var mark_id = mark_parse != null ? mark_parse : 31;

                        var mark_list = new List<MarkItem>(marks.ToArray());

                        var mark_item = mark_list.Find(x => x.Id == mark_id);

                        var mark_value = mark_item != null ? mark_item.LongName : "Нет оценки";

                        item.Mark = mark_value;

                        var studyear_id = stnitem.STUDYEAR_ID;

                        item.Year = studyear_id;

                        var semester_num = stnitem.SEMESTER_NUM;

                        item.Term = semester_num;

                        var date_item = stnitem.STUDENTSESSION_DATE;

                        var session_date = date_item != null ? date_item.date : DateTime.Now.Date;

                        item.Date = session_date;

                        item.TeacherFIO = stnitem.CHAIR_HEAD;

                        item.Discipline = stnitem.CHAIR_NAME;
                    }

                    if (stnbooks.Count != 0)
                    {
                        var stnbook = stnbooks[0];

                        item.Number = stnbook.RECORDBOOK_NUMBER.ToString();
                    }

                    if (stnperiods.Count != 0)
                    {
                        var stnperiod = stnperiods[0];

                        var period_item = stnperiod.PERIOD_DATE;

                        var session_period = period_item != null ? period_item.date : DateTime.Now.AddDays(3);

                        item.Period = session_period;
                    }

                    return item;
                });
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
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

        public class Data
        {
            public IList<STNList> STNLists { get; set; }
            public IList<Book> Books { get; set; }
            public IList<STNPeriod> STNPeriods { get; set; }
        }

        public class STNList
        {
            public long STUDENTSESSION_ID { get; set; }
            public long STUDENT_ID { get; set; }
            public long TEACHERCHAIR_ID { get; set; }
            public int? MARK_ID { get; set; }
            public int STUDENTSESSION_STATE { get; set; }
            public int SEMESTER_NUM { get; set; }
            public int TYPEACCOUNTING_ID { get; set; }
            public int STUDYEAR_ID { get; set; }
            public SessionDate STUDENTSESSION_DATE { get; set; }
            public SessionDate PERIOD_DATE { get; set; }
            public int TEACHERCHAIR_CHAIR_ID { get; set; }
            public int TEACHERCHAIR_STATE { get; set; }
            public string CHAIR_NAME { get; set; }
            public int CHAIR_STATE { get; set; }
            public string CHAIR_HEAD { get; set; }
        }

        public class Book
        {
            public long RECORDBOOK_ID { get; set; }
            public long RECORDBOOK_NUMBER { get; set; }
        }

        public class STNPeriod
        {
            public SessionDate PERIOD_DATE { get; set; }
        }

        public DateTime DateValue
        {
            get => this.date_value;
            set
            {
                if (this.date_value != value)
                {
                    this.date_value = value;
                    OnPropertyChanged(nameof(this.DateValue));
                }
            }
        }

        public string Title
        {
            get => this.title_value;
            set
            {
                if (this.title_value != value)
                {
                    this.title_value = value;
                    OnPropertyChanged(nameof(this.Title));
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

        public string Student
        {
            get => this.student_fio;
            set
            {
                if (this.student_fio != value)
                {
                    this.student_fio = value;
                    OnPropertyChanged(nameof(this.Student));
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

        public string MarkValue
        {
            get => this.mark_value;
            set
            {
                if (this.mark_value != value)
                {
                    this.mark_value = value;
                    OnPropertyChanged(nameof(this.MarkValue));
                }
            }
        }
    }
}
