namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle bundle is in its life cycle.</summary>
public enum MeritCycleBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a merit cycle bundle and its current status.</summary>
public sealed class MeritCycleBundle(string code, string name, string title, string department, bool archived, bool approved, int createdAt, int priority, double maximum, string skill, double budget, MeritCycleBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public bool Archived { get; set; } = archived;
    public bool Approved { get; set; } = approved;
    public int CreatedAt { get; set; } = createdAt;
    public int Priority { get; set; } = priority;
    public double Maximum { get; set; } = maximum;
    public string Skill { get; set; } = skill;
    public double Budget { get; set; } = budget;
    public MeritCycleBundleStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle bundle in status planned.</summary>
    public static MeritCycleBundle NewMeritCycleBundle(string code, string name)
    {
        return new MeritCycleBundle(
            code,
            name,
            "",
            "",
            false,
            false,
            0,
            0,
            0.0,
            "",
            0.0,
            MeritCycleBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleBundleStatus next)
    {
        switch (Status)
        {
            case MeritCycleBundleStatus.Planned:
                switch (next)
                {
                    case MeritCycleBundleStatus.InProgress:
                    case MeritCycleBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case MeritCycleBundleStatus.Scheduled:
                return next == MeritCycleBundleStatus.InProgress || next == MeritCycleBundleStatus.Completed;
            case MeritCycleBundleStatus.InProgress:
                return next == MeritCycleBundleStatus.Completed || next == MeritCycleBundleStatus.Cancelled;
            case MeritCycleBundleStatus.Completed:
                return next == MeritCycleBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(MeritCycleBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Maximum * (double)CreatedAt, 2);
    }

    public MeritCycleBundle Copy()
    {
        return new MeritCycleBundle(
            Code,
            Name,
            Title,
            Department,
            Archived,
            Approved,
            CreatedAt,
            Priority,
            Maximum,
            Skill,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
