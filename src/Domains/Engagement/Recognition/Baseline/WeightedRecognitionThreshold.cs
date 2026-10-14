namespace Acme.Workforce.Engagement.Recognition.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted recognition threshold is in its life cycle.</summary>
public enum WeightedRecognitionThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One weighted recognition threshold, with what the planners need to act on it.</summary>
public sealed class WeightedRecognitionThreshold(string code, string label, int createdAt, string jobCode, string badge, string reviewer, string costCenter, WeightedRecognitionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string JobCode { get; set; } = jobCode;
    public string Badge { get; set; } = badge;
    public string Reviewer { get; set; } = reviewer;
    public string CostCenter { get; set; } = costCenter;
    public WeightedRecognitionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a weighted recognition threshold in status open.</summary>
    public static WeightedRecognitionThreshold NewWeightedRecognitionThreshold(string code, string label)
    {
        return new WeightedRecognitionThreshold(
            code,
            label,
            0,
            "",
            "",
            "",
            "",
            WeightedRecognitionThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRecognitionThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRecognitionThresholdStatus next)
    {
        switch (Status)
        {
            case WeightedRecognitionThresholdStatus.Open:
                switch (next)
                {
                    case WeightedRecognitionThresholdStatus.Resolved:
                        return true;
                }
                return false;
            case WeightedRecognitionThresholdStatus.InReview:
                switch (next)
                {
                    case WeightedRecognitionThresholdStatus.Resolved:
                    case WeightedRecognitionThresholdStatus.Archived:
                        return true;
                }
                return false;
            case WeightedRecognitionThresholdStatus.Resolved:
                return next == WeightedRecognitionThresholdStatus.Reopened || next == WeightedRecognitionThresholdStatus.Archived;
            case WeightedRecognitionThresholdStatus.Reopened:
                switch (next)
                {
                    case WeightedRecognitionThresholdStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedRecognitionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted recognition threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public WeightedRecognitionThreshold Copy()
    {
        return new WeightedRecognitionThreshold(
            Code,
            Label,
            CreatedAt,
            JobCode,
            Badge,
            Reviewer,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
