using DvgupaMobile.DAL.SQLite.Models.Services.Statement;
using DvgupsMobile.DependencyServices;
using DvgupsMobile.ObservableItems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static DvgupsMobile.Services.StatementHandlers.OtherHandler;

namespace DvgupsMobile.Services.StatementHandlers
{
    public static class MarkHandler
    {
        static IToastService Toast = DependencyService.Get<IToastService>();

        public static List<string> GetMarksStrings(List<MarkItem> list, bool islong = false)
        {
            var collection = new List<string>();

            foreach (var item in list)
            {
                if (islong)
                {
                    collection.Add(item.LongName);
                }
                else
                {
                    collection.Add(item.ShortName);
                }
            }

            return collection;
        }

        public static async Task<List<MarkItem>> GetMarks(int type_id)
        {
            var collection = new List<MarkItem>();

            try
            {
                var json_out = await ConnectivityHandler.POST_Async("GetMarks", type_id.ToString());

                if (string.IsNullOrEmpty(json_out))
                {
                    return collection;
                }

                var dataset = JsonConvert.DeserializeObject<DataSet>(json_out);

                var marks = dataset.Tables["marks"].AsEnumerable();

                foreach (var mark in marks)
                {
                    var mark_state = mark["MARK_STATE"].ToString();

                    if (mark_state != "1")
                    {
                        continue;
                    }

                    var type_sid = int.Parse(mark["MARK_TYPESYSPOINT_ID"].ToString());

                    if (type_sid != type_id)
                    {
                        continue;
                    }

                    var mark_id = int.Parse(mark["MARK_ID"].ToString());

                    var mark_digit = int.Parse(mark["MARK_DIGIT"].ToString());

                    var mark_long = mark["MARK_FULLNAME"].ToString();

                    var mark_short = mark["MARK_SHORTNAME"].ToString();

                    var item = new MarkItem
                    {
                        Id = mark_id,
                        Digit = mark_digit,
                        LongName = mark_long,
                        ShortName = mark_short,

                    };

                    collection.Add(item);
                }
            }

            catch(JsonSerializationException jsex)
            {
                Toast.ShortAlert("Не удалось десерелизовать данные");
                LogHandler.Error(jsex, nameof(MarkHandler));
            }

            catch (NullReferenceException nrex)
            {
                Toast.ShortAlert("Нет данных от сервера");
                LogHandler.Error(nrex, nameof(MarkHandler));
            }

            catch (ArgumentException aex)
            {
                Toast.ShortAlert("Ошибка при обработке данных");
                LogHandler.Error(aex, nameof(MarkHandler));
            }

            catch (Exception ex)
            {
                Toast.ShortAlert("Не удалось получить список оценкок");
                LogHandler.Error(ex, nameof(MarkHandler));
            }

            return collection;
        }


        public static async Task<List<MarkItem>> GetMarks()
        {
            var collection = new List<MarkItem>();

            try
            {
                var json_out = await ConnectivityHandler.POST_Async("GetMarks", "1, 4");

                if (string.IsNullOrEmpty(json_out))
                {
                    return collection;
                }

                var dataset = JsonConvert.DeserializeObject<DataSet>(json_out);

                var marks = dataset.Tables["marks"].AsEnumerable();

                foreach (var mark in marks)
                {
                    var mark_state = mark["MARK_STATE"].ToString();

                    if (mark_state != "1")
                    {
                        continue;
                    }

                    var mark_id = int.Parse(mark["MARK_ID"].ToString());

                    var mark_digit = int.Parse(mark["MARK_DIGIT"].ToString());

                    var type_id = int.Parse(mark["MARK_TYPESYSPOINT_ID"].ToString());

                    var mark_long = mark["MARK_FULLNAME"].ToString();

                    var mark_short = mark["MARK_SHORTNAME"].ToString();

                    var item = new MarkItem
                    {
                        Id = mark_id,
                        Digit = mark_digit,
                        LongName = mark_long,
                        ShortName = mark_short,
                        TypeId = type_id
                    };

                    collection.Add(item);
                }
            }

            catch (JsonSerializationException jsex)
            {
                Toast.ShortAlert("Не удалось десерелизовать данные");
                LogHandler.Error(jsex, nameof(MarkHandler));
            }

            catch (NullReferenceException nrex)
            {
                Toast.ShortAlert("Нет данных от сервера");
                LogHandler.Error(nrex, nameof(MarkHandler));
            }

            catch (ArgumentException aex)
            {
                Toast.ShortAlert("Ошибка при обработке данных");
                LogHandler.Error(aex, nameof(MarkHandler));
            }

            catch (Exception ex)
            {
                Toast.ShortAlert("Не удалось получить список оценкок");
                LogHandler.Error(ex, nameof(MarkHandler));
            }

            return collection;
        }

        public static async Task<List<MarkItem>> GetMarks(Func<int, bool> func)
        {
            var list = new List<MarkItem>();

            var items = await SyncDB.GetListAsync<STNMark>();

            foreach (var item in items)
            {
                if (!func(item.TypeId))
                {
                    continue;
                }

                list.Add(new MarkItem
                {
                    Id = item.GlobalId,
                    LongName = item.LongName,
                    ShortName = item.ShortName,
                    Digit = item.Digit
                });
            }

            return list;
        }

        public static async Task SaveMarks()
        {
            var list = await MarkHandler.GetMarks();

            var collection = new List<STNMark>();

            var counter = 0;

            foreach (var item in list)
            {
                collection.Add(new STNMark
                {
                    GlobalId = item.Id,
                    LocalId = counter,
                    LongName = item.LongName,
                    ShortName = item.ShortName,
                    Digit = item.Digit,
                    TypeId = item.TypeId
                });

                counter++;
            }

            await SyncDB.InsertOrUpdateAllAsync(collection);
        }
    }
}
