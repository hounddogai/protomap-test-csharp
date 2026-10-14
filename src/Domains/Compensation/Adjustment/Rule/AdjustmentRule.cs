namespace Acme.Workforce.Compensation.Adjustment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment rule is in its life cycle.</summary>
public enum AdjustmentRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a adjustment rule and its current status.</summary>
public sealed class AdjustmentRule(string code, string label, int version, string summary, double threshold, int tenureMonths, int batchSize, string title, int durationMinutes, string source, AdjustmentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Summary { get; set; } = summary;
    public double Threshold { get; set; } = threshold;
    public int TenureMonths { get; set; } = tenureMonths;
    public int BatchSize { get; set; } = batchSize;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Source { get; set; } = source;
    public AdjustmentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment rule in status new.</summary>
    public static AdjustmentRule NewAdjustmentRule(string code, string label)
    {
        return new AdjustmentRule(code, label, 0, "", 0.0, 0, 0, "", 0, "", AdjustmentRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentRuleStatus next)
    {
        switch (Status)
        {
            case AdjustmentRuleStatus.New:
                return next == AdjustmentRuleStatus.Queued || next == AdjustmentRuleStatus.Done;
            case AdjustmentRuleStatus.Queued:
                switch (next)
                {
                    case AdjustmentRuleStatus.Done:
                        return true;
                }
                return false;
            case AdjustmentRuleStatus.Running:
                switch (next)
                {
                    case AdjustmentRuleStatus.Done:
                    case AdjustmentRuleStatus.Failed:
                        return true;
                }
                return false;
            case AdjustmentRuleStatus.Done:
                switch (next)
                {
                    case AdjustmentRuleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"adjustment rule"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)BatchSize - (double)DurationMinutes, 2);
    }

    public AdjustmentRule Copy()
    {
        return new AdjustmentRule(
            Code,
            Label,
            Version,
            Summary,
            Threshold,
            TenureMonths,
            BatchSize,
            Title,
            DurationMinutes,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
