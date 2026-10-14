namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual milestone is in its life cycle.</summary>
public enum AccrualMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a accrual milestone and its current status.</summary>
public sealed class AccrualMilestone(string code, string name, bool pinned, List<string> tags, int updatedAt, int endsAt, string country, AccrualMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public int EndsAt { get; set; } = endsAt;
    public string Country { get; set; } = country;
    public AccrualMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a accrual milestone in status requested.</summary>
    public static AccrualMilestone NewAccrualMilestone(string code, string name)
    {
        return new AccrualMilestone(
            code,
            name,
            false,
            new List<string>(),
            0,
            0,
            "",
            AccrualMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualMilestoneStatus next)
    {
        switch (Status)
        {
            case AccrualMilestoneStatus.Requested:
                switch (next)
                {
                    case AccrualMilestoneStatus.Revoked:
                        return true;
                }
                return false;
            case AccrualMilestoneStatus.Granted:
                return next == AccrualMilestoneStatus.Revoked;
            case AccrualMilestoneStatus.Revoked:
                return next == AccrualMilestoneStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AccrualMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double ProjectedExposure()
    {
        return Math.Round((double)EndsAt - (double)UpdatedAt, 2);
    }

    public AccrualMilestone Copy()
    {
        return new AccrualMilestone(Code, Name, Pinned, Tags, UpdatedAt, EndsAt, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
