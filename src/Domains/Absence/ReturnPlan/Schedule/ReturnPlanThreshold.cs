namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan threshold is in its life cycle.</summary>
public enum ReturnPlanThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A return plan threshold as the return plan team keeps it.</summary>
public sealed class ReturnPlanThreshold(string code, string title, double budget, double threshold, double weight, int batchSize, int limit, double rating, string label, int expiresAt, ReturnPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public double Threshold { get; set; } = threshold;
    public double Weight { get; set; } = weight;
    public int BatchSize { get; set; } = batchSize;
    public int Limit { get; set; } = limit;
    public double Rating { get; set; } = rating;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public ReturnPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a return plan threshold in status requested.</summary>
    public static ReturnPlanThreshold NewReturnPlanThreshold(string code, string title)
    {
        return new ReturnPlanThreshold(
            code,
            title,
            0.0,
            0.0,
            0.0,
            0,
            0,
            0.0,
            "",
            0,
            ReturnPlanThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanThresholdStatus next)
    {
        switch (Status)
        {
            case ReturnPlanThresholdStatus.Requested:
                switch (next)
                {
                    case ReturnPlanThresholdStatus.Expired:
                        return true;
                }
                return false;
            case ReturnPlanThresholdStatus.Granted:
                return next == ReturnPlanThresholdStatus.Revoked || next == ReturnPlanThresholdStatus.Expired;
            case ReturnPlanThresholdStatus.Revoked:
                switch (next)
                {
                    case ReturnPlanThresholdStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Threshold + Weight, 2);
    }

    public ReturnPlanThreshold Copy()
    {
        return new ReturnPlanThreshold(
            Code,
            Title,
            Budget,
            Threshold,
            Weight,
            BatchSize,
            Limit,
            Rating,
            Label,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
