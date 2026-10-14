namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal engagement milestone is in its life cycle.</summary>
public enum SeasonalEngagementMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal engagement milestone as the engagement team keeps it.</summary>
public sealed class SeasonalEngagementMilestone(string code, string label, string timezone, string description, string title, double rating, string summary, string salaryBand, int createdAt, SeasonalEngagementMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public string Description { get; set; } = description;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Summary { get; set; } = summary;
    public string SalaryBand { get; set; } = salaryBand;
    public int CreatedAt { get; set; } = createdAt;
    public SeasonalEngagementMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal engagement milestone in status requested.</summary>
    public static SeasonalEngagementMilestone NewSeasonalEngagementMilestone(string code, string label)
    {
        return new SeasonalEngagementMilestone(
            code,
            label,
            "",
            "",
            "",
            0.0,
            "",
            "",
            0,
            SeasonalEngagementMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalEngagementMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalEngagementMilestoneStatus next)
    {
        switch (Status)
        {
            case SeasonalEngagementMilestoneStatus.Requested:
                return next == SeasonalEngagementMilestoneStatus.Granted || next == SeasonalEngagementMilestoneStatus.Expired;
            case SeasonalEngagementMilestoneStatus.Granted:
                switch (next)
                {
                    case SeasonalEngagementMilestoneStatus.Revoked:
                    case SeasonalEngagementMilestoneStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalEngagementMilestoneStatus.Revoked:
                switch (next)
                {
                    case SeasonalEngagementMilestoneStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalEngagementMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal engagement milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"seasonal engagement milestone"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rating * (double)CreatedAt, 2);
    }

    public SeasonalEngagementMilestone Copy()
    {
        return new SeasonalEngagementMilestone(
            Code,
            Label,
            Timezone,
            Description,
            Title,
            Rating,
            Summary,
            SalaryBand,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
