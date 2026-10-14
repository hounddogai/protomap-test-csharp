namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback sentiment plan is in its life cycle.</summary>
public enum FallbackSentimentPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A fallback sentiment plan as the sentiment team keeps it.</summary>
public sealed class FallbackSentimentPlan(string code, string name, string employeeId, string summary, int attempts, int tenureMonths, double budget, int endsAt, FallbackSentimentPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string Summary { get; set; } = summary;
    public int Attempts { get; set; } = attempts;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Budget { get; set; } = budget;
    public int EndsAt { get; set; } = endsAt;
    public FallbackSentimentPlanStatus Status { get; set; } = status;

    /// <summary>Starts a fallback sentiment plan in status new.</summary>
    public static FallbackSentimentPlan NewFallbackSentimentPlan(string code, string name)
    {
        return new FallbackSentimentPlan(code, name, "", "", 0, 0, 0.0, 0, FallbackSentimentPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSentimentPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSentimentPlanStatus next)
    {
        switch (Status)
        {
            case FallbackSentimentPlanStatus.New:
                return next == FallbackSentimentPlanStatus.Done || next == FallbackSentimentPlanStatus.Failed;
            case FallbackSentimentPlanStatus.Queued:
                return next == FallbackSentimentPlanStatus.Running || next == FallbackSentimentPlanStatus.Failed;
            case FallbackSentimentPlanStatus.Running:
                switch (next)
                {
                    case FallbackSentimentPlanStatus.Done:
                    case FallbackSentimentPlanStatus.Failed:
                        return true;
                }
                return false;
            case FallbackSentimentPlanStatus.Done:
                return next == FallbackSentimentPlanStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FallbackSentimentPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback sentiment plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetExposure()
    {
        return Math.Round(Budget - (double)Attempts, 2);
    }

    public FallbackSentimentPlan Copy()
    {
        return new FallbackSentimentPlan(
            Code,
            Name,
            EmployeeId,
            Summary,
            Attempts,
            TenureMonths,
            Budget,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
