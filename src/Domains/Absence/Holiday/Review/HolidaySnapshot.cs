namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday snapshot is in its life cycle.</summary>
public enum HolidaySnapshotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A holiday snapshot. Its code is unique within a tenant.</summary>
public sealed class HolidaySnapshot(string code, string name, string country, double discount, bool approved, double threshold, double hours, int quantity, HolidaySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Discount { get; set; } = discount;
    public bool Approved { get; set; } = approved;
    public double Threshold { get; set; } = threshold;
    public double Hours { get; set; } = hours;
    public int Quantity { get; set; } = quantity;
    public HolidaySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a holiday snapshot in status planned.</summary>
    public static HolidaySnapshot NewHolidaySnapshot(string code, string name)
    {
        return new HolidaySnapshot(code, name, "", 0.0, false, 0.0, 0.0, 0, HolidaySnapshotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidaySnapshotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidaySnapshotStatus next)
    {
        switch (Status)
        {
            case HolidaySnapshotStatus.Planned:
                return next == HolidaySnapshotStatus.InProgress || next == HolidaySnapshotStatus.Cancelled;
            case HolidaySnapshotStatus.Scheduled:
                switch (next)
                {
                    case HolidaySnapshotStatus.Cancelled:
                        return true;
                }
                return false;
            case HolidaySnapshotStatus.InProgress:
                return next == HolidaySnapshotStatus.Completed;
            case HolidaySnapshotStatus.Completed:
                return next == HolidaySnapshotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(HolidaySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Discount + Threshold, 2);
    }

    public HolidaySnapshot Copy()
    {
        return new HolidaySnapshot(Code, Name, Country, Discount, Approved, Threshold, Hours, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
