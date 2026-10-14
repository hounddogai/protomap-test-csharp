namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pool rule is in its life cycle.</summary>
public enum StrictPoolRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a strict pool rule and its current status.</summary>
public sealed class StrictPoolRule(string code, string title, double ceiling, double minimum, string region, int durationMinutes, string managerId, int graceDays, StrictPoolRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public double Minimum { get; set; } = minimum;
    public string Region { get; set; } = region;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ManagerId { get; set; } = managerId;
    public int GraceDays { get; set; } = graceDays;
    public StrictPoolRuleStatus Status { get; set; } = status;

    /// <summary>Starts a strict pool rule in status draft.</summary>
    public static StrictPoolRule NewStrictPoolRule(string code, string title)
    {
        return new StrictPoolRule(code, title, 0.0, 0.0, "", 0, "", 0, StrictPoolRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPoolRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPoolRuleStatus next)
    {
        switch (Status)
        {
            case StrictPoolRuleStatus.Draft:
                return next == StrictPoolRuleStatus.Active || next == StrictPoolRuleStatus.Suspended;
            case StrictPoolRuleStatus.Pending:
                return next == StrictPoolRuleStatus.Suspended || next == StrictPoolRuleStatus.Closed;
            case StrictPoolRuleStatus.Active:
                switch (next)
                {
                    case StrictPoolRuleStatus.Closed:
                        return true;
                }
                return false;
            case StrictPoolRuleStatus.Suspended:
                switch (next)
                {
                    case StrictPoolRuleStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPoolRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pool rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)GraceDays + (double)DurationMinutes, 2);
    }

    public StrictPoolRule Copy()
    {
        return new StrictPoolRule(
            Code,
            Title,
            Ceiling,
            Minimum,
            Region,
            DurationMinutes,
            ManagerId,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
