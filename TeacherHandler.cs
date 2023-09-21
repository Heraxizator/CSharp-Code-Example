using DvgupaMobile.DAL.SQLite.Models.Journal;
using DvgupsMobile.DependencyServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static DvgupsMobile.ViewModels.JornalViewModels.ElectronStatement.StatementEditorViewModel;

namespace DvgupsMobile.Services.StatementHandlers
{
    public static class TeacherHandler
    {
        static IToastService ToastService = DependencyService.Get<IToastService>();
        public static async Task<long?> GetSessionTeacher(long discipline_id, long educgroup_id, long studyear_id)
        {
            var json_object = new
            {
                discipline_id,
                educgroup_id,
                studyear_id
            };

            var json_prepared = JsonConvert.SerializeObject(json_object);

            try
            {
                var json_out = await ConnectivityHandler.POST_Async("GetSessionTeacher", json_prepared);

                var data = JsonConvert.DeserializeObject<OtherHandler.Teacher>(json_out);

                return data?.TEACHERCHAIR_ID;
            }

            catch (Exception ex)
            {
                ToastService.ShortAlert("Ошибка при получении идентификатора лектора");
                LogHandler.Error(ex, nameof(TeacherHandler));
                return null;
            }

            
        }

        public static async Task<long> GetTeacherId(long admuser_id)
        {
            try
            {
                var json_out = await ConnectivityHandler.POST_Async("GetTeacherId", admuser_id.ToString());

                if (string.IsNullOrEmpty(json_out))
                {
                    return -1;
                }

                var values = JsonConvert.DeserializeObject<ValueItem>(json_out);

                var row = values.admparamuservalue_value;

                if (string.IsNullOrEmpty(row))
                {
                    return -1;
                }

                var parts = row.Split(',');

                if (parts.Length == 0)
                {
                    return -1;
                }

                var item = parts.First();

                var teacher_id = long.Parse(item);

                return teacher_id;
            }

            catch(Exception ex)
            {
                ToastService.ShortAlert("Ошибка при получении идентификатора преподавателя");
                LogHandler.Error(ex, nameof(TeacherHandler));
                return -1;
            }
        }
    }
}
