namespace Acme.Workforce.Payrollops.Deduction.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled deduction estimate is in its life cycle.</summary>
public enum ScheduledDeductionEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled deduction estimate. Its code is unique within a tenant.</summary>
public sealed class ScheduledDeductionEstimate(string code, string name, string managerId, Dictionary<string, string> attributes, double budget, ScheduledDeductionEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Budget { get; set; } = budget;
    public ScheduledDeductionEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled deduction estimate in status idle.</summary>
    public static ScheduledDeductionEstimate NewScheduledDeductionEstimate(string code, string name)
    {
        return new ScheduledDeductionEstimate(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            0.0,
            ScheduledDeductionEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledDeductionEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledDeductionEstimateStatus next)
    {
        switch (Status)
        {
            case ScheduledDeductionEstimateStatus.Idle:
                switch (next)
                {
                    case ScheduledDeductionEstimateStatus.Ready:
                    case ScheduledDeductionEstimateStatus.Draining:
                        return true;
                }
                return false;
            case ScheduledDeductionEstimateStatus.Warming:
                return next == ScheduledDeductionEstimateStatus.Ready || next == ScheduledDeductionEstimateStatus.Draining;
            case ScheduledDeductionEstimateStatus.Ready:
                return next == ScheduledDeductionEstimateStatus.Draining;
            case ScheduledDeductionEstimateStatus.Draining:
                return next == ScheduledDeductionEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledDeductionEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled deduction estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled deduction estimate"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public ScheduledDeductionEstimate Copy()
    {
        return new ScheduledDeductionEstimate(Code, Name, ManagerId, Attributes, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
