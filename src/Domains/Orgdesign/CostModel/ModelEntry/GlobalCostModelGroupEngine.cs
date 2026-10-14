namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global cost model group calculation.</summary>
public interface IGlobalCostModelGroupStep
{
    string Describe();
    double Adjust(GlobalCostModelGroup item, double valueValue);
}

public sealed record CapGlobalCostModelGroupAdjuster(double Cap) : IGlobalCostModelGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalCostModelGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record GlobalCostModelGroupCapacityOffsetAdjuster(double Weight) : IGlobalCostModelGroupStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(GlobalCostModelGroup item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record RoundingGlobalCostModelGroupAdjuster(double Step) : IGlobalCostModelGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalCostModelGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedGlobalCostModelGroupAdjuster(Registry Flags, string Flag, double Percent) : IGlobalCostModelGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalCostModelGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentGlobalCostModelGroupAdjuster(double Percent) : IGlobalCostModelGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalCostModelGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryGlobalCostModelGroupAdjuster(Dictionary<string, double> Factors) : IGlobalCostModelGroupStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(GlobalCostModelGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global cost model group steps in order and keeps a trace of each.</summary>
public sealed class GlobalCostModelGroupEngine(List<IGlobalCostModelGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalCostModelGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalCostModelGroupEngine DefaultGlobalCostModelGroupEngine(Registry flags)
    {
        return new GlobalCostModelGroupEngine(
            new List<IGlobalCostModelGroupStep>
            {
                new CapGlobalCostModelGroupAdjuster(19441.0),
                new GlobalCostModelGroupCapacityOffsetAdjuster(1.31),
                new RoundingGlobalCostModelGroupAdjuster(0.25),
                new FlaggedGlobalCostModelGroupAdjuster(flags, "global-cost-model-group-scheduled", 7.8),
                new PercentGlobalCostModelGroupAdjuster(-8.4),
                new CategoryGlobalCostModelGroupAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.28, ["east"] = 0.8, ["west"] = 1.32 }),
            },
            new List<string>(),
            4.1,
            64667.6);
    }

    public double Evaluate(GlobalCostModelGroup item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalCostModelGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
