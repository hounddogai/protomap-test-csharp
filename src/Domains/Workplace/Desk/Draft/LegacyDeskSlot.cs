namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy desk slot is in its life cycle.</summary>
public enum LegacyDeskSlotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy desk slot as the desk team keeps it.</summary>
public sealed class LegacyDeskSlot(string code, string title, string notes, double margin, bool visible, string country, string phone, int tenureMonths, LegacyDeskSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public double Margin { get; set; } = margin;
    public bool Visible { get; set; } = visible;
    public string Country { get; set; } = country;
    public string Phone { get; set; } = phone;
    public int TenureMonths { get; set; } = tenureMonths;
    public LegacyDeskSlotStatus Status { get; set; } = status;

    /// <summary>Starts a legacy desk slot in status proposed.</summary>
    public static LegacyDeskSlot NewLegacyDeskSlot(string code, string title)
    {
        return new LegacyDeskSlot(code, title, "", 0.0, false, "", "", 0, LegacyDeskSlotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyDeskSlotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyDeskSlotStatus next)
    {
        switch (Status)
        {
            case LegacyDeskSlotStatus.Proposed:
                return next == LegacyDeskSlotStatus.Rejected;
            case LegacyDeskSlotStatus.Approved:
                return next == LegacyDeskSlotStatus.Rejected || next == LegacyDeskSlotStatus.Withdrawn;
            case LegacyDeskSlotStatus.Rejected:
                return next == LegacyDeskSlotStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyDeskSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy desk slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Margin * (double)TenureMonths, 2);
    }

    public LegacyDeskSlot Copy()
    {
        return new LegacyDeskSlot(Code, Title, Notes, Margin, Visible, Country, Phone, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
