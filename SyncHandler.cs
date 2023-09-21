using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DvgupsMobile.Services.StatementHandlers
{
    public static class SyncHandler
    {
        public static void SaveAll()
        {
            Parallel.Invoke
            (
                async () => await MarkHandler.SaveMarks(),

                async () => await TypeAccountingHandler.SaveAccauntingTypes()
             );

        }
    }
}
