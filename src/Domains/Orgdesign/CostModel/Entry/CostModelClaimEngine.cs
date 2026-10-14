namespace Acme.Workforce.Orgdesign.CostModel.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model claim calculation.</summary>
public interface ICostModelClaimAdjuster
{
    string Describe();
    double Adjust(CostModelClaim item, double valueValue);
}

public sealed record CountryCostModelClaimAdjuster(Dictionary<string, double> Factors) : ICostModelClaimAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(CostModelClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCostModelClaimAdjuster(double Cap) : ICostModelClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumCostModelClaimAdjuster(double Minimum) : ICostModelClaimAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCostModelClaimAdjuster(Registry Flags, string Flag, double Percent) : ICostModelClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCostModelClaimAdjuster(List<double> Bounds, List<double> Rates) : ICostModelClaimAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelClaim item, double valueValue)
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

public sealed record RoundingCostModelClaimAdjuster(double Step) : ICostModelClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the cost model claim steps in order and keeps a trace of each.</summary>
public sealed class CostModelClaimEngine(List<ICostModelClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelClaimEngine DefaultCostModelClaimEngine(Registry flags)
    {
        return new CostModelClaimEngine(
            new List<ICostModelClaimAdjuster>
            {
                new CountryCostModelClaimAdjuster(new Dictionary<string, double> { ["south"] = 0.88 }),
                new CapCostModelClaimAdjuster(40052.0),
                new MinimumCostModelClaimAdjuster(7.9),
                new FlaggedCostModelClaimAdjuster(flags, "cost-model-claim-batched", 4.3),
                new TieredCostModelClaimAdjuster(
                    new List<double> { 5654.0, 6558.0 },
                    new List<double> { 1.03, 0.91 }),
                new RoundingCostModelClaimAdjuster(1.0),
            },
            new List<string>(),
            3.2,
            46008.8);
    }

    public double Price(CostModelClaim item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
