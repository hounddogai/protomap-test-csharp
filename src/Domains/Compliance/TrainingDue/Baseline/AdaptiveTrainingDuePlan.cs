namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive training due plan is in its life cycle.</summary>
public enum AdaptiveTrainingDuePlanStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive training due plan and its current status.</summary>
public sealed class AdaptiveTrainingDuePlan(string code, string label, int durationMinutes, int windowDays, int attempts, int capacity, double score, double rating, int limit, AdaptiveTrainingDuePlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int WindowDays { get; set; } = windowDays;
    public int Attempts { get; set; } = attempts;
    public int Capacity { get; set; } = capacity;
    public double Score { get; set; } = score;
    public double Rating { get; set; } = rating;
    public int Limit { get; set; } = limit;
    public AdaptiveTrainingDuePlanStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive training due plan in status requested.</summary>
    public static AdaptiveTrainingDuePlan NewAdaptiveTrainingDuePlan(string code, string label)
    {
        return new AdaptiveTrainingDuePlan(
            code,
            label,
            0,
            0,
            0,
            0,
            0.0,
            0.0,
            0,
            AdaptiveTrainingDuePlanStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTrainingDuePlanStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTrainingDuePlanStatus next)
    {
        switch (Status)
        {
            case AdaptiveTrainingDuePlanStatus.Requested:
                return next == AdaptiveTrainingDuePlanStatus.Expired;
            case AdaptiveTrainingDuePlanStatus.Granted:
                switch (next)
                {
                    case AdaptiveTrainingDuePlanStatus.Expired:
                        return true;
                }
                return false;
            case AdaptiveTrainingDuePlanStatus.Revoked:
                return next == AdaptiveTrainingDuePlanStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AdaptiveTrainingDuePlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive training due plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive training due plan"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Capacity - (double)DurationMinutes, 2);
    }

    public AdaptiveTrainingDuePlan Copy()
    {
        return new AdaptiveTrainingDuePlan(
            Code,
            Label,
            DurationMinutes,
            WindowDays,
            Attempts,
            Capacity,
            Score,
            Rating,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
