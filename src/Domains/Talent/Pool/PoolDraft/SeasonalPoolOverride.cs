namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pool override is in its life cycle.</summary>
public enum SeasonalPoolOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a seasonal pool override and its current status.</summary>
public sealed class SeasonalPoolOverride(string code, string title, string reviewer, string skill, int capacity, string managerId, bool active, string badge, string category, SeasonalPoolOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Skill { get; set; } = skill;
    public int Capacity { get; set; } = capacity;
    public string ManagerId { get; set; } = managerId;
    public bool Active { get; set; } = active;
    public string Badge { get; set; } = badge;
    public string Category { get; set; } = category;
    public SeasonalPoolOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pool override in status proposed.</summary>
    public static SeasonalPoolOverride NewSeasonalPoolOverride(string code, string title)
    {
        return new SeasonalPoolOverride(
            code,
            title,
            "",
            "",
            0,
            "",
            false,
            "",
            "",
            SeasonalPoolOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPoolOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPoolOverrideStatus next)
    {
        switch (Status)
        {
            case SeasonalPoolOverrideStatus.Proposed:
                switch (next)
                {
                    case SeasonalPoolOverrideStatus.Approved:
                    case SeasonalPoolOverrideStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalPoolOverrideStatus.Approved:
                switch (next)
                {
                    case SeasonalPoolOverrideStatus.Rejected:
                    case SeasonalPoolOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalPoolOverrideStatus.Rejected:
                return next == SeasonalPoolOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalPoolOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pool override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public SeasonalPoolOverride Copy()
    {
        return new SeasonalPoolOverride(
            Code,
            Title,
            Reviewer,
            Skill,
            Capacity,
            ManagerId,
            Active,
            Badge,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
