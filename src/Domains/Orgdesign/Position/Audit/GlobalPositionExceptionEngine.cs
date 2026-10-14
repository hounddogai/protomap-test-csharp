namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the global position exception calculation.</summary>
public interface IGlobalPositionExceptionModifier
{
    string Describe();
    double Adjust(GlobalPositionException item, double valueValue);
}

public sealed record FlaggedGlobalPositionExceptionAdjuster(Registry Flags, string Flag, double Percent) : IGlobalPositionExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalPositionException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalPositionExceptionAdjuster(double Minimum) : IGlobalPositionExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalPositionException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalPositionExceptionAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPositionExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPositionException item, double valueValue)
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

/// <summary>Runs the global position exception steps in order and keeps a trace of each.</summary>
public sealed class GlobalPositionExceptionEngine(List<IGlobalPositionExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPositionExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPositionExceptionEngine DefaultGlobalPositionExceptionEngine(Registry flags)
    {
        return new GlobalPositionExceptionEngine(
            new List<IGlobalPositionExceptionModifier>
            {
                new FlaggedGlobalPositionExceptionAdjuster(flags, "global-position-exception-legacy", -5.7),
                new MinimumGlobalPositionExceptionAdjuster(36.2),
                new TieredGlobalPositionExceptionAdjuster(
                    new List<double> { 2780.0, 3874.0, 3909.0 },
                    new List<double> { 1.0, 0.82, 1.06 }),
            },
            new List<string>(),
            2.9,
            43410.2);
    }

    public double Run(GlobalPositionException item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPositionException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
