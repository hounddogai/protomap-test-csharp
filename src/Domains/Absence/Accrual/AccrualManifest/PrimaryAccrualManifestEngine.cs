namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the primary accrual manifest calculation.</summary>
public interface IPrimaryAccrualManifestAdjuster
{
    string Describe();
    double Adjust(PrimaryAccrualManifest item, double valueValue);
}

public sealed record TitlePrimaryAccrualManifestAdjuster(Dictionary<string, double> Factors) : IPrimaryAccrualManifestAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryAccrualManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPrimaryAccrualManifestAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryAccrualManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryAccrualManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryAccrualManifestThresholdOffsetAdjuster(double Weight) : IPrimaryAccrualManifestAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(PrimaryAccrualManifest item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the primary accrual manifest steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAccrualManifestEngine(List<IPrimaryAccrualManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAccrualManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAccrualManifestEngine DefaultPrimaryAccrualManifestEngine(Registry flags)
    {
        return new PrimaryAccrualManifestEngine(
            new List<IPrimaryAccrualManifestAdjuster>
            {
                new TitlePrimaryAccrualManifestAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.35, ["west"] = 0.83, ["south"] = 1.23 }),
                new FlaggedPrimaryAccrualManifestAdjuster(flags, "primary-accrual-manifest-primary", -2.4),
                new PrimaryAccrualManifestThresholdOffsetAdjuster(0.66),
            },
            new List<string>(),
            3.1,
            57319.1);
    }

    public double Run(PrimaryAccrualManifest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAccrualManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
