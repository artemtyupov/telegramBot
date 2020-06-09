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
            DeleteData = 15,
        }

        public static EActions GetEnumActionFromString(string action)
        {
            EActions res = EActions.Unknown;
            switch (action)
            {
                case "Переименовать хранилище":
                    res = EActions.RenameStorage;
                    break;

                case "Показать хранилища":
                    res = EActions.ShowStorage;
                    break;

                case "Переименовать папку":
                    res = EActions.RenameFolder;
                    break;

                case "Показать папки":
                    res = EActions.ShowFolder;
                    break;

                case "Получение файла":
                    res = EActions.GetData;
                    break;

                case "Назад":
                    res = EActions.Back;
                    break;

                case "Поделиться хранилищем":
                    res = EActions.ShareStorage;
                    break;

                case "Получение приватного хранилища":
                    res = EActions.GetSharedStorage;
                    break;

                case "Создание папки":
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
                    res = "Введите название нового хранилища:";
                    break;

                case EActions.DeleteStorage:
                    res = "Выберите хранилище для удаления:";
                    break;

                case EActions.RenameStorage:
                    res = "Выберите хранилище для переименования:";
                    break;

                case EActions.ShowStorage:
                    res = "Выберите хранилище:";
                    break;

                case EActions.CreateFolder:
                case EActions.CreateFolderInFolder:
                    res = "Выберите новое имя папки:";
                    break;

                case EActions.DeleteFolder:
                    res = "Выберите папку для удаления:";
                    break;

                case EActions.RenameFolder:
                    res = "Выберите папку для переименования:";
                    break;

                case EActions.ShowFolder:
                    res = "Выберите папку:";
                    break;

                case EActions.AddData:
                    res = "Добавьте файл\n" +
                        "Пожалуйста введите название файла с расширением в подпись";
                    break;

                case EActions.GetData:
                    res = "Выберите файл";
                    break;

                case EActions.Back:
                    res = "Выберите действие:";
                    break;

                case EActions.GetSharedStorage:
                    res = "Введите ключ приватного хранилища:";
                    break;

                case EActions.DeleteData:
                    res = "Выберите файл для удаления:";
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

                case EActions.DeleteData:
                    res = new CDeleteDataState();
                    break;
            }
            return res;
        }
    }
}
