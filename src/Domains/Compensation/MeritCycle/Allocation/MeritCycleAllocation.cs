namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle allocation is in its life cycle.</summary>
public enum MeritCycleAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a merit cycle allocation and its current status.</summary>
public sealed class MeritCycleAllocation(string code, string title, int createdAt, string checksum, int windowDays, List<string> tags, double cost, int expiresAt, int version, string team, string country, MeritCycleAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Checksum { get; set; } = checksum;
    public int WindowDays { get; set; } = windowDays;
    public List<string> Tags { get; set; } = tags;
    public double Cost { get; set; } = cost;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Version { get; set; } = version;
    public string Team { get; set; } = team;
    public string Country { get; set; } = country;
    public MeritCycleAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle allocation in status open.</summary>
    public static MeritCycleAllocation NewMeritCycleAllocation(string code, string title)
    {
        return new MeritCycleAllocation(
            code,
            title,
            0,
            "",
            0,
            new List<string>(),
            0.0,
            0,
            0,
            "",
            "",
            MeritCycleAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleAllocationStatus next)
    {
        switch (Status)
        {
            case MeritCycleAllocationStatus.Open:
                switch (next)
                {
                    case MeritCycleAllocationStatus.InReview:
                        return true;
                }
                return false;
            case MeritCycleAllocationStatus.InReview:
                return next == MeritCycleAllocationStatus.Resolved;
            case MeritCycleAllocationStatus.Resolved:
                return next == MeritCycleAllocationStatus.Reopened || next == MeritCycleAllocationStatus.Archived;
            case MeritCycleAllocationStatus.Reopened:
                return next == MeritCycleAllocationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(MeritCycleAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetLoad()
    {
        return Math.Round(Cost + (double)Version, 2);
    }

    public MeritCycleAllocation Copy()
    {
        return new MeritCycleAllocation(
            Code,
            Title,
            CreatedAt,
            Checksum,
            WindowDays,
            Tags,
            Cost,
            ExpiresAt,
            Version,
            Team,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
