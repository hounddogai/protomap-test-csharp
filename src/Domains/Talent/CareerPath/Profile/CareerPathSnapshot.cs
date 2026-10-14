namespace Acme.Workforce.Talent.CareerPath.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path snapshot is in its life cycle.</summary>
public enum CareerPathSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A career path snapshot. Its code is unique within a tenant.</summary>
public sealed class CareerPathSnapshot(string code, string label, string name, int windowDays, string channel, string currency, string unit, int capacity, string region, CareerPathSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Channel { get; set; } = channel;
    public string Currency { get; set; } = currency;
    public string Unit { get; set; } = unit;
    public int Capacity { get; set; } = capacity;
    public string Region { get; set; } = region;
    public CareerPathSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a career path snapshot in status draft.</summary>
    public static CareerPathSnapshot NewCareerPathSnapshot(string code, string label)
    {
        return new CareerPathSnapshot(code, label, "", 0, "", "", "", 0, "", CareerPathSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathSnapshotStatus next)
    {
        switch (Status)
        {
            case CareerPathSnapshotStatus.Draft:
                return next == CareerPathSnapshotStatus.Suspended;
            case CareerPathSnapshotStatus.Pending:
                return next == CareerPathSnapshotStatus.Active || next == CareerPathSnapshotStatus.Suspended;
            case CareerPathSnapshotStatus.Active:
                switch (next)
                {
                    case CareerPathSnapshotStatus.Suspended:
                    case CareerPathSnapshotStatus.Closed:
                        return true;
                }
                return false;
            case CareerPathSnapshotStatus.Suspended:
                switch (next)
                {
                    case CareerPathSnapshotStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)WindowDays + (double)Capacity, 2);
    }

    public CareerPathSnapshot Copy()
    {
        return new CareerPathSnapshot(
            Code,
            Label,
            Name,
            WindowDays,
            Channel,
            Currency,
            Unit,
            Capacity,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
