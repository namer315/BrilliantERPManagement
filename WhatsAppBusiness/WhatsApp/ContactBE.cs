using CommonData.Managers;
using WhatsAppData.DAO;
using WhatsAppData.DTO.Common;
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

    private async Task<ContactVO> GetNew(string waId , string Name = null)
    {
        ContactVO contact = new ContactVO();

        //contact.PhoneNumber = phoneNumber;
        contact.WaId = waId;

        return contact;
    }

    public async Task<IList<ContactDTO>> GetChatsContactList()
    {
        if (!TenantManager.IskeyExist)
            throw new InvalidOperationException("Tenant key does not exist.");

        if (TenantManager.CurrentTenant is null)
            throw new InvalidOperationException("Current tenant is not set.");

        IList<ContactVO> rawData = await _dao.GetChatListContacts();

        IList<ContactDTO> contactDTOList = rawData
            .Select(x => new ContactDTO()
            {
                Id = x.Id ,
                WaId = x.WaId
            })
            .ToList();

        return contactDTOList;
    }
}
