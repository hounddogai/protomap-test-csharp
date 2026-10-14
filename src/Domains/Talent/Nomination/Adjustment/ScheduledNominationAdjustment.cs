namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled nomination adjustment is in its life cycle.</summary>
public enum ScheduledNominationAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled nomination adjustment. Its code is unique within a tenant.</summary>
public sealed class ScheduledNominationAdjustment(string code, string label, double floorValue, string externalRef, List<string> tags, double hours, ScheduledNominationAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string ExternalRef { get; set; } = externalRef;
    public List<string> Tags { get; set; } = tags;
    public double Hours { get; set; } = hours;
    public ScheduledNominationAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled nomination adjustment in status idle.</summary>
    public static ScheduledNominationAdjustment NewScheduledNominationAdjustment(string code, string label)
    {
        return new ScheduledNominationAdjustment(
            code,
            label,
            0.0,
            "",
            new List<string>(),
            0.0,
            ScheduledNominationAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledNominationAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledNominationAdjustmentStatus next)
    {
        switch (Status)
        {
            case ScheduledNominationAdjustmentStatus.Idle:
                switch (next)
                {
                    case ScheduledNominationAdjustmentStatus.Warming:
                        return true;
                }
                return false;
            case ScheduledNominationAdjustmentStatus.Warming:
                switch (next)
                {
                    case ScheduledNominationAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledNominationAdjustmentStatus.Ready:
                return next == ScheduledNominationAdjustmentStatus.Draining;
            case ScheduledNominationAdjustmentStatus.Draining:
                return next == ScheduledNominationAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledNominationAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled nomination adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedValue()
    {
        return Math.Round(Hours + FloorValue, 2);
    }

    public ScheduledNominationAdjustment Copy()
    {
        return new ScheduledNominationAdjustment(Code, Label, FloorValue, ExternalRef, Tags, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
