namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan digest calculation.</summary>
public interface IReturnPlanDigestFactor
{
    string Describe();
    double Adjust(ReturnPlanDigest item, double valueValue);
}

public sealed record ReturnPlanDigestMarginOffsetAdjuster(double Weight) : IReturnPlanDigestFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(ReturnPlanDigest item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record RegionReturnPlanDigestAdjuster(Dictionary<string, double> Factors) : IReturnPlanDigestFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(ReturnPlanDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredReturnPlanDigestAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanDigestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanDigest item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record MinimumReturnPlanDigestAdjuster(double Minimum) : IReturnPlanDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingReturnPlanDigestAdjuster(double Step) : IReturnPlanDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedReturnPlanDigestAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the return plan digest steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanDigestEngine(List<IReturnPlanDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanDigestEngine DefaultReturnPlanDigestEngine(Registry flags)
    {
        return new ReturnPlanDigestEngine(
            new List<IReturnPlanDigestFactor>
            {
                new ReturnPlanDigestMarginOffsetAdjuster(0.58),
                new RegionReturnPlanDigestAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.84, ["online"] = 1.39 }),
                new TieredReturnPlanDigestAdjuster(
                    new List<double> { 2805.0, 8205.0 },
                    new List<double> { 0.88, 0.8 }),
                new MinimumReturnPlanDigestAdjuster(38.7),
                new RoundingReturnPlanDigestAdjuster(0.25),
                new FlaggedReturnPlanDigestAdjuster(flags, "return-plan-digest-default", 8.0),
            },
            new List<string>(),
            2.4,
            84008.1);
    }

    public double Compute(ReturnPlanDigest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
