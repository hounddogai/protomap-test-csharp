namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>One step of the merit cycle manifest calculation.</summary>
public interface IMeritCycleManifestStep
{
    string Describe();
    double Adjust(MeritCycleManifest item, double valueValue);
}

public sealed record PercentMeritCycleManifestAdjuster(double Percent) : IMeritCycleManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredMeritCycleManifestAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleManifestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleManifest item, double valueValue)
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

public sealed record CapMeritCycleManifestAdjuster(double Cap) : IMeritCycleManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the merit cycle manifest steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleManifestEngine(List<IMeritCycleManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleManifestEngine DefaultMeritCycleManifestEngine()
    {
        return new MeritCycleManifestEngine(
            new List<IMeritCycleManifestStep>
            {
                new PercentMeritCycleManifestAdjuster(-8.4),
                new TieredMeritCycleManifestAdjuster(
                    new List<double> { 1497.0, 5817.0, 9473.0, 9720.0 },
                    new List<double> { 0.86, 1.07, 1.08, 1.06 }),
                new CapMeritCycleManifestAdjuster(13429.0),
            },
            new List<string>(),
            4.2,
            7239.3);
    }

    public double Evaluate(MeritCycleManifest item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
