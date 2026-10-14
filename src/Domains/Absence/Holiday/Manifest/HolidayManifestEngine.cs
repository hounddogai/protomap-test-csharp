namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the holiday manifest calculation.</summary>
public interface IHolidayManifestModifier
{
    string Describe();
    double Adjust(HolidayManifest item, double valueValue);
}

public sealed record SourceHolidayManifestAdjuster(Dictionary<string, double> Factors) : IHolidayManifestModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(HolidayManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentHolidayManifestAdjuster(double Percent) : IHolidayManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedHolidayManifestAdjuster(Registry Flags, string Flag, double Percent) : IHolidayManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HolidayManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the holiday manifest steps in order and keeps a trace of each.</summary>
public sealed class HolidayManifestEngine(List<IHolidayManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayManifestEngine DefaultHolidayManifestEngine(Registry flags)
    {
        return new HolidayManifestEngine(
            new List<IHolidayManifestModifier>
            {
                new SourceHolidayManifestAdjuster(new Dictionary<string, double> { ["central"] = 1.06 }),
                new PercentHolidayManifestAdjuster(-15.0),
                new FlaggedHolidayManifestAdjuster(flags, "holiday-manifest-tiered", 0.0),
            },
            new List<string>(),
            3.6,
            5332.5);
    }

    public double Compute(HolidayManifest item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<HolidayManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
