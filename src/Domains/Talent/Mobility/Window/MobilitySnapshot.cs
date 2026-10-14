namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility snapshot is in its life cycle.</summary>
public enum MobilitySnapshotStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A mobility snapshot as the mobility team keeps it.</summary>
public sealed class MobilitySnapshot(string code, string name, double fte, double rate, string unit, string notes, int revision, string team, int createdAt, string timezone, string jobCode, MobilitySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Rate { get; set; } = rate;
    public string Unit { get; set; } = unit;
    public string Notes { get; set; } = notes;
    public int Revision { get; set; } = revision;
    public string Team { get; set; } = team;
    public int CreatedAt { get; set; } = createdAt;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public MobilitySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a mobility snapshot in status planned.</summary>
    public static MobilitySnapshot NewMobilitySnapshot(string code, string name)
    {
        return new MobilitySnapshot(code, name, 0.0, 0.0, "", "", 0, "", 0, "", "", MobilitySnapshotStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilitySnapshotStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilitySnapshotStatus next)
    {
        switch (Status)
        {
            case MobilitySnapshotStatus.Planned:
                return next == MobilitySnapshotStatus.Scheduled;
            case MobilitySnapshotStatus.Scheduled:
                return next == MobilitySnapshotStatus.Cancelled;
            case MobilitySnapshotStatus.InProgress:
                return next == MobilitySnapshotStatus.Completed || next == MobilitySnapshotStatus.Cancelled;
            case MobilitySnapshotStatus.Completed:
                return next == MobilitySnapshotStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(MobilitySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Rate * (double)Revision, 2);
    }

    public MobilitySnapshot Copy()
    {
        return new MobilitySnapshot(
            Code,
            Name,
            Fte,
            Rate,
            Unit,
            Notes,
            Revision,
            Team,
            CreatedAt,
            Timezone,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
