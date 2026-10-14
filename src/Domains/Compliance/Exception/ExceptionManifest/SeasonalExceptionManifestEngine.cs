namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal exception manifest calculation.</summary>
public interface ISeasonalExceptionManifestFactor
{
    string Describe();
    double Adjust(SeasonalExceptionManifest item, double valueValue);
}

public sealed record TieredSeasonalExceptionManifestAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalExceptionManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalExceptionManifest item, double valueValue)
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

public sealed record LabelSeasonalExceptionManifestAdjuster(Dictionary<string, double> Factors) : ISeasonalExceptionManifestFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalExceptionManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSeasonalExceptionManifestAdjuster(double Step) : ISeasonalExceptionManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalExceptionManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SeasonalExceptionManifestWindowDaysOffsetAdjuster(double Weight) : ISeasonalExceptionManifestFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(SeasonalExceptionManifest item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record MinimumSeasonalExceptionManifestAdjuster(double Minimum) : ISeasonalExceptionManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalExceptionManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal exception manifest steps in order and keeps a trace of each.</summary>
public sealed class SeasonalExceptionManifestEngine(List<ISeasonalExceptionManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalExceptionManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalExceptionManifestEngine DefaultSeasonalExceptionManifestEngine()
    {
        return new SeasonalExceptionManifestEngine(
            new List<ISeasonalExceptionManifestFactor>
            {
                new TieredSeasonalExceptionManifestAdjuster(
                    new List<double> { 4054.0, 5207.0 },
                    new List<double> { 0.95, 1.08 }),
                new LabelSeasonalExceptionManifestAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.06, ["retail"] = 1.38, ["north"] = 0.84 }),
                new RoundingSeasonalExceptionManifestAdjuster(0.5),
                new SeasonalExceptionManifestWindowDaysOffsetAdjuster(1.04),
                new MinimumSeasonalExceptionManifestAdjuster(38.9),
            },
            new List<string>(),
            4.0,
            94552.0);
    }

    public double Evaluate(SeasonalExceptionManifest item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalExceptionManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
