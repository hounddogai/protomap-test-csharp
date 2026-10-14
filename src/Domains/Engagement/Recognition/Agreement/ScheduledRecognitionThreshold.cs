namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled recognition threshold is in its life cycle.</summary>
public enum ScheduledRecognitionThresholdStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scheduled recognition threshold and its current status.</summary>
public sealed class ScheduledRecognitionThreshold(string code, string label, string region, string locale, string skill, int capacity, string costCenter, ScheduledRecognitionThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public string Locale { get; set; } = locale;
    public string Skill { get; set; } = skill;
    public int Capacity { get; set; } = capacity;
    public string CostCenter { get; set; } = costCenter;
    public ScheduledRecognitionThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled recognition threshold in status proposed.</summary>
    public static ScheduledRecognitionThreshold NewScheduledRecognitionThreshold(string code, string label)
    {
        return new ScheduledRecognitionThreshold(
            code,
            label,
            "",
            "",
            "",
            0,
            "",
            ScheduledRecognitionThresholdStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRecognitionThresholdStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRecognitionThresholdStatus next)
    {
        switch (Status)
        {
            case ScheduledRecognitionThresholdStatus.Proposed:
                return next == ScheduledRecognitionThresholdStatus.Approved || next == ScheduledRecognitionThresholdStatus.Withdrawn;
            case ScheduledRecognitionThresholdStatus.Approved:
                switch (next)
                {
                    case ScheduledRecognitionThresholdStatus.Rejected:
                    case ScheduledRecognitionThresholdStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledRecognitionThresholdStatus.Rejected:
                return next == ScheduledRecognitionThresholdStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledRecognitionThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled recognition threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled recognition threshold"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public ScheduledRecognitionThreshold Copy()
    {
        return new ScheduledRecognitionThreshold(Code, Label, Region, Locale, Skill, Capacity, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
