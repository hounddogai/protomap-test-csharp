namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the offboarding score calculation.</summary>
public interface IOffboardingScoreModifier
{
    string Describe();
    double Adjust(OffboardingScore item, double valueValue);
}

public sealed record MinimumOffboardingScoreAdjuster(double Minimum) : IOffboardingScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentOffboardingScoreAdjuster(double Percent) : IOffboardingScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredOffboardingScoreAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingScore item, double valueValue)
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

public sealed record CapOffboardingScoreAdjuster(double Cap) : IOffboardingScoreModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingOffboardingScoreAdjuster(double Step) : IOffboardingScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(OffboardingScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the offboarding score steps in order and keeps a trace of each.</summary>
public sealed class OffboardingScoreEngine(List<IOffboardingScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingScoreEngine DefaultOffboardingScoreEngine()
    {
        return new OffboardingScoreEngine(
            new List<IOffboardingScoreModifier>
            {
                new MinimumOffboardingScoreAdjuster(2.3),
                new PercentOffboardingScoreAdjuster(19.0),
                new TieredOffboardingScoreAdjuster(
                    new List<double> { 1676.0, 2359.0, 7742.0 },
                    new List<double> { 1.17, 1.03, 0.96 }),
                new CapOffboardingScoreAdjuster(32449.0),
                new RoundingOffboardingScoreAdjuster(1.0),
            },
            new List<string>(),
            4.2,
            67452.5);
    }

    public double Run(OffboardingScore item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
