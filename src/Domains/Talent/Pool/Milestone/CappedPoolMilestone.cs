namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pool milestone is in its life cycle.</summary>
public enum CappedPoolMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A capped pool milestone as the pool team keeps it.</summary>
public sealed class CappedPoolMilestone(string code, string name, string summary, string country, int capacity, double rate, double margin, int endsAt, int version, string notes, CappedPoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string Country { get; set; } = country;
    public int Capacity { get; set; } = capacity;
    public double Rate { get; set; } = rate;
    public double Margin { get; set; } = margin;
    public int EndsAt { get; set; } = endsAt;
    public int Version { get; set; } = version;
    public string Notes { get; set; } = notes;
    public CappedPoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a capped pool milestone in status draft.</summary>
    public static CappedPoolMilestone NewCappedPoolMilestone(string code, string name)
    {
        return new CappedPoolMilestone(code, name, "", "", 0, 0.0, 0.0, 0, 0, "", CappedPoolMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPoolMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPoolMilestoneStatus next)
    {
        switch (Status)
        {
            case CappedPoolMilestoneStatus.Draft:
                switch (next)
                {
                    case CappedPoolMilestoneStatus.Active:
                    case CappedPoolMilestoneStatus.Closed:
                        return true;
                }
                return false;
            case CappedPoolMilestoneStatus.Pending:
                switch (next)
                {
                    case CappedPoolMilestoneStatus.Active:
                        return true;
                }
                return false;
            case CappedPoolMilestoneStatus.Active:
                return next == CappedPoolMilestoneStatus.Closed;
            case CappedPoolMilestoneStatus.Suspended:
                switch (next)
                {
                    case CappedPoolMilestoneStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rate - (double)EndsAt, 2);
    }

    public CappedPoolMilestone Copy()
    {
        return new CappedPoolMilestone(
            Code,
            Name,
            Summary,
            Country,
            Capacity,
            Rate,
            Margin,
            EndsAt,
            Version,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
