namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk snapshot is in its life cycle.</summary>
public enum DeskSnapshotStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A desk snapshot. Its code is unique within a tenant.</summary>
public sealed class DeskSnapshot(string code, string label, int limit, double cost, string name, double maximum, string currency, int sequence, int revision, int capacity, DeskSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public double Cost { get; set; } = cost;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public int Sequence { get; set; } = sequence;
    public int Revision { get; set; } = revision;
    public int Capacity { get; set; } = capacity;
    public DeskSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a desk snapshot in status draft.</summary>
    public static DeskSnapshot NewDeskSnapshot(string code, string label)
    {
        return new DeskSnapshot(code, label, 0, 0.0, "", 0.0, "", 0, 0, 0, DeskSnapshotStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskSnapshotStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskSnapshotStatus next)
    {
        switch (Status)
        {
            case DeskSnapshotStatus.Draft:
                return next == DeskSnapshotStatus.Active || next == DeskSnapshotStatus.Suspended;
            case DeskSnapshotStatus.Pending:
                switch (next)
                {
                    case DeskSnapshotStatus.Suspended:
                        return true;
                }
                return false;
            case DeskSnapshotStatus.Active:
                switch (next)
                {
                    case DeskSnapshotStatus.Suspended:
                        return true;
                }
                return false;
            case DeskSnapshotStatus.Suspended:
                return next == DeskSnapshotStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DeskSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"desk snapshot"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Maximum * (double)Sequence, 2);
    }

    public DeskSnapshot Copy()
    {
        return new DeskSnapshot(
            Code,
            Label,
            Limit,
            Cost,
            Name,
            Maximum,
            Currency,
            Sequence,
            Revision,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
