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
using Xamarin.Forms;
using static DvgupsMobile.Services.StatementHandlers.OtherHandler;

namespace DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement
{
    public class StatementItemViewModel : StatementViewModel
    {
        public tbdiscipcourse Journal { get; set; }
        public StudentItem StudentItem { get; set; }
        public int TypeId { get; set; }
        public long EducgroupId { get; set; }
        public long DisciplineId { get; set; }
        public StudentItem Student { get; set; }
        public ObservableCollection<StatementItem> StatementItems { get; set; }

        public StatementItemViewModel(int type_id, tbdiscipcourse journal, long educgroup_id, long discipline_id, StudentItem student)
        {
            Animation(Mode.Loading);

            this.Journal = journal;

            this.StudentItem = student;

            this.TypeId = type_id;

            this.EducgroupId = educgroup_id;

            this.DisciplineId = discipline_id;

            this.Student = student;

            this.StatementItems = new ObservableCollection<StatementItem>();

            Init();

            MessagingCenter.Subscribe<StatementListViewModel>(this, "changed", (vm) =>
            {
                Animation(Mode.Loading);

                this.StatementItems.Clear();

                Init();
            });

        }

        public StatementItemsPage StatementItemsPage { get; set; }

        public StatementItemViewModel()
        {

        }

        private async void Init()
        {
            var type_items = await TypeAccountingHandler.GetTypes();

            var mark_items = await MarkHandler.GetMarks(x => true);

            var items = await GetItems(mark_items, type_items, this.Student);

            if (items.Count == 0)
            {
                Animation(Mode.Failed);
                return;
            }

            foreach (var item in items)
            {
                this.StatementItems.Add(item);
            }

            Animation(Mode.Loaded);

        }

        private async Task<ObservableCollection<StatementItem>> GetItems(List<MarkItem> marks, List<TypeAccountingItem> types, StudentItem student)
        {
            var collection = new ObservableCollection<StatementItem>();

            var discipline_id = student.Discipline_id;

            var educgroup_id = student.Educkgroup_id;

            var student_id = student.StudentId;

            var json_object = new
            {
                discipline_id,
                educgroup_id,
                student_id
            };

            try
            {
                var json_prepared = JsonConvert.SerializeObject(json_object);

                var json_out = await ConnectivityHandler.POST_Async("GetListStatement", json_prepared);

                if (string.IsNullOrEmpty(json_out))
                {
                    return collection;
                }

                var rows = JsonConvert.DeserializeObject<DataRows>(json_out);

                var sessions = rows.Sessions;

                var specilas = rows.STNLists;

                var speciality = specilas[0].SPECIALITY_NAME;

                foreach (var session in sessions)
                {
                    var item = new StatementItem
                    {
                        Fio = student.LongName,

                        SessionId = session.STUDENTSESSION_ID
                    };

                    item.Number = "Лист №" + item.SessionId;

                    var mark_parse = session.MARK_ID;

                    var mark_id = mark_parse != null ? mark_parse : 28;

                    var mark_item = marks.Find(x => x.Id == mark_id);

                    item.Mark = mark_item.LongName;

                    var type_id = session.TYPEACCOUNTING_ID;

                    var type_item = types.Find(x => x.Id == type_id);

                    item.Type = type_item.FullName;

                    item.Speciality = speciality;

                    var date_item = session.STUDENTSESSION_DATE;

                    item.Date = date_item != null ? date_item.date : DateTime.Now;

                    item.Type_id = type_id;

                    collection.Add(item);
                }

                return collection;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return collection;
            }    
        }

        public class DataRows
        {
            public IList<Session> Sessions { get; set; }
            public IList<STNList> STNLists { get; set; }
        }

        public class Session
        {
            public long STUDENTSESSION_ID { get; set; }
            public int? MARK_ID { get; set; }
            public int TYPEACCOUNTING_ID { get; set; }
            public SessionDate STUDENTSESSION_DATE { get; set; }
        }
        public class STNList
        {
            public long CERTIFICATIONSTATEMENT_EDUCGROUP_ID { get; set; }
            public long SPECFACSPECIALIZATION_ID { get; set; }
            public long SPECIALFACULTY_ID { get; set; }
            public long SPECIALITY_ID { get; set; }
            public int SPECIALFACULTY_STATE { get; set; }
            public string SPECIALITY_NAME { get; set; }
            public int SPECIALITY_STATE { get; set; }

        }
    }
}
