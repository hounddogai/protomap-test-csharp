namespace Acme.Workforce.Orgdesign.CostModel.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model mapping calculation.</summary>
public interface ICostModelMappingModifier
{
    string Describe();
    double Adjust(CostModelMapping item, double valueValue);
}

public sealed record FlaggedCostModelMappingAdjuster(Registry Flags, string Flag, double Percent) : ICostModelMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCostModelMappingAdjuster(double Percent) : ICostModelMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCostModelMappingAdjuster(double Minimum) : ICostModelMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCostModelMappingAdjuster(List<double> Bounds, List<double> Rates) : ICostModelMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelMapping item, double valueValue)
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

public sealed record RoundingCostModelMappingAdjuster(double Step) : ICostModelMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapCostModelMappingAdjuster(double Cap) : ICostModelMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the cost model mapping steps in order and keeps a trace of each.</summary>
public sealed class CostModelMappingEngine(List<ICostModelMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelMappingEngine DefaultCostModelMappingEngine(Registry flags)
    {
        return new CostModelMappingEngine(
            new List<ICostModelMappingModifier>
            {
                new FlaggedCostModelMappingAdjuster(flags, "cost-model-mapping-global", 8.1),
                new PercentCostModelMappingAdjuster(21.8),
                new MinimumCostModelMappingAdjuster(23.8),
                new TieredCostModelMappingAdjuster(
                    new List<double> { 3897.0, 5470.0, 5833.0, 9541.0 },
                    new List<double> { 1.08, 0.96, 1.06, 0.98 }),
                new RoundingCostModelMappingAdjuster(0.05),
                new CapCostModelMappingAdjuster(39386.0),
            },
            new List<string>(),
            1.5,
            87845.4);
    }

    public double Evaluate(CostModelMapping item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
