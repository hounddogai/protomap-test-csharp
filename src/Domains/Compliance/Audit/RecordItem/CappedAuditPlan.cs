namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped audit plan is in its life cycle.</summary>
public enum CappedAuditPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One capped audit plan, with what the planners need to act on it.</summary>
public sealed class CappedAuditPlan(string code, string label, double ratio, int revision, string unit, int durationMinutes, bool approved, double minimum, string description, CappedAuditPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public int Revision { get; set; } = revision;
    public string Unit { get; set; } = unit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Approved { get; set; } = approved;
    public double Minimum { get; set; } = minimum;
    public string Description { get; set; } = description;
    public CappedAuditPlanStatus Status { get; set; } = status;

    /// <summary>Starts a capped audit plan in status idle.</summary>
    public static CappedAuditPlan NewCappedAuditPlan(string code, string label)
    {
        return new CappedAuditPlan(code, label, 0.0, 0, "", 0, false, 0.0, "", CappedAuditPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAuditPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAuditPlanStatus next)
    {
        switch (Status)
        {
            case CappedAuditPlanStatus.Idle:
                return next == CappedAuditPlanStatus.Warming;
            case CappedAuditPlanStatus.Warming:
                switch (next)
                {
                    case CappedAuditPlanStatus.Ready:
                        return true;
                }
                return false;
            case CappedAuditPlanStatus.Ready:
                return next == CappedAuditPlanStatus.Draining || next == CappedAuditPlanStatus.Stopped;
            case CappedAuditPlanStatus.Draining:
                return next == CappedAuditPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(CappedAuditPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped audit plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"capped audit plan"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)DurationMinutes * Minimum, 2);
    }

    public CappedAuditPlan Copy()
    {
        return new CappedAuditPlan(
            Code,
            Label,
            Ratio,
            Revision,
            Unit,
            DurationMinutes,
            Approved,
            Minimum,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
