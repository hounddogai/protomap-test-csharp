namespace Acme.Workforce.Absence.Policy.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient policy threshold is in its life cycle.</summary>
public enum LenientPolicyThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient policy threshold as the policy team keeps it.</summary>
public sealed class LenientPolicyThreshold(string code, string name, int durationMinutes, bool approved, bool active, int graceDays, LenientPolicyThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Approved { get; set; } = approved;
    public bool Active { get; set; } = active;
    public int GraceDays { get; set; } = graceDays;
    public LenientPolicyThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a lenient policy threshold in status idle.</summary>
    public static LenientPolicyThreshold NewLenientPolicyThreshold(string code, string name)
    {
        return new LenientPolicyThreshold(code, name, 0, false, false, 0, LenientPolicyThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPolicyThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPolicyThresholdStatus next)
    {
        switch (Status)
        {
            case LenientPolicyThresholdStatus.Idle:
                switch (next)
                {
                    case LenientPolicyThresholdStatus.Draining:
                        return true;
                }
                return false;
            case LenientPolicyThresholdStatus.Warming:
                switch (next)
                {
                    case LenientPolicyThresholdStatus.Draining:
                    case LenientPolicyThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case LenientPolicyThresholdStatus.Ready:
                switch (next)
                {
                    case LenientPolicyThresholdStatus.Draining:
                        return true;
                }
                return false;
            case LenientPolicyThresholdStatus.Draining:
                return next == LenientPolicyThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LenientPolicyThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient policy threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetValue()
    {
        return Math.Round((double)GraceDays + (double)DurationMinutes, 2);
    }

    public LenientPolicyThreshold Copy()
    {
        return new LenientPolicyThreshold(Code, Name, DurationMinutes, Approved, Active, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
