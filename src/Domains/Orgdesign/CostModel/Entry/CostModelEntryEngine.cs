namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model entry calculation.</summary>
public interface ICostModelEntryFactor
{
    string Describe();
    double Adjust(CostModelEntry item, double valueValue);
}

public sealed record FlaggedCostModelEntryAdjuster(Registry Flags, string Flag, double Percent) : ICostModelEntryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCostModelEntryAdjuster(double Percent) : ICostModelEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCostModelEntryAdjuster(double Minimum) : ICostModelEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RegionCostModelEntryAdjuster(Dictionary<string, double> Factors) : ICostModelEntryFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CostModelEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the cost model entry steps in order and keeps a trace of each.</summary>
public sealed class CostModelEntryEngine(List<ICostModelEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelEntryEngine DefaultCostModelEntryEngine(Registry flags)
    {
        return new CostModelEntryEngine(
            new List<ICostModelEntryFactor>
            {
                new FlaggedCostModelEntryAdjuster(flags, "cost-model-entry-pending", 8.5),
                new PercentCostModelEntryAdjuster(18.4),
                new MinimumCostModelEntryAdjuster(46.7),
                new RegionCostModelEntryAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.25, ["online"] = 1.14 }),
            },
            new List<string>(),
            3.6,
            90821.0);
    }

    public double Evaluate(CostModelEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<CostModelEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
