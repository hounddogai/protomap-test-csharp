namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient succession plan score calculation.</summary>
public interface ILenientSuccessionPlanScoreFactor
{
    string Describe();
    double Adjust(LenientSuccessionPlanScore item, double valueValue);
}

public sealed record CapLenientSuccessionPlanScoreAdjuster(double Cap) : ILenientSuccessionPlanScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientSuccessionPlanScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryLenientSuccessionPlanScoreAdjuster(Dictionary<string, double> Factors) : ILenientSuccessionPlanScoreFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LenientSuccessionPlanScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLenientSuccessionPlanScoreAdjuster(List<double> Bounds, List<double> Rates) : ILenientSuccessionPlanScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientSuccessionPlanScore item, double valueValue)
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

public sealed record RoundingLenientSuccessionPlanScoreAdjuster(double Step) : ILenientSuccessionPlanScoreFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientSuccessionPlanScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient succession plan score steps in order and keeps a trace of each.</summary>
public sealed class LenientSuccessionPlanScoreEngine(List<ILenientSuccessionPlanScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientSuccessionPlanScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientSuccessionPlanScoreEngine DefaultLenientSuccessionPlanScoreEngine()
    {
        return new LenientSuccessionPlanScoreEngine(
            new List<ILenientSuccessionPlanScoreFactor>
            {
                new CapLenientSuccessionPlanScoreAdjuster(41156.0),
                new CountryLenientSuccessionPlanScoreAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.92, ["online"] = 1.04 }),
                new TieredLenientSuccessionPlanScoreAdjuster(
                    new List<double> { 2021.0, 3588.0, 9869.0 },
                    new List<double> { 0.95, 0.98, 1.05 }),
                new RoundingLenientSuccessionPlanScoreAdjuster(0.5),
            },
            new List<string>(),
            1.8,
            65731.2);
    }

    public double Run(LenientSuccessionPlanScore item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientSuccessionPlanScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
