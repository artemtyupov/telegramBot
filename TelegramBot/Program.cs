using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotFS
{
    class Program
    {
        static void Main(string[] args)
        {
            DokanNet.Dokan.Unmount('M');
            DokanNet.Dokan.Mount(new TelegramBotFSClass(), "M:\\");
        }
    }
}
