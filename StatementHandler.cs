using DvgupsMobile.DependencyServices;
using DvgupsMobile.ObservableItems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static DvgupsMobile.Services.StatementHandlers.OtherHandler;

namespace DvgupsMobile.Services.StatementHandlers
{
    public static class StatementHandler
    {
        static IToastService ToastService = DependencyService.Get<IToastService>();

        [Obsolete]
        public static async Task<StatementGroupItem> GetStatementData(List<MarkItem> marks, long session_id, long discipline_id, long educgroup_id)
        {
            var item = new StatementGroupItem();

            try
            {
                var value = await AuthHandler.GetCurrentAdmuserAsync();

                item.Teacher = value.admuser_fio;

                var json = $"{discipline_id},{educgroup_id},{session_id}";

                var json_out = await ConnectivityHandler.POST_Async("GetGroupStatement", json);

                if (string.IsNullOrEmpty(json_out))
                {
                    return null;
                }

                var data = JsonConvert.DeserializeObject<Data>(json_out);

                var sessions = data.Sessions;

                var certifications = data.Certifications;

                var books = data.Books;

                if (certifications.Count == 0)
                {
                    return null;
                }

                var certification = certifications[0];

                item.CertificationId = certification.CERTIFICATIONSTATEMENT_ID;

                item.Institute = certification.FACULTY_FULLNAME;

                item.Group = certification.EDUCGROUP_NAME;

                item.Speciality = certification.SPECIALITY_NAME;

                item.Subject = certification.STUDDISCIPLINE_NAME;

                if (sessions.Count == 0)
                {
                    return null;
                }

                var session = sessions[0];

                var study_year = session.STUDYEAR_ID;

                item.Year = study_year;

                var term_number = session.SEMESTER_NUM;

                item.Term = term_number;

                var term_half = term_number / 2;

                var course_number = term_number % 2 != 0 ? term_half + 1 : term_half;

                item.Course = course_number;

                var coursework_name = session.STUDENTSESSION_COURSEWORK_NAME;

                item.Theme = coursework_name;

                var date_item = session.STUDENTSESSION_DATE;

                item.Date = date_item != null ? date_item.date : DateTime.Now;

                var mark_id = session.MARK_ID;

                var mark_item = marks.FirstOrDefault(x => x.Id == mark_id);

                item.Mark = mark_item.LongName;

                if (books.Count == 0)
                {
                    return null;
                }

                var book = books[0];

                item.Book = book.RECORDBOOK_NUMBER;
            }

            catch (Exception ex)
            {
                ToastService.ShortAlert("Ошибка при загрузке ведомости по группе");
                LogHandler.Error(ex, nameof(StatementHandler));
            }

            return item;
        }
    }
}
