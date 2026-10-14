namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path rollup calculation.</summary>
public interface ICareerPathRollupFactor
{
    string Describe();
    double Adjust(CareerPathRollup item, double valueValue);
}

public sealed record FlaggedCareerPathRollupAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCareerPathRollupAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathRollup item, double valueValue)
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

public sealed record MinimumCareerPathRollupAdjuster(double Minimum) : ICareerPathRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LocaleCareerPathRollupAdjuster(Dictionary<string, double> Factors) : ICareerPathRollupFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(CareerPathRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the career path rollup steps in order and keeps a trace of each.</summary>
public sealed class CareerPathRollupEngine(List<ICareerPathRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathRollupEngine DefaultCareerPathRollupEngine(Registry flags)
    {
        return new CareerPathRollupEngine(
            new List<ICareerPathRollupFactor>
            {
                new FlaggedCareerPathRollupAdjuster(flags, "career-path-rollup-manual", -8.8),
                new TieredCareerPathRollupAdjuster(
                    new List<double> { 4979.0, 8944.0 },
                    new List<double> { 1.15, 0.85 }),
                new MinimumCareerPathRollupAdjuster(1.4),
                new LocaleCareerPathRollupAdjuster(new Dictionary<string, double> { ["north"] = 0.99 }),
            },
            new List<string>(),
            4.2,
            75714.6);
    }

    public double Compute(CareerPathRollup item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
