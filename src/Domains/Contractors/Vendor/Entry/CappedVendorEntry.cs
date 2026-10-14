namespace Acme.Workforce.Contractors.Vendor.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped vendor entry is in its life cycle.</summary>
public enum CappedVendorEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One capped vendor entry, with what the planners need to act on it.</summary>
public sealed class CappedVendorEntry(string code, string name, double cost, int leaveDays, string locale, string contactEmail, int graceDays, CappedVendorEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public int LeaveDays { get; set; } = leaveDays;
    public string Locale { get; set; } = locale;
    public string ContactEmail { get; set; } = contactEmail;
    public int GraceDays { get; set; } = graceDays;
    public CappedVendorEntryStatus Status { get; set; } = status;

    /// <summary>Starts a capped vendor entry in status draft.</summary>
    public static CappedVendorEntry NewCappedVendorEntry(string code, string name)
    {
        return new CappedVendorEntry(code, name, 0.0, 0, "", "", 0, CappedVendorEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedVendorEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedVendorEntryStatus next)
    {
        switch (Status)
        {
            case CappedVendorEntryStatus.Draft:
                switch (next)
                {
                    case CappedVendorEntryStatus.Active:
                    case CappedVendorEntryStatus.Suspended:
                        return true;
                }
                return false;
            case CappedVendorEntryStatus.Pending:
                switch (next)
                {
                    case CappedVendorEntryStatus.Suspended:
                        return true;
                }
                return false;
            case CappedVendorEntryStatus.Active:
                switch (next)
                {
                    case CappedVendorEntryStatus.Suspended:
                    case CappedVendorEntryStatus.Closed:
                        return true;
                }
                return false;
            case CappedVendorEntryStatus.Suspended:
                return next == CappedVendorEntryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(CappedVendorEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped vendor entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)GraceDays + (double)LeaveDays, 2);
    }

    public CappedVendorEntry Copy()
    {
        return new CappedVendorEntry(Code, Name, Cost, LeaveDays, Locale, ContactEmail, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
