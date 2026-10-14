namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor record is in its life cycle.</summary>
public enum VendorRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One vendor record, with what the planners need to act on it.</summary>
public sealed class VendorRecord(string code, string name, string country, int durationMinutes, int updatedAt, string currency, VendorRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Currency { get; set; } = currency;
    public VendorRecordStatus Status { get; set; } = status;

    /// <summary>Starts a vendor record in status draft.</summary>
    public static VendorRecord NewVendorRecord(string code, string name)
    {
        return new VendorRecord(code, name, "", 0, 0, "", VendorRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorRecordStatus next)
    {
        switch (Status)
        {
            case VendorRecordStatus.Draft:
                switch (next)
                {
                    case VendorRecordStatus.Active:
                        return true;
                }
                return false;
            case VendorRecordStatus.Pending:
                switch (next)
                {
                    case VendorRecordStatus.Active:
                    case VendorRecordStatus.Closed:
                        return true;
                }
                return false;
            case VendorRecordStatus.Active:
                switch (next)
                {
                    case VendorRecordStatus.Suspended:
                    case VendorRecordStatus.Closed:
                        return true;
                }
                return false;
            case VendorRecordStatus.Suspended:
                switch (next)
                {
                    case VendorRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VendorRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)UpdatedAt + (double)DurationMinutes, 2);
    }

    public VendorRecord Copy()
    {
        return new VendorRecord(Code, Name, Country, DurationMinutes, UpdatedAt, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
