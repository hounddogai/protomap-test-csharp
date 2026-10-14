namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional access badge score is in its life cycle.</summary>
public enum RegionalAccessBadgeScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional access badge score. Its code is unique within a tenant.</summary>
public sealed class RegionalAccessBadgeScore(string code, string name, string contactEmail, int capacity, double floorValue, string ownerId, int leaveDays, string description, double score, string skill, int windowDays, RegionalAccessBadgeScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int Capacity { get; set; } = capacity;
    public double FloorValue { get; set; } = floorValue;
    public string OwnerId { get; set; } = ownerId;
    public int LeaveDays { get; set; } = leaveDays;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public string Skill { get; set; } = skill;
    public int WindowDays { get; set; } = windowDays;
    public RegionalAccessBadgeScoreStatus Status { get; set; } = status;

    /// <summary>Starts a regional access badge score in status created.</summary>
    public static RegionalAccessBadgeScore NewRegionalAccessBadgeScore(string code, string name)
    {
        return new RegionalAccessBadgeScore(
            code,
            name,
            "",
            0,
            0.0,
            "",
            0,
            "",
            0.0,
            "",
            0,
            RegionalAccessBadgeScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAccessBadgeScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAccessBadgeScoreStatus next)
    {
        switch (Status)
        {
            case RegionalAccessBadgeScoreStatus.Created:
                return next == RegionalAccessBadgeScoreStatus.Validated || next == RegionalAccessBadgeScoreStatus.Retired;
            case RegionalAccessBadgeScoreStatus.Validated:
                switch (next)
                {
                    case RegionalAccessBadgeScoreStatus.Published:
                    case RegionalAccessBadgeScoreStatus.Retired:
                        return true;
                }
                return false;
            case RegionalAccessBadgeScoreStatus.Published:
                switch (next)
                {
                    case RegionalAccessBadgeScoreStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAccessBadgeScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional access badge score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional access badge score"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)Capacity * Score, 2);
    }

    public RegionalAccessBadgeScore Copy()
    {
        return new RegionalAccessBadgeScore(
            Code,
            Name,
            ContactEmail,
            Capacity,
            FloorValue,
            OwnerId,
            LeaveDays,
            Description,
            Score,
            Skill,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
