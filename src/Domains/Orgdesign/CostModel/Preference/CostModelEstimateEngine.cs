namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model estimate calculation.</summary>
public interface ICostModelEstimateStep
{
    string Describe();
    double Adjust(CostModelEstimate item, double valueValue);
}

public sealed record RoundingCostModelEstimateAdjuster(double Step) : ICostModelEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCostModelEstimateAdjuster(Registry Flags, string Flag, double Percent) : ICostModelEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCostModelEstimateAdjuster(List<double> Bounds, List<double> Rates) : ICostModelEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelEstimate item, double valueValue)
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

public sealed record SegmentCostModelEstimateAdjuster(Dictionary<string, double> Factors) : ICostModelEstimateStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(CostModelEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCostModelEstimateAdjuster(double Cap) : ICostModelEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCostModelEstimateAdjuster(double Percent) : ICostModelEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the cost model estimate steps in order and keeps a trace of each.</summary>
public sealed class CostModelEstimateEngine(List<ICostModelEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelEstimateEngine DefaultCostModelEstimateEngine(Registry flags)
    {
        return new CostModelEstimateEngine(
            new List<ICostModelEstimateStep>
            {
                new RoundingCostModelEstimateAdjuster(0.25),
                new FlaggedCostModelEstimateAdjuster(flags, "cost-model-estimate-capped", -8.7),
                new TieredCostModelEstimateAdjuster(
                    new List<double> { 846.0, 3330.0, 5041.0, 9738.0 },
                    new List<double> { 1.03, 0.99, 1.12, 1.02 }),
                new SegmentCostModelEstimateAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.88, ["north"] = 1.21, ["east"] = 1.39 }),
                new CapCostModelEstimateAdjuster(6901.0),
                new PercentCostModelEstimateAdjuster(-24.0),
            },
            new List<string>(),
            1.2,
            14108.0);
    }

    public double Compute(CostModelEstimate item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
