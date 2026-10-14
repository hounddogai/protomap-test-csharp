namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global vendor reservation is in its life cycle.</summary>
public enum GlobalVendorReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global vendor reservation as the vendor team keeps it.</summary>
public sealed class GlobalVendorReservation(string code, string label, int priority, string reviewer, int retries, int graceDays, double margin, string segment, string description, int leaveDays, GlobalVendorReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Priority { get; set; } = priority;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public int GraceDays { get; set; } = graceDays;
    public double Margin { get; set; } = margin;
    public string Segment { get; set; } = segment;
    public string Description { get; set; } = description;
    public int LeaveDays { get; set; } = leaveDays;
    public GlobalVendorReservationStatus Status { get; set; } = status;

    /// <summary>Starts a global vendor reservation in status planned.</summary>
    public static GlobalVendorReservation NewGlobalVendorReservation(string code, string label)
    {
        return new GlobalVendorReservation(
            code,
            label,
            0,
            "",
            0,
            0,
            0.0,
            "",
            "",
            0,
            GlobalVendorReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalVendorReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalVendorReservationStatus next)
    {
        switch (Status)
        {
            case GlobalVendorReservationStatus.Planned:
                return next == GlobalVendorReservationStatus.Completed || next == GlobalVendorReservationStatus.Cancelled;
            case GlobalVendorReservationStatus.Scheduled:
                switch (next)
                {
                    case GlobalVendorReservationStatus.InProgress:
                        return true;
                }
                return false;
            case GlobalVendorReservationStatus.InProgress:
                return next == GlobalVendorReservationStatus.Cancelled;
            case GlobalVendorReservationStatus.Completed:
                return next == GlobalVendorReservationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalVendorReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global vendor reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Retries + (double)LeaveDays, 2);
    }

    public GlobalVendorReservation Copy()
    {
        return new GlobalVendorReservation(
            Code,
            Label,
            Priority,
            Reviewer,
            Retries,
            GraceDays,
            Margin,
            Segment,
            Description,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
