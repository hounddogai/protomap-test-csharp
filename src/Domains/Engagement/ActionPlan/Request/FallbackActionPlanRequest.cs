namespace Acme.Workforce.Engagement.ActionPlan.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback action plan request is in its life cycle.</summary>
public enum FallbackActionPlanRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback action plan request. Its code is unique within a tenant.</summary>
public sealed class FallbackActionPlanRequest(string code, string name, int attempts, string currency, string grade, string label, string channel, int graceDays, string team, double cost, FallbackActionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Currency { get; set; } = currency;
    public string Grade { get; set; } = grade;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int GraceDays { get; set; } = graceDays;
    public string Team { get; set; } = team;
    public double Cost { get; set; } = cost;
    public FallbackActionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback action plan request in status open.</summary>
    public static FallbackActionPlanRequest NewFallbackActionPlanRequest(string code, string name)
    {
        return new FallbackActionPlanRequest(
            code,
            name,
            0,
            "",
            "",
            "",
            "",
            0,
            "",
            0.0,
            FallbackActionPlanRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackActionPlanRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackActionPlanRequestStatus next)
    {
        switch (Status)
        {
            case FallbackActionPlanRequestStatus.Open:
                return next == FallbackActionPlanRequestStatus.Archived;
            case FallbackActionPlanRequestStatus.InReview:
                return next == FallbackActionPlanRequestStatus.Resolved;
            case FallbackActionPlanRequestStatus.Resolved:
                switch (next)
                {
                    case FallbackActionPlanRequestStatus.Archived:
                        return true;
                }
                return false;
            case FallbackActionPlanRequestStatus.Reopened:
                return next == FallbackActionPlanRequestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackActionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback action plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Cost * (double)Attempts, 2);
    }

    public FallbackActionPlanRequest Copy()
    {
        return new FallbackActionPlanRequest(
            Code,
            Name,
            Attempts,
            Currency,
            Grade,
            Label,
            Channel,
            GraceDays,
            Team,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
