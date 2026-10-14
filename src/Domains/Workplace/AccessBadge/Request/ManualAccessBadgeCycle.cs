namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual access badge cycle is in its life cycle.</summary>
public enum ManualAccessBadgeCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a manual access badge cycle and its current status.</summary>
public sealed class ManualAccessBadgeCycle(string code, string label, string segment, string jobCode, double threshold, ManualAccessBadgeCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string JobCode { get; set; } = jobCode;
    public double Threshold { get; set; } = threshold;
    public ManualAccessBadgeCycleStatus Status { get; set; } = status;

    /// <summary>Starts a manual access badge cycle in status proposed.</summary>
    public static ManualAccessBadgeCycle NewManualAccessBadgeCycle(string code, string label)
    {
        return new ManualAccessBadgeCycle(code, label, "", "", 0.0, ManualAccessBadgeCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAccessBadgeCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAccessBadgeCycleStatus next)
    {
        switch (Status)
        {
            case ManualAccessBadgeCycleStatus.Proposed:
                return next == ManualAccessBadgeCycleStatus.Approved;
            case ManualAccessBadgeCycleStatus.Approved:
                switch (next)
                {
                    case ManualAccessBadgeCycleStatus.Rejected:
                        return true;
                }
                return false;
            case ManualAccessBadgeCycleStatus.Rejected:
                return next == ManualAccessBadgeCycleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualAccessBadgeCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual access badge cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"manual access badge cycle"} {Code}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public ManualAccessBadgeCycle Copy()
    {
        return new ManualAccessBadgeCycle(Code, Label, Segment, JobCode, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
