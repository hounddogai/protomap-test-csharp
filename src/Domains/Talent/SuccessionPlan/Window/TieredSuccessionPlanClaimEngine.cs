namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered succession plan claim calculation.</summary>
public interface ITieredSuccessionPlanClaimAdjuster
{
    string Describe();
    double Adjust(TieredSuccessionPlanClaim item, double valueValue);
}

public sealed record TieredSuccessionPlanClaimUpdatedAtOffsetAdjuster(double Weight) : ITieredSuccessionPlanClaimAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(TieredSuccessionPlanClaim item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record CapTieredSuccessionPlanClaimAdjuster(double Cap) : ITieredSuccessionPlanClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredSuccessionPlanClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryTieredSuccessionPlanClaimAdjuster(Dictionary<string, double> Factors) : ITieredSuccessionPlanClaimAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(TieredSuccessionPlanClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredSuccessionPlanClaimAdjuster(List<double> Bounds, List<double> Rates) : ITieredSuccessionPlanClaimAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredSuccessionPlanClaim item, double valueValue)
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

/// <summary>Runs the tiered succession plan claim steps in order and keeps a trace of each.</summary>
public sealed class TieredSuccessionPlanClaimEngine(List<ITieredSuccessionPlanClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSuccessionPlanClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSuccessionPlanClaimEngine DefaultTieredSuccessionPlanClaimEngine()
    {
        return new TieredSuccessionPlanClaimEngine(
            new List<ITieredSuccessionPlanClaimAdjuster>
            {
                new TieredSuccessionPlanClaimUpdatedAtOffsetAdjuster(1.46),
                new CapTieredSuccessionPlanClaimAdjuster(39906.0),
                new CategoryTieredSuccessionPlanClaimAdjuster(new Dictionary<string, double> { ["retail"] = 0.83 }),
                new TieredTieredSuccessionPlanClaimAdjuster(
                    new List<double> { 3235.0, 6038.0, 6893.0 },
                    new List<double> { 1.09, 0.99, 1.15 }),
            },
            new List<string>(),
            3.6,
            48122.5);
    }

    public double Price(TieredSuccessionPlanClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredSuccessionPlanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
