namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered return plan threshold is in its life cycle.</summary>
public enum TieredReturnPlanThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One tiered return plan threshold, with what the planners need to act on it.</summary>
public sealed class TieredReturnPlanThreshold(string code, string label, bool approved, double budget, int endsAt, TieredReturnPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public double Budget { get; set; } = budget;
    public int EndsAt { get; set; } = endsAt;
    public TieredReturnPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a tiered return plan threshold in status idle.</summary>
    public static TieredReturnPlanThreshold NewTieredReturnPlanThreshold(string code, string label)
    {
        return new TieredReturnPlanThreshold(code, label, false, 0.0, 0, TieredReturnPlanThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredReturnPlanThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredReturnPlanThresholdStatus next)
    {
        switch (Status)
        {
            case TieredReturnPlanThresholdStatus.Idle:
                switch (next)
                {
                    case TieredReturnPlanThresholdStatus.Ready:
                        return true;
                }
                return false;
            case TieredReturnPlanThresholdStatus.Warming:
                switch (next)
                {
                    case TieredReturnPlanThresholdStatus.Ready:
                    case TieredReturnPlanThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case TieredReturnPlanThresholdStatus.Ready:
                switch (next)
                {
                    case TieredReturnPlanThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case TieredReturnPlanThresholdStatus.Draining:
                switch (next)
                {
                    case TieredReturnPlanThresholdStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredReturnPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered return plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetValue()
    {
        return Math.Round((double)EndsAt * Budget, 2);
    }

    public TieredReturnPlanThreshold Copy()
    {
        return new TieredReturnPlanThreshold(Code, Label, Approved, Budget, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
