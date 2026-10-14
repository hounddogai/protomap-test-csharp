namespace Acme.Workforce.Absence.Policy.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy threshold is in its life cycle.</summary>
public enum PolicyThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A policy threshold as the policy team keeps it.</summary>
public sealed class PolicyThreshold(string code, string name, bool active, string unit, double minimum, double fte, PolicyThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public string Unit { get; set; } = unit;
    public double Minimum { get; set; } = minimum;
    public double Fte { get; set; } = fte;
    public PolicyThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a policy threshold in status new.</summary>
    public static PolicyThreshold NewPolicyThreshold(string code, string name)
    {
        return new PolicyThreshold(code, name, false, "", 0.0, 0.0, PolicyThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyThresholdStatus next)
    {
        switch (Status)
        {
            case PolicyThresholdStatus.New:
                switch (next)
                {
                    case PolicyThresholdStatus.Done:
                        return true;
                }
                return false;
            case PolicyThresholdStatus.Queued:
                switch (next)
                {
                    case PolicyThresholdStatus.Running:
                    case PolicyThresholdStatus.Done:
                        return true;
                }
                return false;
            case PolicyThresholdStatus.Running:
                switch (next)
                {
                    case PolicyThresholdStatus.Failed:
                        return true;
                }
                return false;
            case PolicyThresholdStatus.Done:
                switch (next)
                {
                    case PolicyThresholdStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy threshold"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Fte - Minimum, 2);
    }

    public PolicyThreshold Copy()
    {
        return new PolicyThreshold(Code, Name, Active, Unit, Minimum, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
