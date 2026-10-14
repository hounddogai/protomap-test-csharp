namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle checkpoint is in its life cycle.</summary>
public enum MeritCycleCheckpointStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A merit cycle checkpoint as the merit cycle team keeps it.</summary>
public sealed class MeritCycleCheckpoint(string code, string name, string category, string locale, int limit, int createdAt, bool approved, string source, string department, string unit, MeritCycleCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Locale { get; set; } = locale;
    public int Limit { get; set; } = limit;
    public int CreatedAt { get; set; } = createdAt;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public string Department { get; set; } = department;
    public string Unit { get; set; } = unit;
    public MeritCycleCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle checkpoint in status proposed.</summary>
    public static MeritCycleCheckpoint NewMeritCycleCheckpoint(string code, string name)
    {
        return new MeritCycleCheckpoint(
            code,
            name,
            "",
            "",
            0,
            0,
            false,
            "",
            "",
            "",
            MeritCycleCheckpointStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleCheckpointStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleCheckpointStatus next)
    {
        switch (Status)
        {
            case MeritCycleCheckpointStatus.Proposed:
                switch (next)
                {
                    case MeritCycleCheckpointStatus.Approved:
                    case MeritCycleCheckpointStatus.Withdrawn:
                        return true;
                }
                return false;
            case MeritCycleCheckpointStatus.Approved:
                return next == MeritCycleCheckpointStatus.Rejected || next == MeritCycleCheckpointStatus.Withdrawn;
            case MeritCycleCheckpointStatus.Rejected:
                return next == MeritCycleCheckpointStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(MeritCycleCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"merit cycle checkpoint"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Limit - (double)CreatedAt, 2);
    }

    public MeritCycleCheckpoint Copy()
    {
        return new MeritCycleCheckpoint(
            Code,
            Name,
            Category,
            Locale,
            Limit,
            CreatedAt,
            Approved,
            Source,
            Department,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
