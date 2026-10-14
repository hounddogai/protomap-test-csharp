namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pay band estimate is in its life cycle.</summary>
public enum DefaultPayBandEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A default pay band estimate as the pay band team keeps it.</summary>
public sealed class DefaultPayBandEstimate(string code, string title, string reviewer, string name, bool active, string costCenter, string description, string externalRef, string notes, string team, string skill, double score, DefaultPayBandEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string CostCenter { get; set; } = costCenter;
    public string Description { get; set; } = description;
    public string ExternalRef { get; set; } = externalRef;
    public string Notes { get; set; } = notes;
    public string Team { get; set; } = team;
    public string Skill { get; set; } = skill;
    public double Score { get; set; } = score;
    public DefaultPayBandEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a default pay band estimate in status open.</summary>
    public static DefaultPayBandEstimate NewDefaultPayBandEstimate(string code, string title)
    {
        return new DefaultPayBandEstimate(
            code,
            title,
            "",
            "",
            false,
            "",
            "",
            "",
            "",
            "",
            "",
            0.0,
            DefaultPayBandEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPayBandEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPayBandEstimateStatus next)
    {
        switch (Status)
        {
            case DefaultPayBandEstimateStatus.Open:
                return next == DefaultPayBandEstimateStatus.Resolved;
            case DefaultPayBandEstimateStatus.InReview:
                switch (next)
                {
                    case DefaultPayBandEstimateStatus.Resolved:
                    case DefaultPayBandEstimateStatus.Reopened:
                        return true;
                }
                return false;
            case DefaultPayBandEstimateStatus.Resolved:
                switch (next)
                {
                    case DefaultPayBandEstimateStatus.Reopened:
                        return true;
                }
                return false;
            case DefaultPayBandEstimateStatus.Reopened:
                switch (next)
                {
                    case DefaultPayBandEstimateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPayBandEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pay band estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public DefaultPayBandEstimate Copy()
    {
        return new DefaultPayBandEstimate(
            Code,
            Title,
            Reviewer,
            Name,
            Active,
            CostCenter,
            Description,
            ExternalRef,
            Notes,
            Team,
            Skill,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
