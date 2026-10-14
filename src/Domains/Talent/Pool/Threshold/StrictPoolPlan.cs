namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pool plan is in its life cycle.</summary>
public enum StrictPoolPlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a strict pool plan and its current status.</summary>
public sealed class StrictPoolPlan(string code, string label, int retries, double margin, string summary, int endsAt, string reviewer, double floorValue, double ceiling, double weight, StrictPoolPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public double Margin { get; set; } = margin;
    public string Summary { get; set; } = summary;
    public int EndsAt { get; set; } = endsAt;
    public string Reviewer { get; set; } = reviewer;
    public double FloorValue { get; set; } = floorValue;
    public double Ceiling { get; set; } = ceiling;
    public double Weight { get; set; } = weight;
    public StrictPoolPlanStatus Status { get; set; } = status;

    /// <summary>Starts a strict pool plan in status requested.</summary>
    public static StrictPoolPlan NewStrictPoolPlan(string code, string label)
    {
        return new StrictPoolPlan(code, label, 0, 0.0, "", 0, "", 0.0, 0.0, 0.0, StrictPoolPlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPoolPlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPoolPlanStatus next)
    {
        switch (Status)
        {
            case StrictPoolPlanStatus.Requested:
                switch (next)
                {
                    case StrictPoolPlanStatus.Revoked:
                        return true;
                }
                return false;
            case StrictPoolPlanStatus.Granted:
                switch (next)
                {
                    case StrictPoolPlanStatus.Revoked:
                    case StrictPoolPlanStatus.Expired:
                        return true;
                }
                return false;
            case StrictPoolPlanStatus.Revoked:
                return next == StrictPoolPlanStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictPoolPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pool plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Weight * Ceiling, 2);
    }

    public StrictPoolPlan Copy()
    {
        return new StrictPoolPlan(
            Code,
            Label,
            Retries,
            Margin,
            Summary,
            EndsAt,
            Reviewer,
            FloorValue,
            Ceiling,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
