@cd C:\Users\Artem\Desktop\Bot\TelegramBot
@del database.db
@sqlite3.exe database.db ".read script_for_sqlite.sql"
@pause