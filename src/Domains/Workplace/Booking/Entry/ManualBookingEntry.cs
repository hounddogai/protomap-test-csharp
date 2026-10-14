namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual booking entry is in its life cycle.</summary>
public enum ManualBookingEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual booking entry. Its code is unique within a tenant.</summary>
public sealed class ManualBookingEntry(string code, string title, string timezone, string costCenter, string location, string checksum, bool visible, double amount, ManualBookingEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public string CostCenter { get; set; } = costCenter;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public bool Visible { get; set; } = visible;
    public double Amount { get; set; } = amount;
    public ManualBookingEntryStatus Status { get; set; } = status;

    /// <summary>Starts a manual booking entry in status created.</summary>
    public static ManualBookingEntry NewManualBookingEntry(string code, string title)
    {
        return new ManualBookingEntry(code, title, "", "", "", "", false, 0.0, ManualBookingEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBookingEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBookingEntryStatus next)
    {
        switch (Status)
        {
            case ManualBookingEntryStatus.Created:
                return next == ManualBookingEntryStatus.Validated || next == ManualBookingEntryStatus.Retired;
            case ManualBookingEntryStatus.Validated:
                return next == ManualBookingEntryStatus.Retired;
            case ManualBookingEntryStatus.Published:
                switch (next)
                {
                    case ManualBookingEntryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualBookingEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual booking entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"manual booking entry"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ManualBookingEntry Copy()
    {
        return new ManualBookingEntry(
            Code,
            Title,
            Timezone,
            CostCenter,
            Location,
            Checksum,
            Visible,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
