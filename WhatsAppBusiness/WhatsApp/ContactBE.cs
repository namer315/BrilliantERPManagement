using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class ContactBE
{
    private ContactDAO _dao = new ContactDAO();
    public async Task<ContactVO> GetContactBy(string phoneNumber)
    {
        ContactVO contact = await _dao.GetContactBy(phoneNumber);
        if (contact is null)
        {
            contact = await GetNew(phoneNumber);
            string s = await Persist(contact);
        }

        return contact;
    }

    private async Task<string> Persist(ContactVO contact)
    {


        return await _dao.PersistAsync(contact);
    }

    private async Task<ContactVO?> GetNew(string phoneNumber , string Name = null)
    {
        ContactVO contact = new ContactVO();

        contact.PhoneNumber = phoneNumber;

        return contact;
    }
}
