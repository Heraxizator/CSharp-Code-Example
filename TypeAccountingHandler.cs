using DvgupaMobile.DAL.SQLite.Models.Services.Statement;
using DvgupsMobile.DependencyServices;
using DvgupsMobile.ObservableItems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DvgupsMobile.Services.StatementHandlers
{
    public static class TypeAccountingHandler
    {
        static IToastService ToastService = DependencyService.Get<IToastService>();
        public static List<string> GetTypesStrings(List<TypeAccountingItem> list)
        {
            var collection = new List<string>();

            foreach (var item in list)
            {
                collection.Add(item.FullName);
            }

            return collection;

        }

        public static async Task<List<TypeAccountingItem>> GetAccountingType()
        {
            var collection = new List<TypeAccountingItem>();

            try
            {
                var json_out = await ConnectivityHandler.POST_Async("GetTypeAccounting");

                if (string.IsNullOrEmpty(json_out))
                {
                    return collection;
                }

                var dataset = JsonConvert.DeserializeObject<DataSet>(json_out);

                var types = dataset.Tables["types"];

                foreach (DataRow type in types.Rows)
                {
                    var type_state = type["TYPEACCOUNTING_STATE"].ToString();

                    if (type_state != "1")
                    {
                        continue;
                    }

                    var type_id = int.Parse(type["TYPEACCOUNTING_ID"].ToString());

                    var short_name = type["TYPEACCOUNTING_FULLNAME"].ToString();

                    var long_name = type["TYPEACCOUNTING_FULLNAME"].ToString();

                    var item = new TypeAccountingItem
                    {
                        Id = type_id,
                        ShortName = short_name,
                        FullName = long_name
                    };

                    collection.Add(item);
                }
            }

            catch (Exception ex)
            {
                ToastService.ShortAlert("Ошибка при получении типа");
                LogHandler.Error(ex, nameof(TypeAccountingHandler));
            }

            return collection;
        }

        public static async Task<List<TypeAccountingItem>> GetTypes()
        {
            var list = new List<TypeAccountingItem>();

            var items = await SyncDB.GetListAsync<STNAccountingType>();

            foreach (var item in items)
            {
                list.Add(new TypeAccountingItem
                {
                    Id = item.GlobalId,
                    FullName = item.LongName,
                    ShortName = item.ShortName
                });
            }

            return list;
        }

        public static async Task SaveAccauntingTypes()
        {
            var list = await TypeAccountingHandler.GetAccountingType();

            var collection = new List<STNAccountingType>();

            var counter = 0;

            foreach (var item in list)
            {
                collection.Add(new STNAccountingType
                {
                    GlobalId = item.Id,
                    LocalId = counter,
                    LongName = item.FullName,
                    ShortName = item.ShortName
                });

                counter++;
            }

            await SyncDB.InsertOrUpdateAllAsync(collection);
        }
    }
}
