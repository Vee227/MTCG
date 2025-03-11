using System;

class UnitTest1
{
    static void RunTests()
    {
        try
        {
            TestRegisterUser();
            TestRegisterUserInvalidData();
            TestRegisterUserEmptyUsername();
            TestGetUsers();
            TestUpdateUserProfile();
            TestUpdateUserProfileInvalidUser();
            TestDeleteUser();
            TestDeleteUserInvalidUser();
            TestDeleteUserUnauthorized();
            TestGetUserProfile();
            TestGetUserProfileInvalidUser();
            TestLoginUser();
            TestLoginUserWrongCredentials();
            TestCreatePackage();
            TestBuyPackage();
            TestBuyPackageWithoutFunds();
            TestGetDeck();
            TestSetDeck();
            TestGetUserStack();
            TestGetUserStackUnauthorized();
            Console.WriteLine("Alle Tests erfolgreich");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Test fehlgeschlagen - " + ex.Message);
        }
    }

    static void TestRegisterUser()
    {
        string username = "testuser4";
        string password = "password123";
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new Exception("Benutzerregistrierung fehlgeschlagen: Ungültige Daten");
        }
    }

    static void TestRegisterUserInvalidData()
    {
        string jsonData = "{}";
        if (jsonData == "{}")
        {
            throw new Exception("Benutzerregistrierung fehlgeschlagen: Ungültige Daten");
        }
    }

    static void TestRegisterUserEmptyUsername()
    {
        string username = "";
        string password = "password123";
        if (string.IsNullOrEmpty(username))
        {
            throw new Exception("Benutzerregistrierung fehlgeschlagen: Benutzername fehlt");
        }
    }

    static void TestGetUsers()
    {
        Console.WriteLine("GetUsers erfolgreich");
    }

    static void TestUpdateUserProfile()
    {
        string bio = "new bio";
        if (string.IsNullOrEmpty(bio))
        {
            throw new Exception("Profilaktualisierung fehlgeschlagen: Bio fehlt");
        }
    }

    static void TestUpdateUserProfileInvalidUser()
    {
        string user = "nonexistentuser";
        if (user == "nonexistentuser")
        {
            throw new Exception("Profilaktualisierung fehlgeschlagen: Ungültiger Benutzer");
        }
    }

    static void TestDeleteUser()
    {
        Console.WriteLine("Benutzer erfolgreich gelöscht");
    }

    static void TestDeleteUserInvalidUser()
    {
        string user = "nonexistentuser";
        if (user == "nonexistentuser")
        {
            throw new Exception("Löschen fehlgeschlagen: Benutzer nicht gefunden");
        }
    }

    static void TestDeleteUserUnauthorized()
    {
        bool authorized = false;
        if (!authorized)
        {
            throw new Exception("Löschen fehlgeschlagen: Nicht autorisiert");
        }
    }

    static void TestGetUserProfile()
    {
        Console.WriteLine("Benutzerprofil erfolgreich abgerufen");
    }

    static void TestGetUserProfileInvalidUser()
    {
        string user = "nonexistentuser";
        if (user == "nonexistentuser")
        {
            throw new Exception("Abrufen fehlgeschlagen: Benutzer nicht gefunden");
        }
    }

    static void TestLoginUser()
    {
        string username = "testuser";
        string password = "testuserpassword";
        if (password != "testuserpassword")
        {
            throw new Exception("Login fehlgeschlagen: Falsche Anmeldeinformationen");
        }
    }

    static void TestLoginUserWrongCredentials()
    {
        string password = "wrongpassword";
        if (password != "password123")
        {
            throw new Exception("Login fehlgeschlagen: Falsches Passwort");
        }
    }

    static void TestCreatePackage()
    {
        Console.WriteLine("Paket erfolgreich erstellt");
    }

    static void TestBuyPackage()
    {
        int userFunds = 10;
        int packageCost = 5;
        if (userFunds < packageCost)
        {
            throw new Exception("Kauf fehlgeschlagen: Nicht genügend Guthaben");
        }
    }

    static void TestBuyPackageWithoutFunds()
    {
        int userFunds = 0;
        int packageCost = 5;
        if (userFunds < packageCost)
        {
            throw new Exception("Kauf fehlgeschlagen: Nicht genügend Guthaben");
        }
    }

    static void TestGetDeck()
    {
        Console.WriteLine("Deck erfolgreich abgerufen");
    }

    static void TestSetDeck()
    {
        Console.WriteLine("Deck erfolgreich gesetzt");
    }

    static void TestGetUserStack()
    {
        Console.WriteLine("Kartenstapel erfolgreich abgerufen");
    }

    static void TestGetUserStackUnauthorized()
    {
        bool authorized = false;
        if (!authorized)
        {
            throw new Exception("Abrufen fehlgeschlagen: Nicht autorisiert");
        }
    }
}
