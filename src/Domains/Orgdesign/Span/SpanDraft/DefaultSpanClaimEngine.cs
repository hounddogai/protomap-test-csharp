namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default span claim calculation.</summary>
public interface IDefaultSpanClaimStep
{
    string Describe();
    double Adjust(DefaultSpanClaim item, double valueValue);
}

public sealed record MinimumDefaultSpanClaimAdjuster(double Minimum) : IDefaultSpanClaimStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultSpanClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDefaultSpanClaimAdjuster(List<double> Bounds, List<double> Rates) : IDefaultSpanClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultSpanClaim item, double valueValue)
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

public sealed record RoundingDefaultSpanClaimAdjuster(double Step) : IDefaultSpanClaimStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultSpanClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default span claim steps in order and keeps a trace of each.</summary>
public sealed class DefaultSpanClaimEngine(List<IDefaultSpanClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSpanClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSpanClaimEngine DefaultDefaultSpanClaimEngine()
    {
        return new DefaultSpanClaimEngine(
            new List<IDefaultSpanClaimStep>
            {
                new MinimumDefaultSpanClaimAdjuster(23.0),
                new TieredDefaultSpanClaimAdjuster(
                    new List<double> { 1577.0, 7918.0 },
                    new List<double> { 1.17, 0.86 }),
                new RoundingDefaultSpanClaimAdjuster(5.0),
            },
            new List<string>(),
            1.2,
            74385.9);
    }

    public double Run(DefaultSpanClaim item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSpanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
