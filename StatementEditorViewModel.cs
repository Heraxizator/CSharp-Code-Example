using DvgupsMobile.DependencyServices;
using DvgupsMobile.Models;
using DvgupsMobile.ObservableItems;
using DvgupsMobile.Services;
using DvgupsMobile.Services.StatementHandlers;
using DvgupsMobile.Views.Jornal.ElectronStatement;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using static DvgupsMobile.Services.StatementHandlers.OtherHandler;

namespace DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement
{
    public class StatementEditorViewModel : StatementViewModel
    {
        public int TypeId { get; set; }
        public IToastService ToastService { get; set; }
        public tbdiscipcourse Journal { get; set; }
        public ObservableCollection<StudentItem> AllItems { get; set; }
        public ObservableCollection<StudentItem> SelectedItems { get; set; }
        public ICommand LoadData { get; set; }
        public long EducgroupId { get; set; }
        public long DisciplineId { get; set; }
        public long TeacherId { get; set; }
        public bool EditEnabled { get; set; }
        private string text_entered { get; set; }
        private bool executing { get; set; }

        [Obsolete]
        public StatementEditorViewModel(tbdiscipcourse journal, long educgroup_id, long discipline_id, int type_id, bool edit_enabled)
        {
            Animation(Mode.Loading);

            this.ToastService = DependencyService.Get<IToastService>();

            this.Excecuting = false;

            this.Text_Entered = string.Empty;

            this.LoadData = new Command(ToReloadItems);

            this.AllItems = new ObservableCollection<StudentItem>();

            this.SelectedItems = new ObservableCollection<StudentItem>();

            this.TypeId = type_id;

            this.Journal = journal;

            this.EducgroupId = educgroup_id;

            this.DisciplineId = discipline_id;

            this.EditEnabled = edit_enabled;

            Init();

            MessagingCenter.Subscribe<StatementListViewModel>(this, "changed", (vm) => ToReloadItems());

            MessagingCenter.Subscribe<StatementGroupViewModel>(this, "changed", (vm) => ToReloadItems());

            MessagingCenter.Subscribe<StatementSTNViewModel>(this, "changed", (vm) => ToReloadItems());
        }

        [Obsolete]
        public async void UpdateItem(int item_index, long teacher_id)
        {
            if (!this.EditEnabled)
            {
                ToastService.LongAlert("Редактирование недоступно");
                return;
            }

            try
            {
                SwapMark(item_index);

                var admuser = await AuthHandler.GetCurrentAdmuserAsync();

                var item = this.SelectedItems[item_index];

                var marks = await MarkHandler.GetMarks(x => true);

                var mark_item = marks.Find(x => x.LongName == item.Mark);

                if (mark_item == null)
                {
                    ToastService.ShortAlert("Не удалось сохранить, поскольку оценка не найдена");
                    return;
                }

                var stn_student = new STNStudent
                {
                    Mark_id = mark_item.Id,
                    Session_date = item.Date,
                    Teacher_id = teacher_id,
                    StudentStatement_Id = item.StudentStatementId
                };

                var stn_status = new STNStatus
                {
                    CertificationStatement_Id = item.CertificationStatementId,
                    ValidUntill = item.Date.AddDays(3),
                    IsBegunok = this.TypeId == 0 ? 1 : 0
                };

                var stn_teacher = new STNTeacher
                {
                    CertificationStatement_Id = item.CertificationStatementId,
                    Teacher_id = teacher_id
                };

                var result = await UpdateAll(stn_student, stn_status, stn_teacher);

                if (!result.Item1 || !result.Item2)
                {
                    if (!ConnectivityHandler.Connected())
                    {
                        ToastService.ShortAlert("Нет подключения к интернету");
                    }

                    else
                    {
                        ToastService.ShortAlert("Не удалось сохранить. Попробуйте ещё раз");
                    }
                    
                    return;
                }
            }

            catch (ArgumentOutOfRangeException aorex)
            {
                Console.WriteLine(aorex.Message);
                LogHandler.Error(aorex, nameof(StatementEditorViewModel));
            }

            catch (Exception ex)
            {
                ToastService.ShortAlert("Ошибка при сохранении оценки");
                LogHandler.Error(ex, nameof(StatementEditorViewModel));
            }
        }

        [Obsolete]
        private void ToReloadItems()
        {
            Animation(Mode.Loading);

            ClearItems();

            Init();
        }

        [Obsolete]
        private async void Init()
        {
            var marks = await MarkHandler.GetMarks(x => x == 1);

            var offsets = await MarkHandler.GetMarks(x => x == 4);

            marks.Reverse();

            offsets.Reverse();

            var isbegunok = this.TypeId == 0 ? 1 : 0;

            var items = await GetItems(marks, offsets, this.EducgroupId, this.DisciplineId, isbegunok);

            if (items.Count == 0)
            {
                Animation(Mode.Failed);
                return;
            }

            InitItems(items);

            var admuser = await AuthHandler.GetCurrentAdmuserAsync();

            var teacher_id = await TeacherHandler.GetTeacherId(admuser.admuser_id);

            this.TeacherId = teacher_id;

            Animation(Mode.Loaded);
              
        }

        private void InitItems(ConcurrentBag<StudentItem> items)
        {
            foreach (var item in items)
            {
                this.AllItems.Add(item);
                this.SelectedItems.Add(item);

            }
        }

        private void ClearItems()
        {
            this.AllItems.Clear();
            this.SelectedItems.Clear();
        }

        private Color GetColor(string mark)
        {
            return mark switch
            {
                "Неудовлетворительно" => Color.FromHex("#E11F1F"),
                "Удовлетворительно" => Color.FromHex("#E0B311"),
                "Хорошо" => Color.FromHex("#40A028"),
                "Отлично" => Color.FromHex("#40A028"),
                "Зачтено" => Color.FromHex("#40A028"),
                "Незачет" => Color.FromHex("#E11F1F"),
                "Неявка" => Color.Gray,
                "Не допущен" => Color.Gray,
                _ => Color.Gray,
            };
        }

        public void SwapMark(int item_index)
        {
            try
            {
                var item = this.SelectedItems[item_index];

                var mark_id = item.MarkId;

                var mark_last = item.Marks.Count - 1;

                var current_id = mark_id != mark_last ? mark_id + 1 : 0;

                var current_mark = item.Marks[current_id];

                var current_color = GetColor(current_mark);

                this.SelectedItems[item_index].MarkId = current_id;

                this.SelectedItems[item_index].Mark = current_mark;

                this.SelectedItems[item_index].Color = current_color;
            }

            catch(ArgumentOutOfRangeException aorex)
            {
                Console.WriteLine(aorex.Message);
            }

            catch(Exception ex)
            {
                ToastService.ShortAlert("Ошибка при смене цвета");
                LogHandler.Error(ex, nameof(StatementEditorViewModel));
            }
        }

        public StatementEditorPage StatementEditorPage { get; set; }

        public StatementEditorViewModel()
        {

        }

        private async Task<ConcurrentBag<StudentItem>> GetItems(List<MarkItem> marks, List<MarkItem> offsets, long educgroup_id, long discipline_id, int isbegunok)
        {
            var collection = new ConcurrentBag<StudentItem>();

            var json_object = new
            {
                discipline_id,
                educgroup_id,
                isbegunok
            };

            try
            {
                var json_prepared = JsonConvert.SerializeObject(json_object);

                var json_out = await ConnectivityHandler.POST_Async("GetStudentsByIds", json_prepared);

                if (string.IsNullOrEmpty(json_out))
                {
                    return collection;
                }

                var marks_strings = MarkHandler.GetMarksStrings(marks, true);

                var offsets_strings = MarkHandler.GetMarksStrings(offsets, true);

                var session = JsonConvert.DeserializeObject<StudentItems>(json_out);

                var students = session.Students;

                foreach (var student in students)
                {
                    var studentsession_id = student.STUDENTSESSION_ID;

                    var student_id = student.STUDENT_ID;

                    var date_item = student.STUDENTSESSION_DATE;

                    var date_value = date_item != null ? date_item.date : DateTime.Now;

                    var teacher_id = student.TEACHERCHAIR_ID;

                    var accounting_typeid = student.TYPEACCOUNTING_ID;

                    var ismark = accounting_typeid != 1;

                    var mark_emptyid = ismark ? 29 : 31;

                    var mark_id = student.MARK_ID != null ? student.MARK_ID : mark_emptyid;

                    var mark_items = ismark ? marks : offsets;

                    var mark_parse = mark_items.FirstOrDefault(x => x.Id == mark_id);

                    var mark = mark_parse ?? mark_items.FirstOrDefault(x => x.Id == mark_emptyid);

                    var mark_strings = ismark ? marks_strings : offsets_strings;

                    var first_name = student.PERSON_FIRSTNAME;

                    var last_name = student.PERSON_LASTNAME;

                    var father_name = student.PERSON_FATHERNAME;

                    var item = new StudentItem
                    {
                        Marks = mark_strings,
                        StudentId = student_id,
                        SessionId = studentsession_id,
                        Educkgroup_id = educgroup_id,
                        Discipline_id = discipline_id,
                        Date = date_value,
                        Mark = mark.LongName,
                        TypeId = accounting_typeid,
                        Color = GetColor(mark.LongName),
                        LongName = $"{last_name} {first_name} {father_name}",
                        ShortName = $"{last_name}{first_name.First()}{father_name.First()}",
                        Speciality = student.TYPEACCOUNTING_FULLNAME,
                        StudentStatementId = student.STUDENTCERTIFICATIONSTATEMENT_ID,
                        CertificationStatementId = student.CERTIFICATIONSTATEMENT_ID,
                        Enabled = student.CERTIFICATIONSTATEMENT_STATE == "Открыта"
                    };

                    var mark_index = mark_strings.IndexOf(mark.LongName);

                    item.MarkId = mark_index != -1 ? mark_index : 0;

                    collection.Add(item);
                }
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return collection;
        }

        private async Task<(bool, bool, bool)> UpdateAll(STNStudent student, STNStatus status, STNTeacher teacher)
        {
            var is_student = await UpdateItem(student, "UpdateStnStudent");

            var is_teacher = await UpdateItem(teacher, "UpdateSTNTeacher");

            var is_status = await UpdateItem(status, "UpdateStnStatus");

            return (is_student, is_teacher, is_status);
        }

        private async Task<bool> UpdateItem<T>(T item, string method)
        {
            try
            {
                var json_object = JsonConvert.SerializeObject(item);

                var json_out = await ConnectivityHandler.POST_Async(method, json_object);

                return !string.IsNullOrEmpty(json_out);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private ObservableCollection<StudentItem> SelectItems(ObservableCollection<StudentItem> collection, string pattern)
        {
            var items = new ObservableCollection<StudentItem>();

            var upper = pattern.ToUpper();

            foreach (var item in collection)
            {
                var name_long = item.LongName.ToUpper();

                var name_short = item.ShortName.ToUpper();

                if (name_long.Contains(upper) || name_short.Contains(upper))
                {
                    items.Add(item);
                }
            }

            return items;
        }

        public void ExecuteSearch()
        {

            var list = SelectItems(this.AllItems, this.Text_Entered);

            this.SelectedItems.Clear();

            foreach (var item in list)
            {
                this.SelectedItems.Add(item);
            }
        }

        public string Text_Entered
        {
            get => this.text_entered;
            set
            {
                if (this.text_entered != value)
                {
                    this.text_entered = value;
                    OnPropertyChanged(nameof(this.Text_Entered));
                }
            }
        }

        public bool Excecuting
        {
            get => this.executing;
            set
            {
                if (this.executing != value)
                {
                    this.executing = value;
                    OnPropertyChanged(nameof(this.Excecuting));
                }
            }
        }

        public class ValueItem
        {
            public string admparamuservalue_value { get; set; }
        }

        private class STNStudent
        {
            public long StudentStatement_Id { get; set; }
            public long Mark_id { get; set; }
            public DateTime Session_date { get; set; }
            public long Teacher_id { get; set; }
        }

        private class STNStatus
        {
            public long CertificationStatement_Id { get; set; }
            public DateTime ValidUntill { get; set; }
            public int IsBegunok { get; set; }
        }

        private class STNTeacher
        {
            public long CertificationStatement_Id { get; set; }
            public long Teacher_id { get; set; }
        }

        public class StudentItems
        {
            public IList<Student> Students { get; set; }
        }

        public class Student
        {
            public long STUDENTSESSION_ID { get; set; }
            public long STUDENT_ID { get; set; }
            public long? MARK_ID { get; set; }
            public long? TEACHERCHAIR_ID { get; set; }
            public int STUDENTSESSION_STATE { get; set; }
            public SessionDate STUDENTSESSION_DATE { get; set; }
            public int TYPEACCOUNTING_ID { get; set; }
            public string TYPEACCOUNTING_FULLNAME { get; set; }
            public long STUDPERSON_ID { get; set; }
            public int STUDENT_STATE { get; set; }
            public long PERSON_ID { get; set; }
            public string PERSON_LASTNAME { get; set; }
            public string PERSON_FIRSTNAME { get; set; }
            public string PERSON_FATHERNAME { get; set; }
            public int PERSON_STATE { get; set; }
            public string CERTIFICATIONSTATEMENT_STATE { get; set; }
            public long STUDENTCERTIFICATIONSTATEMENT_ID { get; set; }
            public long CERTIFICATIONSTATEMENT_ID { get; set; }
        }

    }
}
