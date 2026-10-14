namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy exception profile calculation.</summary>
public interface ILegacyExceptionProfileStep
{
    string Describe();
    double Adjust(LegacyExceptionProfile item, double valueValue);
}

public sealed record TieredLegacyExceptionProfileAdjuster(List<double> Bounds, List<double> Rates) : ILegacyExceptionProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyExceptionProfile item, double valueValue)
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

public sealed record FlaggedLegacyExceptionProfileAdjuster(Registry Flags, string Flag, double Percent) : ILegacyExceptionProfileStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyExceptionProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLegacyExceptionProfileAdjuster(double Percent) : ILegacyExceptionProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyExceptionProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyExceptionProfileAdjuster(double Cap) : ILegacyExceptionProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyExceptionProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLegacyExceptionProfileAdjuster(double Step) : ILegacyExceptionProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyExceptionProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy exception profile steps in order and keeps a trace of each.</summary>
public sealed class LegacyExceptionProfileEngine(List<ILegacyExceptionProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyExceptionProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyExceptionProfileEngine DefaultLegacyExceptionProfileEngine(Registry flags)
    {
        return new LegacyExceptionProfileEngine(
            new List<ILegacyExceptionProfileStep>
            {
                new TieredLegacyExceptionProfileAdjuster(
                    new List<double> { 915.0, 1381.0 },
                    new List<double> { 0.98, 0.95 }),
                new FlaggedLegacyExceptionProfileAdjuster(flags, "legacy-exception-profile-tiered", -2.4),
                new PercentLegacyExceptionProfileAdjuster(-6.9),
                new CapLegacyExceptionProfileAdjuster(21752.0),
                new RoundingLegacyExceptionProfileAdjuster(0.25),
            },
            new List<string>(),
            4.6,
            70205.7);
    }

    public double Evaluate(LegacyExceptionProfile item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyExceptionProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
