namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment adjustment is in its life cycle.</summary>
public enum SentimentAdjustmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a sentiment adjustment and its current status.</summary>
public sealed class SentimentAdjustment(string code, string title, double weight, double budget, double ratio, int durationMinutes, string externalRef, string employeeId, bool active, string location, double cost, SentimentAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public double Budget { get; set; } = budget;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ExternalRef { get; set; } = externalRef;
    public string EmployeeId { get; set; } = employeeId;
    public bool Active { get; set; } = active;
    public string Location { get; set; } = location;
    public double Cost { get; set; } = cost;
    public SentimentAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment adjustment in status requested.</summary>
    public static SentimentAdjustment NewSentimentAdjustment(string code, string title)
    {
        return new SentimentAdjustment(
            code,
            title,
            0.0,
            0.0,
            0.0,
            0,
            "",
            "",
            false,
            "",
            0.0,
            SentimentAdjustmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentAdjustmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentAdjustmentStatus next)
    {
        switch (Status)
        {
            case SentimentAdjustmentStatus.Requested:
                return next == SentimentAdjustmentStatus.Revoked;
            case SentimentAdjustmentStatus.Granted:
                return next == SentimentAdjustmentStatus.Revoked || next == SentimentAdjustmentStatus.Expired;
            case SentimentAdjustmentStatus.Revoked:
                return next == SentimentAdjustmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SentimentAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment adjustment"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Weight - Budget, 2);
    }

    public SentimentAdjustment Copy()
    {
        return new SentimentAdjustment(
            Code,
            Title,
            Weight,
            Budget,
            Ratio,
            DurationMinutes,
            ExternalRef,
            EmployeeId,
            Active,
            Location,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
