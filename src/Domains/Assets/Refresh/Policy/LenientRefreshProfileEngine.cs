namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient refresh profile calculation.</summary>
public interface ILenientRefreshProfileModifier
{
    string Describe();
    double Adjust(LenientRefreshProfile item, double valueValue);
}

public sealed record CapLenientRefreshProfileAdjuster(double Cap) : ILenientRefreshProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientRefreshProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLenientRefreshProfileAdjuster(List<double> Bounds, List<double> Rates) : ILenientRefreshProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientRefreshProfile item, double valueValue)
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

public sealed record RoundingLenientRefreshProfileAdjuster(double Step) : ILenientRefreshProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientRefreshProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient refresh profile steps in order and keeps a trace of each.</summary>
public sealed class LenientRefreshProfileEngine(List<ILenientRefreshProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRefreshProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRefreshProfileEngine DefaultLenientRefreshProfileEngine()
    {
        return new LenientRefreshProfileEngine(
            new List<ILenientRefreshProfileModifier>
            {
                new CapLenientRefreshProfileAdjuster(38974.0),
                new TieredLenientRefreshProfileAdjuster(
                    new List<double> { 3690.0, 5887.0, 7802.0 },
                    new List<double> { 1.07, 0.95, 0.89 }),
                new RoundingLenientRefreshProfileAdjuster(0.05),
            },
            new List<string>(),
            4.4,
            54570.0);
    }

    public double Evaluate(LenientRefreshProfile item)
    {
        var current = item.WeightedValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LenientRefreshProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
