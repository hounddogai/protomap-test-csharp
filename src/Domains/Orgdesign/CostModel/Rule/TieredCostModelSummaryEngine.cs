namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered cost model summary calculation.</summary>
public interface ITieredCostModelSummaryStep
{
    string Describe();
    double Adjust(TieredCostModelSummary item, double valueValue);
}

public sealed record PercentTieredCostModelSummaryAdjuster(double Percent) : ITieredCostModelSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredCostModelSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTieredCostModelSummaryAdjuster(Registry Flags, string Flag, double Percent) : ITieredCostModelSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredCostModelSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTieredCostModelSummaryAdjuster(List<double> Bounds, List<double> Rates) : ITieredCostModelSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredCostModelSummary item, double valueValue)
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

public sealed record MinimumTieredCostModelSummaryAdjuster(double Minimum) : ITieredCostModelSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredCostModelSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTieredCostModelSummaryAdjuster(double Step) : ITieredCostModelSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredCostModelSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTieredCostModelSummaryAdjuster(double Cap) : ITieredCostModelSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredCostModelSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tiered cost model summary steps in order and keeps a trace of each.</summary>
public sealed class TieredCostModelSummaryEngine(List<ITieredCostModelSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredCostModelSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredCostModelSummaryEngine DefaultTieredCostModelSummaryEngine(Registry flags)
    {
        return new TieredCostModelSummaryEngine(
            new List<ITieredCostModelSummaryStep>
            {
                new PercentTieredCostModelSummaryAdjuster(18.7),
                new FlaggedTieredCostModelSummaryAdjuster(flags, "tiered-cost-model-summary-tiered", 7.5),
                new TieredTieredCostModelSummaryAdjuster(
                    new List<double> { 2025.0, 8328.0, 8363.0, 8441.0 },
                    new List<double> { 0.87, 0.89, 1.13, 1.16 }),
                new MinimumTieredCostModelSummaryAdjuster(24.8),
                new RoundingTieredCostModelSummaryAdjuster(0.25),
                new CapTieredCostModelSummaryAdjuster(7735.0),
            },
            new List<string>(),
            1.7,
            44409.8);
    }

    public double Run(TieredCostModelSummary item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredCostModelSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
