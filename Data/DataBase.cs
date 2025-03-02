using MySqlConnector;
using PuppeteerSharp;
using TgPetProject.Crypt;
using TgPetProject.Encrypt;

namespace TgPetProject.Data;

public class DataBase
{
    private readonly string
        dbConnectionString = "Server=localhost;Database=tgusersdata;User=root;Password=7853;";

    private readonly byte[] key;

    private readonly byte[] iv;

    private readonly EncryptPassword encryptPassword;

    private readonly AccountService accountService;

    public DataBase(AccountService accountService)
    {
        this.accountService = accountService;
        var (key, iv) = CryptReader.ReadKeyAndIv("");//ваш путь с файлом для шифрования паролей
        this.key = key;
        this.iv = iv;
        encryptPassword = new EncryptPassword(key, iv);
    }


    public async Task SaveUserToDatabase(long chatId, string email, string password)
    {
        await using var connection = new MySqlConnection(dbConnectionString);
        await connection.OpenAsync();
        var hashedPassword = await encryptPassword.Encrypt(password);

        var query = @"
        INSERT INTO users (chatId, email, password) 
        VALUES (@ChatId, @Email, @Password) 
        ON DUPLICATE KEY UPDATE 
        email = VALUES(email), password = VALUES(password)";

        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        command.Parameters.AddWithValue("@Email", email);
        command.Parameters.AddWithValue("@Password", hashedPassword);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsAuthorized(long chatId)
    {
        await using var connection = new MySqlConnection(dbConnectionString);
        await connection.OpenAsync();

        var query = @"SELECT COUNT(*) FROM users WHERE chatId = @ChatId";
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        var count = (long)await command.ExecuteScalarAsync();

        return count > 0;
    }

    public async Task<Page> ReLogin(long chatId)
    {
        await using var connection = new MySqlConnection(dbConnectionString);
        await connection.OpenAsync();

        var query = @"Select email, password from users where chatId = @ChatId";
        await using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@ChatId", chatId);
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var email = reader.GetString(0);
            var password = await encryptPassword.Decrypt(reader.GetString(1));
            var page = await accountService.Login(email, password);
            return page;
        }

        return null;
    }
}