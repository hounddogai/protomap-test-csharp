namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient retro pay summary calculation.</summary>
public interface ILenientRetroPaySummaryModifier
{
    string Describe();
    double Adjust(LenientRetroPaySummary item, double valueValue);
}

public sealed record LenientRetroPaySummaryEndsAtOffsetAdjuster(double Weight) : ILenientRetroPaySummaryModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(LenientRetroPaySummary item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record CapLenientRetroPaySummaryAdjuster(double Cap) : ILenientRetroPaySummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientRetroPaySummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryLenientRetroPaySummaryAdjuster(Dictionary<string, double> Factors) : ILenientRetroPaySummaryModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(LenientRetroPaySummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLenientRetroPaySummaryAdjuster(double Percent) : ILenientRetroPaySummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientRetroPaySummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientRetroPaySummaryAdjuster(List<double> Bounds, List<double> Rates) : ILenientRetroPaySummaryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientRetroPaySummary item, double valueValue)
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

public sealed record RoundingLenientRetroPaySummaryAdjuster(double Step) : ILenientRetroPaySummaryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientRetroPaySummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient retro pay summary steps in order and keeps a trace of each.</summary>
public sealed class LenientRetroPaySummaryEngine(List<ILenientRetroPaySummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientRetroPaySummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientRetroPaySummaryEngine DefaultLenientRetroPaySummaryEngine()
    {
        return new LenientRetroPaySummaryEngine(
            new List<ILenientRetroPaySummaryModifier>
            {
                new LenientRetroPaySummaryEndsAtOffsetAdjuster(0.4),
                new CapLenientRetroPaySummaryAdjuster(37162.0),
                new CountryLenientRetroPaySummaryAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.75, ["online"] = 1.16 }),
                new PercentLenientRetroPaySummaryAdjuster(-18.5),
                new TieredLenientRetroPaySummaryAdjuster(
                    new List<double> { 245.0, 4321.0, 8147.0 },
                    new List<double> { 1.1, 1.04, 1.14 }),
                new RoundingLenientRetroPaySummaryAdjuster(0.05),
            },
            new List<string>(),
            0.9,
            6151.2);
    }

    public double Run(LenientRetroPaySummary item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientRetroPaySummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
