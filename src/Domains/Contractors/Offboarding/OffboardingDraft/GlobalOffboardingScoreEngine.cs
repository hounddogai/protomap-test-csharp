namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global offboarding score calculation.</summary>
public interface IGlobalOffboardingScoreFactor
{
    string Describe();
    double Adjust(GlobalOffboardingScore item, double valueValue);
}

public sealed record CapGlobalOffboardingScoreAdjuster(double Cap) : IGlobalOffboardingScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalOffboardingScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedGlobalOffboardingScoreAdjuster(Registry Flags, string Flag, double Percent) : IGlobalOffboardingScoreFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalOffboardingScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalOffboardingScoreAdjuster(List<double> Bounds, List<double> Rates) : IGlobalOffboardingScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalOffboardingScore item, double valueValue)
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

public sealed record GlobalOffboardingScoreFloorValueOffsetAdjuster(double Weight) : IGlobalOffboardingScoreFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(GlobalOffboardingScore item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

/// <summary>Runs the global offboarding score steps in order and keeps a trace of each.</summary>
public sealed class GlobalOffboardingScoreEngine(List<IGlobalOffboardingScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalOffboardingScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalOffboardingScoreEngine DefaultGlobalOffboardingScoreEngine(Registry flags)
    {
        return new GlobalOffboardingScoreEngine(
            new List<IGlobalOffboardingScoreFactor>
            {
                new CapGlobalOffboardingScoreAdjuster(24518.0),
                new FlaggedGlobalOffboardingScoreAdjuster(flags, "global-offboarding-score-rolling", 3.9),
                new TieredGlobalOffboardingScoreAdjuster(
                    new List<double> { 6337.0, 7910.0, 8395.0 },
                    new List<double> { 0.82, 1.2, 1.1 }),
                new GlobalOffboardingScoreFloorValueOffsetAdjuster(0.02),
            },
            new List<string>(),
            2.3,
            72430.2);
    }

    public double Run(GlobalOffboardingScore item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalOffboardingScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
