namespace Acme.Workforce.Contractors.Vendor.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor slot is in its life cycle.</summary>
public enum VendorSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A vendor slot. Its code is unique within a tenant.</summary>
public sealed class VendorSlot(string code, string title, int expiresAt, int tenureMonths, string location, int graceDays, double discount, string locale, string country, int capacity, string currency, VendorSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Location { get; set; } = location;
    public int GraceDays { get; set; } = graceDays;
    public double Discount { get; set; } = discount;
    public string Locale { get; set; } = locale;
    public string Country { get; set; } = country;
    public int Capacity { get; set; } = capacity;
    public string Currency { get; set; } = currency;
    public VendorSlotStatus Status { get; set; } = status;

    /// <summary>Starts a vendor slot in status created.</summary>
    public static VendorSlot NewVendorSlot(string code, string title)
    {
        return new VendorSlot(code, title, 0, 0, "", 0, 0.0, "", "", 0, "", VendorSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorSlotStatus next)
    {
        switch (Status)
        {
            case VendorSlotStatus.Created:
                switch (next)
                {
                    case VendorSlotStatus.Retired:
                        return true;
                }
                return false;
            case VendorSlotStatus.Validated:
                return next == VendorSlotStatus.Published;
            case VendorSlotStatus.Published:
                return next == VendorSlotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(VendorSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount + (double)TenureMonths, 2);
    }

    public VendorSlot Copy()
    {
        return new VendorSlot(
            Code,
            Title,
            ExpiresAt,
            TenureMonths,
            Location,
            GraceDays,
            Discount,
            Locale,
            Country,
            Capacity,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
