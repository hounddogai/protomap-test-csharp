namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive return plan threshold is in its life cycle.</summary>
public enum AdaptiveReturnPlanThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a adaptive return plan threshold and its current status.</summary>
public sealed class AdaptiveReturnPlanThreshold(string code, string label, int expiresAt, string department, int priority, int capacity, bool approved, double margin, string notes, double rating, string category, AdaptiveReturnPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Department { get; set; } = department;
    public int Priority { get; set; } = priority;
    public int Capacity { get; set; } = capacity;
    public bool Approved { get; set; } = approved;
    public double Margin { get; set; } = margin;
    public string Notes { get; set; } = notes;
    public double Rating { get; set; } = rating;
    public string Category { get; set; } = category;
    public AdaptiveReturnPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive return plan threshold in status draft.</summary>
    public static AdaptiveReturnPlanThreshold NewAdaptiveReturnPlanThreshold(string code, string label)
    {
        return new AdaptiveReturnPlanThreshold(
            code,
            label,
            0,
            "",
            0,
            0,
            false,
            0.0,
            "",
            0.0,
            "",
            AdaptiveReturnPlanThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveReturnPlanThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveReturnPlanThresholdStatus next)
    {
        switch (Status)
        {
            case AdaptiveReturnPlanThresholdStatus.Draft:
                switch (next)
                {
                    case AdaptiveReturnPlanThresholdStatus.Active:
                    case AdaptiveReturnPlanThresholdStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveReturnPlanThresholdStatus.Pending:
                return next == AdaptiveReturnPlanThresholdStatus.Suspended || next == AdaptiveReturnPlanThresholdStatus.Closed;
            case AdaptiveReturnPlanThresholdStatus.Active:
                switch (next)
                {
                    case AdaptiveReturnPlanThresholdStatus.Suspended:
                        return true;
                }
                return false;
            case AdaptiveReturnPlanThresholdStatus.Suspended:
                switch (next)
                {
                    case AdaptiveReturnPlanThresholdStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveReturnPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive return plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Rating - (double)Capacity, 2);
    }

    public AdaptiveReturnPlanThreshold Copy()
    {
        return new AdaptiveReturnPlanThreshold(
            Code,
            Label,
            ExpiresAt,
            Department,
            Priority,
            Capacity,
            Approved,
            Margin,
            Notes,
            Rating,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
