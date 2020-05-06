using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    class EnumActions
    {
        public enum EActions
        {
            Unknown = -1,
            CreateStorage = 0,
            DeleteStorage = 1,
            RenameStorage = 2,
            ShowStorage = 3,
            MenuStorage = 4,
            GetSharedStorage = 5,
            CreateFolder = 6,
            DeleteFolder = 7,
            RenameFolder = 8,
            ShowFolder = 9,
            ShareStorage = 10,
            Back = 11,
            GetData = 12,
            AddData = 13,
            CreateFolderInFolder = 14,
        }

        public static EActions GetEnumActionFromString(string action)
        {
            EActions res = EActions.Unknown;
            switch (action)
            {
                /*case "Create storage":
                    res = EActions.CreateStorage;
                    break;

                case "Delete storage":
                    res = EActions.DeleteStorage;
                    break;*/

                case "Rename storage":
                    res = EActions.RenameStorage;
                    break;

                case "Show storage's":
                    res = EActions.ShowStorage;
                    break;

                case "Create folder":
                    res = EActions.CreateFolder;
                    break;

                //case "Delete folder":
                //    res = EActions.DeleteFolder;
                //    break;

                case "Rename folder":
                    res = EActions.RenameFolder;
                    break;

                case "Show folders":
                    res = EActions.ShowFolder;
                    break;

                case "Add data":
                    res = EActions.AddData;
                    break;

                case "Get data":
                    res = EActions.GetData;
                    break;

                case "<- Back":
                    res = EActions.Back;
                    break;

                case "Share storage":
                    res = EActions.ShareStorage;
                    break;

                case "Get shared storage":
                    res = EActions.GetSharedStorage;
                    break;

                case "Add folder":
                    res = EActions.CreateFolderInFolder;
                    break;
            }
            return res;
        }

        public static string GetStringFromEAction(EActions action)
        {
            string res = "";
            switch (action)
            {
                case EActions.CreateStorage:
                    res = "Enter new storage name: ";
                    break;

                case EActions.DeleteStorage:
                    res = "Choose storage to delete";
                    break;

                case EActions.RenameStorage:
                    res = "Choose storage to rename";
                    break;

                case EActions.ShowStorage:
                    res = "Choose storage:";
                    break;

                case EActions.CreateFolder:
                case EActions.CreateFolderInFolder:
                    res = "Enter new folder name: ";
                    break;

                case EActions.DeleteFolder:
                    res = "Choose to delete folder";
                    break;

                case EActions.RenameFolder:
                    res = "Choose to rename folder";
                    break;

                case EActions.ShowFolder:
                    res = "Choose folder:";
                    break;

                case EActions.AddData:
                    res = "Add some data\n" +
                        "Please enter the caption, when you add photo(better with extension)";
                    break;

                case EActions.GetData:
                    res = "Choose file";
                    break;

                case EActions.Back:
                    res = "Choose action:";
                    break;

                case EActions.GetSharedStorage:
                    res = "Enter share key: ";
                    break;
            }
            return res;
        }

        public static IState GetStateObjectFromEAction(EActions action)
        {
            IState res = null;
            switch (action)
            {
                case EActions.CreateStorage:
                    res = new CCreateStorageState();
                    break;

                case EActions.DeleteStorage:
                    res = new CDeleteStorageState();
                    break;

                case EActions.RenameStorage:
                    res = new CRenameStorageState();
                    break;

                case EActions.ShowStorage:
                    res = new CShowStorageState();
                    break;

                case EActions.CreateFolder:
                    res = new CCreateState();
                    break;

                case EActions.DeleteFolder:
                    res = new CDeleteState();
                    break;

                case EActions.RenameFolder:
                    res = new CRenameState();
                    break;

                case EActions.ShowFolder:
                    res = new CShowState();
                    break;

                case EActions.AddData:
                    res = new CAddDataState();
                    break;

                case EActions.GetData:
                    res = new CGetDataState();
                    break;

                case EActions.GetSharedStorage:
                    res = new CGetSharedState();
                    break;

                case EActions.CreateFolderInFolder:
                    res = new CCreateFolderInFolderState();
                    break;
            }
            return res;
        }
    }
}
