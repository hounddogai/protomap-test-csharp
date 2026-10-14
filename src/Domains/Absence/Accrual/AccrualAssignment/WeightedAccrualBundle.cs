namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted accrual bundle is in its life cycle.</summary>
public enum WeightedAccrualBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One weighted accrual bundle, with what the planners need to act on it.</summary>
public sealed class WeightedAccrualBundle(string code, string name, int capacity, string badge, double ceiling, WeightedAccrualBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Badge { get; set; } = badge;
    public double Ceiling { get; set; } = ceiling;
    public WeightedAccrualBundleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted accrual bundle in status idle.</summary>
    public static WeightedAccrualBundle NewWeightedAccrualBundle(string code, string name)
    {
        return new WeightedAccrualBundle(code, name, 0, "", 0.0, WeightedAccrualBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAccrualBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAccrualBundleStatus next)
    {
        switch (Status)
        {
            case WeightedAccrualBundleStatus.Idle:
                switch (next)
                {
                    case WeightedAccrualBundleStatus.Warming:
                        return true;
                }
                return false;
            case WeightedAccrualBundleStatus.Warming:
                return next == WeightedAccrualBundleStatus.Ready || next == WeightedAccrualBundleStatus.Draining;
            case WeightedAccrualBundleStatus.Ready:
                return next == WeightedAccrualBundleStatus.Draining || next == WeightedAccrualBundleStatus.Stopped;
            case WeightedAccrualBundleStatus.Draining:
                return next == WeightedAccrualBundleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WeightedAccrualBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted accrual bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)Capacity * Ceiling, 2);
    }

    public WeightedAccrualBundle Copy()
    {
        return new WeightedAccrualBundle(Code, Name, Capacity, Badge, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
