namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default action plan rollup is in its life cycle.</summary>
public enum DefaultActionPlanRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One default action plan rollup, with what the planners need to act on it.</summary>
public sealed class DefaultActionPlanRollup(string code, string label, double rate, int expiresAt, string jobCode, int startsAt, int durationMinutes, int revision, string team, DefaultActionPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int ExpiresAt { get; set; } = expiresAt;
    public string JobCode { get; set; } = jobCode;
    public int StartsAt { get; set; } = startsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Revision { get; set; } = revision;
    public string Team { get; set; } = team;
    public DefaultActionPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a default action plan rollup in status open.</summary>
    public static DefaultActionPlanRollup NewDefaultActionPlanRollup(string code, string label)
    {
        return new DefaultActionPlanRollup(code, label, 0.0, 0, "", 0, 0, 0, "", DefaultActionPlanRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultActionPlanRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultActionPlanRollupStatus next)
    {
        switch (Status)
        {
            case DefaultActionPlanRollupStatus.Open:
                switch (next)
                {
                    case DefaultActionPlanRollupStatus.InReview:
                        return true;
                }
                return false;
            case DefaultActionPlanRollupStatus.InReview:
                return next == DefaultActionPlanRollupStatus.Resolved;
            case DefaultActionPlanRollupStatus.Resolved:
                switch (next)
                {
                    case DefaultActionPlanRollupStatus.Reopened:
                    case DefaultActionPlanRollupStatus.Archived:
                        return true;
                }
                return false;
            case DefaultActionPlanRollupStatus.Reopened:
                switch (next)
                {
                    case DefaultActionPlanRollupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultActionPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default action plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)StartsAt * Rate, 2);
    }

    public DefaultActionPlanRollup Copy()
    {
        return new DefaultActionPlanRollup(
            Code,
            Label,
            Rate,
            ExpiresAt,
            JobCode,
            StartsAt,
            DurationMinutes,
            Revision,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
