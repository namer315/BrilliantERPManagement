using System;

namespace WhatsAppData.Search;

public class WhatsAppTenantSH
{
    public string? TenantName { get; set; }
    public string? ContactPhone { get; set; }
    public int Status { get; set; } // 0 = All, 1 = Active, 2 = Inactive
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
