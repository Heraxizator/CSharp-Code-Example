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
    public class GroupStatementsViewModel : StatementViewModel
    {
        public int TypeId { get; set; }
        public long DisciplineId { get; set; }
        public long Educgroup_id { get; set; }
        public bool EditEnabled { get; set; }
        public tbdiscipcourse Journal { get; set; }
        public ObservableCollection<GroupStatement> GroupStatements { get; set; }

        public GroupStatementsViewModel(tbdiscipcourse journal, long educgroup_id, long discipline_id, int type_id, bool edit_enabled)
        {
            Animation(Mode.Loading);

            this.TypeId = type_id;

            this.Journal = journal;

            this.Educgroup_id = educgroup_id;

            this.DisciplineId = discipline_id;

            this.EditEnabled = edit_enabled;

            this.GroupStatements = new ObservableCollection<GroupStatement>();

            Init();

            MessagingCenter.Subscribe<StatementGroupViewModel>(this, "changed", (vm) =>
            {
                Animation(Mode.Loading);

                this.GroupStatements.Clear();

                Init();
            });
        }

        public GroupStatementsPage GroupStatementsPage { get; set; }

        public GroupStatementsViewModel()
        {

        }

        private async void Init()
        {
            var educgroup_id = this.Journal.educgroup_id_global;

            var prepared_asuid = await GetEducgroupAsuId(educgroup_id);

            if (prepared_asuid == null)
            {
                Animation(Mode.Failed);
                return;
            }

            var educgroup_asuid = (long)prepared_asuid;

            var discipline_id = this.Journal.discipline_id_global;

            var types = await TypeAccountingHandler.GetTypes();

            var items = await GetStatementItems(types, educgroup_asuid, discipline_id);

            if (items == null)
            {
                Animation(Mode.Failed);
                return;
            }

            foreach (var item in items)
            {
                this.GroupStatements.Add(item);
            }

            Animation(Mode.Loaded);
        }

        private async Task<int?> GetEducgroupAsuId(long educgroup_id)
        {
            var educgroup = await SyncDB.FirstOrDefaultAsync<educgroup>(x => x.educgroup_id_global == educgroup_id);

            return educgroup.educgroup_asuid;
        }

        private async Task<List<GroupStatement>> GetStatementItems(List<TypeAccountingItem> types, long educgroup_id, long discipline_id)
        {
            try
            {
                var list = new List<GroupStatement>();

                var statement_item = new GroupStatement();

                var data_prepared = $"{discipline_id},{educgroup_id}";

                var json_out = await ConnectivityHandler.POST_Async("GetGroupItems", data_prepared);

                if (string.IsNullOrEmpty(json_out))
                {
                    return null;
                }

                var data = JsonConvert.DeserializeObject<Data>(json_out);

                var certifications = data.Certifications;

                if (certifications.Count == 0)
                {
                    return null;
                }

                var certification = certifications[0];

                var certification_id = certification.CERTIFICATIONSTATEMENT_ID;

                statement_item.Id = certification_id;

                statement_item.Number = "Ведомость №" + certification_id;

                var typeaccounting_id = certification.CERTIFICATIONSTATEMENT_TYPEACCOUNTING_ID;

                var type_item = types.Find(x => x.Id == typeaccounting_id);

                var type_name = type_item.FullName;

                statement_item.Type = type_name;

                var certification_state = certification.CERTIFICATIONSTATEMENT_STATE;

                statement_item.Status = certification_state ?? "Не подписан";

                statement_item.Subject = certification.STUDDISCIPLINE_NAME;

                statement_item.Group = certification.SPECIALITY_NAME;

                var certification_date = certification.CERTIFICATIONSTATEMENT_ISSUEDATE;

                statement_item.Date = certification_date != null ? certification_date.date : DateTime.Now;

                list.Add(statement_item);

                return list;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public class Data
        {
            public IList<CertificationItem> Certifications { get; set; }
        }

        public class CertificationItem : Certification
        {
            public SessionDate CERTIFICATIONSTATEMENT_ISSUEDATE { get; set; }
        }
    }
}
