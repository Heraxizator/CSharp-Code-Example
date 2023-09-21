using DvgupsMobile.DependencyServices;
using DvgupsMobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using static DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement.StatementEditorViewModel;

namespace DvgupsMobile.Services.StatementHandlers
{
    public static class OtherHandler
    {
        static IToastService Toast = DependencyService.Get<IToastService>();

        public static async Task<int> GetRunnersCount(long discipline_id, long educgroup_id, long isbegunok)
        {
            var json_object = new
            {
                discipline_id,
                educgroup_id,
                isbegunok
            };

            var json_prepared = JsonConvert.SerializeObject(json_object);

            try
            {
                var json_out = await ConnectivityHandler.POST_Async("GetStudentsByIds", json_prepared);

                var data = JsonConvert.DeserializeObject<StudentItems>(json_out);

                if (data == null)
                {
                    return 0;
                }

                var items = data.Students;

                return items.Count;
            }

            catch (Exception ex)
            {
                Toast.ShortAlert("Ошибка при вычислении количества бегунков");
                LogHandler.Error(ex, nameof(OtherHandler));
                return default;
            }
        }

        public static async Task<int?> GetEducgroupAsuId(long educgroup_id)
        {
            var educgroup = await SyncDB.FirstOrDefaultAsync<educgroup>(x => x.educgroup_id_global == educgroup_id);

            return educgroup.educgroup_asuid;
        }

        #region public structures
        public class Data
        {
            public IList<Session> Sessions { get; set; }
            public IList<Certification> Certifications { get; set; }
            public IList<Book> Books { get; set; }
        }

        public class Session
        {
            public long STUDENTSESSION_ID { get; set; }
            public long STUDENT_ID { get; set; }
            public int STUDENTSESSION_STATE { get; set; }
            public string STUDENTSESSION_COURSEWORK_NAME { get; set; }
            public int SEMESTER_NUM { get; set; }
            public int TYPEACCOUNTING_ID { get; set; }
            public SessionDate STUDENTSESSION_DATE { get; set; }
            public int STUDYEAR_ID { get; set; }
            public int MARK_ID { get; set; }
        }

        public class Certification
        {
            public int CERTIFICATIONSTATEMENT_ID { get; set; }
            public long CERTIFICATIONSTATEMENT_STUDDISCIPLINE_ID { get; set; }
            public long CERTIFICATIONSTATEMENT_EDUCGROUP_ID { get; set; }
            public int CERTIFICATIONSTATEMENT_TYPEACCOUNTING_ID { get; set; }
            public string CERTIFICATIONSTATEMENT_STATE { get; set; }
            public string STUDDISCIPLINE_NAME { get; set; }
            public string EDUCGROUP_NAME { get; set; }
            public long SPECIALITY_ID { get; set; }
            public long FACULTY_ID { get; set; }
            public string FACULTY_FULLNAME { get; set; }
            public string SPECIALITY_NAME { get; set; }
        }

        public class Book
        {
            public long RECORDBOOK_ID { get; set; }
            public int RECORDBOOK_NUMBER { get; set; }
        }

        public class SessionDate
        {
            public DateTime date { get; set; }
            public int timezone_type { get; set; }
            public string timezone { get; set; }
        }

        public class Teacher
        {
            public long TEACHERCHAIR_ID { get; set; }
        }

        #endregion
    }
}
