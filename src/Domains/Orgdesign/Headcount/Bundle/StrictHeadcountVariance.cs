namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict headcount variance is in its life cycle.</summary>
public enum StrictHeadcountVarianceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One strict headcount variance, with what the planners need to act on it.</summary>
public sealed class StrictHeadcountVariance(string code, string label, string costCenter, double rating, string unit, string currency, int version, StrictHeadcountVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public double Rating { get; set; } = rating;
    public string Unit { get; set; } = unit;
    public string Currency { get; set; } = currency;
    public int Version { get; set; } = version;
    public StrictHeadcountVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a strict headcount variance in status idle.</summary>
    public static StrictHeadcountVariance NewStrictHeadcountVariance(string code, string label)
    {
        return new StrictHeadcountVariance(code, label, "", 0.0, "", "", 0, StrictHeadcountVarianceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictHeadcountVarianceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictHeadcountVarianceStatus next)
    {
        switch (Status)
        {
            case StrictHeadcountVarianceStatus.Idle:
                return next == StrictHeadcountVarianceStatus.Ready;
            case StrictHeadcountVarianceStatus.Warming:
                switch (next)
                {
                    case StrictHeadcountVarianceStatus.Ready:
                        return true;
                }
                return false;
            case StrictHeadcountVarianceStatus.Ready:
                return next == StrictHeadcountVarianceStatus.Stopped;
            case StrictHeadcountVarianceStatus.Draining:
                switch (next)
                {
                    case StrictHeadcountVarianceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictHeadcountVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict headcount variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"strict headcount variance"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Rating * (double)Version, 2);
    }

    public StrictHeadcountVariance Copy()
    {
        return new StrictHeadcountVariance(Code, Label, CostCenter, Rating, Unit, Currency, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
