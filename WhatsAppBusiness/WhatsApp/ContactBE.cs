using WhatsAppData.DAO;
using WhatsAppData.VO.WhatsApp;

namespace WhatsAppBusiness.WhatsApp;

public class ContactBE
{
    private ContactDAO _dao = new ContactDAO();
    public async Task<ContactVO> GetContactBy(string waId)
    {
        ContactVO contact = await _dao.GetContactBy(waId);
        if (contact is null)
        {
            contact = await GetNew(waId);
            string s = await Persist(contact);
        }

        return contact;
    }

    public async Task<string> Persist(ContactVO contact)
    {
        Validation(contact);

        return await _dao.PersistAsync(contact);
    }

    private void Validation(ContactVO contact)
    {

    }

    private async Task<ContactVO?> GetNew(string waId , string Name = null)
    {
        ContactVO contact = new ContactVO();

        //contact.PhoneNumber = phoneNumber;
        contact.WaId = waId;

        return contact;
    }
}
