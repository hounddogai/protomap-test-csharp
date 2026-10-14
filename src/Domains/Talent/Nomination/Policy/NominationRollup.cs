namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination rollup is in its life cycle.</summary>
public enum NominationRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One nomination rollup, with what the planners need to act on it.</summary>
public sealed class NominationRollup(string code, string label, double ceiling, double weight, double rate, bool active, NominationRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public bool Active { get; set; } = active;
    public NominationRollupStatus Status { get; set; } = status;

    /// <summary>Starts a nomination rollup in status idle.</summary>
    public static NominationRollup NewNominationRollup(string code, string label)
    {
        return new NominationRollup(code, label, 0.0, 0.0, 0.0, false, NominationRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationRollupStatus next)
    {
        switch (Status)
        {
            case NominationRollupStatus.Idle:
                switch (next)
                {
                    case NominationRollupStatus.Draining:
                    case NominationRollupStatus.Stopped:
                        return true;
                }
                return false;
            case NominationRollupStatus.Warming:
                switch (next)
                {
                    case NominationRollupStatus.Draining:
                        return true;
                }
                return false;
            case NominationRollupStatus.Ready:
                return next == NominationRollupStatus.Draining || next == NominationRollupStatus.Stopped;
            case NominationRollupStatus.Draining:
                return next == NominationRollupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(NominationRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"nomination rollup"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Weight * Ceiling, 2);
    }

    public NominationRollup Copy()
    {
        return new NominationRollup(Code, Label, Ceiling, Weight, Rate, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
