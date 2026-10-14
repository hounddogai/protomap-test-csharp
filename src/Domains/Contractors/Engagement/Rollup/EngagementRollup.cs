namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement rollup is in its life cycle.</summary>
public enum EngagementRollupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One engagement rollup, with what the planners need to act on it.</summary>
public sealed class EngagementRollup(string code, string label, string skill, string timezone, double floorValue, double hours, string phone, double rating, int updatedAt, EngagementRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string Timezone { get; set; } = timezone;
    public double FloorValue { get; set; } = floorValue;
    public double Hours { get; set; } = hours;
    public string Phone { get; set; } = phone;
    public double Rating { get; set; } = rating;
    public int UpdatedAt { get; set; } = updatedAt;
    public EngagementRollupStatus Status { get; set; } = status;

    /// <summary>Starts a engagement rollup in status draft.</summary>
    public static EngagementRollup NewEngagementRollup(string code, string label)
    {
        return new EngagementRollup(code, label, "", "", 0.0, 0.0, "", 0.0, 0, EngagementRollupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementRollupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementRollupStatus next)
    {
        switch (Status)
        {
            case EngagementRollupStatus.Draft:
                switch (next)
                {
                    case EngagementRollupStatus.Closed:
                        return true;
                }
                return false;
            case EngagementRollupStatus.Pending:
                switch (next)
                {
                    case EngagementRollupStatus.Active:
                    case EngagementRollupStatus.Closed:
                        return true;
                }
                return false;
            case EngagementRollupStatus.Active:
                return next == EngagementRollupStatus.Closed;
            case EngagementRollupStatus.Suspended:
                return next == EngagementRollupStatus.Closed;
        }
        return false;
    }

    public void MoveTo(EngagementRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"engagement rollup"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rating - Hours, 2);
    }

    public EngagementRollup Copy()
    {
        return new EngagementRollup(
            Code,
            Label,
            Skill,
            Timezone,
            FloorValue,
            Hours,
            Phone,
            Rating,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
