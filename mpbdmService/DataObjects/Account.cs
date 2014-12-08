using Microsoft.WindowsAzure.Mobile.Service;
using mpbdmService.DataObjects;
public class Account : EntityData
{
    public string Username { get; set; }
    public byte[] Salt { get; set; }
    public byte[] SaltedAndHashedPassword { get; set; }

    public virtual Users User {get;set;}
}