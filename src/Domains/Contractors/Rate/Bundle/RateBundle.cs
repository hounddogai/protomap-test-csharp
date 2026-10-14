namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate bundle is in its life cycle.</summary>
public enum RateBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rate bundle, with what the planners need to act on it.</summary>
public sealed class RateBundle(string code, string label, bool pinned, double rate, string unit, RateBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public double Rate { get; set; } = rate;
    public string Unit { get; set; } = unit;
    public RateBundleStatus Status { get; set; } = status;

    /// <summary>Starts a rate bundle in status planned.</summary>
    public static RateBundle NewRateBundle(string code, string label)
    {
        return new RateBundle(code, label, false, 0.0, "", RateBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateBundleStatus next)
    {
        switch (Status)
        {
            case RateBundleStatus.Planned:
                switch (next)
                {
                    case RateBundleStatus.Completed:
                        return true;
                }
                return false;
            case RateBundleStatus.Scheduled:
                return next == RateBundleStatus.InProgress || next == RateBundleStatus.Cancelled;
            case RateBundleStatus.InProgress:
                switch (next)
                {
                    case RateBundleStatus.Completed:
                        return true;
                }
                return false;
            case RateBundleStatus.Completed:
                return next == RateBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RateBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rate bundle"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public RateBundle Copy()
    {
        return new RateBundle(Code, Label, Pinned, Rate, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
