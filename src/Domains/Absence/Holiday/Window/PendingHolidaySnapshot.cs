namespace Acme.Workforce.Absence.Holiday.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending holiday snapshot is in its life cycle.</summary>
public enum PendingHolidaySnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending holiday snapshot. Its code is unique within a tenant.</summary>
public sealed class PendingHolidaySnapshot(string code, string name, double fte, double budget, int priority, string contactEmail, double floorValue, string currency, double rating, double amount, PendingHolidaySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Budget { get; set; } = budget;
    public int Priority { get; set; } = priority;
    public string ContactEmail { get; set; } = contactEmail;
    public double FloorValue { get; set; } = floorValue;
    public string Currency { get; set; } = currency;
    public double Rating { get; set; } = rating;
    public double Amount { get; set; } = amount;
    public PendingHolidaySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a pending holiday snapshot in status new.</summary>
    public static PendingHolidaySnapshot NewPendingHolidaySnapshot(string code, string name)
    {
        return new PendingHolidaySnapshot(
            code,
            name,
            0.0,
            0.0,
            0,
            "",
            0.0,
            "",
            0.0,
            0.0,
            PendingHolidaySnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingHolidaySnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingHolidaySnapshotStatus next)
    {
        switch (Status)
        {
            case PendingHolidaySnapshotStatus.New:
                switch (next)
                {
                    case PendingHolidaySnapshotStatus.Queued:
                        return true;
                }
                return false;
            case PendingHolidaySnapshotStatus.Queued:
                return next == PendingHolidaySnapshotStatus.Running || next == PendingHolidaySnapshotStatus.Done;
            case PendingHolidaySnapshotStatus.Running:
                return next == PendingHolidaySnapshotStatus.Done;
            case PendingHolidaySnapshotStatus.Done:
                return next == PendingHolidaySnapshotStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingHolidaySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending holiday snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Amount - Fte, 2);
    }

    public PendingHolidaySnapshot Copy()
    {
        return new PendingHolidaySnapshot(
            Code,
            Name,
            Fte,
            Budget,
            Priority,
            ContactEmail,
            FloorValue,
            Currency,
            Rating,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
