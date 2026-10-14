namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive certification plan calculation.</summary>
public interface IAdaptiveCertificationPlanAdjuster
{
    string Describe();
    double Adjust(AdaptiveCertificationPlan item, double valueValue);
}

public sealed record AdaptiveCertificationPlanExpiresAtOffsetAdjuster(double Weight) : IAdaptiveCertificationPlanAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(AdaptiveCertificationPlan item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record FlaggedAdaptiveCertificationPlanAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveCertificationPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveCertificationPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveCertificationPlanAdjuster(double Minimum) : IAdaptiveCertificationPlanAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveCertificationPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the adaptive certification plan steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveCertificationPlanEngine(List<IAdaptiveCertificationPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveCertificationPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveCertificationPlanEngine DefaultAdaptiveCertificationPlanEngine(Registry flags)
    {
        return new AdaptiveCertificationPlanEngine(
            new List<IAdaptiveCertificationPlanAdjuster>
            {
                new AdaptiveCertificationPlanExpiresAtOffsetAdjuster(1.5),
                new FlaggedAdaptiveCertificationPlanAdjuster(flags, "adaptive-certification-plan-capped", 4.1),
                new MinimumAdaptiveCertificationPlanAdjuster(37.6),
            },
            new List<string>(),
            0.5,
            44485.9);
    }

    public double Price(AdaptiveCertificationPlan item)
    {
        var current = item.AdjustedExposure();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<AdaptiveCertificationPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
