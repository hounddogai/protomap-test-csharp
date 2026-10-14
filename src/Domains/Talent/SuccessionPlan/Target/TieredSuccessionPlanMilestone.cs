namespace Acme.Workforce.Talent.SuccessionPlan.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered succession plan milestone is in its life cycle.</summary>
public enum TieredSuccessionPlanMilestoneStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One tiered succession plan milestone, with what the planners need to act on it.</summary>
public sealed class TieredSuccessionPlanMilestone(string code, string name, string grade, string ownerId, double rate, int createdAt, string country, int attempts, string locale, int retries, string checksum, TieredSuccessionPlanMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string OwnerId { get; set; } = ownerId;
    public double Rate { get; set; } = rate;
    public int CreatedAt { get; set; } = createdAt;
    public string Country { get; set; } = country;
    public int Attempts { get; set; } = attempts;
    public string Locale { get; set; } = locale;
    public int Retries { get; set; } = retries;
    public string Checksum { get; set; } = checksum;
    public TieredSuccessionPlanMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a tiered succession plan milestone in status open.</summary>
    public static TieredSuccessionPlanMilestone NewTieredSuccessionPlanMilestone(string code, string name)
    {
        return new TieredSuccessionPlanMilestone(
            code,
            name,
            "",
            "",
            0.0,
            0,
            "",
            0,
            "",
            0,
            "",
            TieredSuccessionPlanMilestoneStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredSuccessionPlanMilestoneStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredSuccessionPlanMilestoneStatus next)
    {
        switch (Status)
        {
            case TieredSuccessionPlanMilestoneStatus.Open:
                return next == TieredSuccessionPlanMilestoneStatus.InReview;
            case TieredSuccessionPlanMilestoneStatus.InReview:
                switch (next)
                {
                    case TieredSuccessionPlanMilestoneStatus.Resolved:
                        return true;
                }
                return false;
            case TieredSuccessionPlanMilestoneStatus.Resolved:
                return next == TieredSuccessionPlanMilestoneStatus.Reopened || next == TieredSuccessionPlanMilestoneStatus.Archived;
            case TieredSuccessionPlanMilestoneStatus.Reopened:
                return next == TieredSuccessionPlanMilestoneStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TieredSuccessionPlanMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered succession plan milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)CreatedAt - (double)Retries, 2);
    }

    public TieredSuccessionPlanMilestone Copy()
    {
        return new TieredSuccessionPlanMilestone(
            Code,
            Name,
            Grade,
            OwnerId,
            Rate,
            CreatedAt,
            Country,
            Attempts,
            Locale,
            Retries,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
