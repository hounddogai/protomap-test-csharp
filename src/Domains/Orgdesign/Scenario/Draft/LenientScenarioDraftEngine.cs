namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient scenario draft calculation.</summary>
public interface ILenientScenarioDraftModifier
{
    string Describe();
    double Adjust(LenientScenarioDraft item, double valueValue);
}

public sealed record LenientScenarioDraftHoursOffsetAdjuster(double Weight) : ILenientScenarioDraftModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LenientScenarioDraft item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record CapLenientScenarioDraftAdjuster(double Cap) : ILenientScenarioDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientScenarioDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientScenarioDraftAdjuster(double Percent) : ILenientScenarioDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientScenarioDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLenientScenarioDraftAdjuster(Registry Flags, string Flag, double Percent) : ILenientScenarioDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientScenarioDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientScenarioDraftAdjuster(List<double> Bounds, List<double> Rates) : ILenientScenarioDraftModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientScenarioDraft item, double valueValue)
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

/// <summary>Runs the lenient scenario draft steps in order and keeps a trace of each.</summary>
public sealed class LenientScenarioDraftEngine(List<ILenientScenarioDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientScenarioDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientScenarioDraftEngine DefaultLenientScenarioDraftEngine(Registry flags)
    {
        return new LenientScenarioDraftEngine(
            new List<ILenientScenarioDraftModifier>
            {
                new LenientScenarioDraftHoursOffsetAdjuster(0.72),
                new CapLenientScenarioDraftAdjuster(12056.0),
                new PercentLenientScenarioDraftAdjuster(6.6),
                new FlaggedLenientScenarioDraftAdjuster(flags, "lenient-scenario-draft-fallback", -4.2),
                new TieredLenientScenarioDraftAdjuster(
                    new List<double> { 972.0, 3051.0, 5200.0 },
                    new List<double> { 1.19, 1.06, 1.18 }),
            },
            new List<string>(),
            4.3,
            97844.3);
    }

    public double Evaluate(LenientScenarioDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientScenarioDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
