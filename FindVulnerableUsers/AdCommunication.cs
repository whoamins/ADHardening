using LdapForNet;
using LdapForNet.Native;


namespace Find_Asreproastable_Users;

public class AdCommunication
{
    // TODO: Use only one connection to DC

    private static string? Domain { get; set; }
    private static string? Username { get; set; }
    private static string? Password { get; set; }
    
    public AdCommunication(string? domain, string? username, string? password)
    {
        Domain = domain;
        Username = username;
        Password = password;
    }

    /// <summary>
    /// Returns connection
    /// </summary>
    /// <returns></returns>
    private static LdapConnection? Connect()
    {
        var cn = new LdapConnection();

        if (Domain?.Split(".").Length == 1)
        {
            Console.WriteLine("The domain must have TLD\nTrying to add '.com'\n");
            Domain += ".com";
        }

        try
        {
            cn.Connect(new Uri($"LDAP://{Domain}"));
            cn.Bind(userDn: Username, password: Password);
        }
        catch (LdapInvalidCredentialsException)
        {
            Console.WriteLine("Invalid Credentials!");
            return null;
        }
        catch (LdapException)
        {
            Console.WriteLine("Can't connect to this domain!");
            return null;
        }

        return cn;
    }

    /// <summary>
    /// Prints vulnerable users
    /// </summary>
    /// <returns></returns>
    public static bool ShowVulnerableUsers()
    {
        var entries = GetVulnerableUsers2();

        if (entries == null)
        {
            Console.WriteLine("I haven't found any asreproastable users");
            return false;
        }
        
        foreach (var e in entries)
        {
            Console.WriteLine(e.DirectoryAttributes["CN"].GetValue<string>());
        }

        return true;
    }

    /// <summary>
    /// Returns users vulnerable to asreproasting attack
    /// </summary>
    /// <param name="cn">LDAP Connection</param>
    private static IList<LdapEntry>? GetAsreprostableUsers(LdapConnection? cn = null)
    {
        cn ??= Connect();

        var splittedDomain = Domain?.Split('.');
        IList<LdapEntry>? entries = new List<LdapEntry>();

        switch (splittedDomain?.Length)
        {
            case 3:
                entries = cn?.Search($"dc={splittedDomain[0]},dc={splittedDomain[1]},dc={splittedDomain[2]}",
                    "(&(UserAccountControl:1.2.840.113556.1.4.803:=4194304))");
                break;
            case 2:
                entries = cn?.Search($"dc={splittedDomain[0]},dc={splittedDomain[1]}",
                    "(&(UserAccountControl:1.2.840.113556.1.4.803:=4194304))");
                break;
        }
        
        if (entries == null || entries.Count == 0) return null;

        return entries;
    }

    /// <summary>
    /// Returns users vulnerable to kerberoasting attack
    /// </summary>
    /// <param name="cn">LDAP Connection</param>
    private static IList<LdapEntry>? GetKerberoastableUsers(LdapConnection? cn = null)
    {
        cn ??= Connect();

        var splittedDomain = Domain?.Split('.');
        IList<LdapEntry>? entries = new List<LdapEntry>();

        switch (splittedDomain?.Length)
        {
            case 3:
                entries = cn?.Search($"dc={splittedDomain[0]},dc={splittedDomain[1]},dc={splittedDomain[2]}",
                    "(&(samAccountType=805306368) (servicePrincipalName=*))");
                break;
            case 2:
                entries = cn?.Search($"dc={splittedDomain[0]},dc={splittedDomain[1]}",
                    "(&(samAccountType=805306368) (servicePrincipalName=*))");
                break;
        }
        
        if (entries == null || entries.Count == 0) return null;

        return entries;
    }

    /// <summary>
    /// Changes userAccountControl to 512 (NORMAL_ACCOUNT)
    /// </summary>
    public static void ChangeSomeProperties()
    {
        using var cn = Connect();

        var vulnerableUsers = GetKerberoastableUsers(cn);

        if (vulnerableUsers == null) return;
        
        foreach (var user in vulnerableUsers)
        {
            cn?.Modify(new LdapModifyEntry
            {
                Dn = user.Dn,
                Attributes = new List<LdapModifyAttribute>
                {
                    new()
                    {
                        LdapModOperation = Native.LdapModOperation.LDAP_MOD_REPLACE,
                        Type = "userAccountControl",
                        Values = new List<string> {"512"}
                    }
                }
            });
        }
    }
}