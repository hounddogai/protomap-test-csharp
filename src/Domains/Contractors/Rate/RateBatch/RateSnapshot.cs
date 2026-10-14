namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate snapshot is in its life cycle.</summary>
public enum RateSnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A rate snapshot as the rate team keeps it.</summary>
public sealed class RateSnapshot(string code, string name, int windowDays, string notes, string region, RateSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Notes { get; set; } = notes;
    public string Region { get; set; } = region;
    public RateSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a rate snapshot in status proposed.</summary>
    public static RateSnapshot NewRateSnapshot(string code, string name)
    {
        return new RateSnapshot(code, name, 0, "", "", RateSnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateSnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateSnapshotStatus next)
    {
        switch (Status)
        {
            case RateSnapshotStatus.Proposed:
                switch (next)
                {
                    case RateSnapshotStatus.Approved:
                        return true;
                }
                return false;
            case RateSnapshotStatus.Approved:
                switch (next)
                {
                    case RateSnapshotStatus.Rejected:
                    case RateSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
            case RateSnapshotStatus.Rejected:
                switch (next)
                {
                    case RateSnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rate snapshot"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public RateSnapshot Copy()
    {
        return new RateSnapshot(Code, Name, WindowDays, Notes, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
