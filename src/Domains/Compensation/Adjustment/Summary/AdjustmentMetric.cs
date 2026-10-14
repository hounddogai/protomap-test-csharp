namespace Acme.Workforce.Compensation.Adjustment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment metric is in its life cycle.</summary>
public enum AdjustmentMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adjustment metric, with what the planners need to act on it.</summary>
public sealed class AdjustmentMetric(string code, string title, bool archived, string locale, string currency, string channel, string description, double score, AdjustmentMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Locale { get; set; } = locale;
    public string Currency { get; set; } = currency;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public AdjustmentMetricStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment metric in status new.</summary>
    public static AdjustmentMetric NewAdjustmentMetric(string code, string title)
    {
        return new AdjustmentMetric(code, title, false, "", "", "", "", 0.0, AdjustmentMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentMetricStatus next)
    {
        switch (Status)
        {
            case AdjustmentMetricStatus.New:
                return next == AdjustmentMetricStatus.Queued;
            case AdjustmentMetricStatus.Queued:
                return next == AdjustmentMetricStatus.Running || next == AdjustmentMetricStatus.Done;
            case AdjustmentMetricStatus.Running:
                return next == AdjustmentMetricStatus.Done || next == AdjustmentMetricStatus.Failed;
            case AdjustmentMetricStatus.Done:
                return next == AdjustmentMetricStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdjustmentMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AdjustmentMetric Copy()
    {
        return new AdjustmentMetric(Code, Title, Archived, Locale, Currency, Channel, Description, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
