using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Schema;
using Telegram.Bot.Types.ReplyMarkups;

namespace Tests
{
    public class Tests
    {
        private static readonly MySqlConnection Conn = TelegramBot.Database.OpenMysqlConnection("mydb");
        
        [Theory]
        [InlineData("testUser")]
        [InlineData("test")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(8)]
        public void testRegistartion(string nameTest)
        {
            
            var result = false;
            
            Conn.Open();
            var sqlStr = "delete from storage where id > 0";
            var command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            sqlStr = "delete from user where id > 0";
            command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            Conn.Close();
            
            
            
            TelegramBot.Funcs.Registration(nameTest, Conn);
            var sqlForCheck = $"Select * FROM User WHERE name = \"{nameTest}\"";
            Conn.Open();
            command = new MySqlCommand(sqlForCheck, Conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                result = true;
            }
            reader.Close();
            Conn.Close();
              
            
            Assert.True(result);
            
            
            Conn.Open();
            sqlStr = "delete from storage where id > 0";
            command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            sqlStr = "delete from user where id > 0";
            command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            Conn.Close();
        }
        
        [Theory]
        [InlineData("testUser")]
        [InlineData("test")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(8)]
        public void testAuthorize(string nameTest)
        {
            var result = TelegramBot.Funcs.Authorize(nameTest, Conn);
            Conn.Open();
            var resTr = $"SELECT * FROM User WHERE name = \"{nameTest}\"";
            var command = new MySqlCommand(resTr, Conn);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                Conn.Close();
                Assert.True(result);
            }
            Conn.Close();
            Assert.False(result);
            
        }
        
        [Fact]
        public void testGetListFolders()
        {
            Conn.Open();
            var sqlStr = "delete from folders where id > 0";
            var command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            sqlStr = "delete from storage where id > 0";
            command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            sqlStr = "delete from user where id > 0";
            command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            Conn.Close();
            
            
            TelegramBot.Funcs.Registration("test", Conn);
            var idUser = Convert.ToInt32(TelegramBot.Database.MysqlSelect($"SELECT id FROM User WHERE name = \"test\"", Conn));
            var idStorage = Convert.ToInt32(TelegramBot.Database.MysqlSelect($"SELECT id FROM storage WHERE idUser = {idUser}", Conn));
            TelegramBot.Database.MysqlDeleteOrInsert($"INSERT INTO Folders (idStorage, Name) VALUES({idStorage}, \"testFolder1\");", Conn);
            TelegramBot.Database.MysqlDeleteOrInsert($"INSERT INTO Folders (idStorage, Name) VALUES({idStorage}, \"testFolder2\");", Conn);
            TelegramBot.Database.MysqlDeleteOrInsert($"INSERT INTO Folders (idStorage, Name) VALUES({idStorage}, \"testFolder3\");", Conn);
            
            
            var result = TelegramBot.Funcs.GetListFolders(-1, Conn);
            var res = new List<string> {"<- Back","testFolder1", "testFolder2", "testFolder3"};
            
            Assert.Equal(result, res);
            
            Conn.Open();
            sqlStr = "delete from folders where id > 0";
            command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            sqlStr = "delete from storage where id > 0";
            command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            sqlStr = "delete from user where id > 0";
            command = new MySqlCommand(sqlStr, Conn);
            command.ExecuteNonQuery();
            Conn.Close();
            
        }

        [Theory]
        [MemberData(nameof(Data))]
        /*[InlineData("1")]
        [InlineData("")]
        [InlineData("1", "2", "3", "4")]
        [InlineData("1", "2", "3", "4", "5")]
        [InlineData("1", "2", "3", "1", "2", "3", "1", "2", "3", "1", "2", "3")]*/

        public void testGetInlineKeyboard(List<string> lst)
        {
            var result = TelegramBot.Funcs.GetInlineKeyboard(lst);
            var res = new InlineKeyboardButton[1][];
            InlineKeyboardButton[] keyboardButton = new InlineKeyboardButton[3];
            keyboardButton[0] = InlineKeyboardButton.WithCallbackData("1");
            keyboardButton[1] = InlineKeyboardButton.WithCallbackData("2");
            keyboardButton[2] = InlineKeyboardButton.WithCallbackData("3");
            res[0] = keyboardButton;
            var flag = true;
            var i = 0;
            foreach (var item in result)
            {
                
                if (item[i].Text != res[0][i].Text)
                    flag = false;
                i++;
            }
            Assert.True(flag);
        }
        
        
        
        public static List<object[]> Data =>
            new List<object[]>
            {
                new object[]{ new List<string> {"1", "2", "3" }}
            };
        
        [Fact]
        public void testGetShareKey()
        {
            
            Assert.False(false);
            
        }
        
        [Fact]
        public void testGetNewShareKey()
        {
            
            Assert.False(false);
            
        }
        
        [Theory]
        [InlineData("testUser")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(8)]
        public void testGetIdStorageFromIdUser(string nameTest)
        {
            
            Assert.False(false);
            
        }
        
        [Theory]
        [InlineData("testUser")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(8)]
        public void testGetIdUserFromUsername(string nameTest)
        {
            
            Assert.False(false);
            
        }
        
        [Fact]
        public void testGetListFiles()
        {
            Assert.True(true);
            
        }
        
        [Fact]
        public void testGetListStorages()
        {
            Assert.True(true);
            
        }
        
        [Theory]
        [InlineData("testUser")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(8)]
        public void testOpenMysqlConnection(string nameTest)
        {
            
            Assert.False(false);
            
        }
        
        [Theory]
        [InlineData("testUser")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(8)]
        public void testMysqlSelect(string nameTest)
        {
            
            Assert.False(false);
            
        }
        
        [Theory]
        [InlineData("testUser")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(8)]
        public void testMysqlDeleteOrInsert(string nameTest)
        {
            
            Assert.False(false);
            
        }
        
    }
}