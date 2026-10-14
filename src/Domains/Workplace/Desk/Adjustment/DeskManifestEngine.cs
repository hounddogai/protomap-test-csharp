namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk manifest calculation.</summary>
public interface IDeskManifestModifier
{
    string Describe();
    double Adjust(DeskManifest item, double valueValue);
}

public sealed record RoundingDeskManifestAdjuster(double Step) : IDeskManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitDeskManifestAdjuster(Dictionary<string, double> Factors) : IDeskManifestModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(DeskManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedDeskManifestAdjuster(Registry Flags, string Flag, double Percent) : IDeskManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeskManifestAdjuster(List<double> Bounds, List<double> Rates) : IDeskManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskManifest item, double valueValue)
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

/// <summary>Runs the desk manifest steps in order and keeps a trace of each.</summary>
public sealed class DeskManifestEngine(List<IDeskManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskManifestEngine DefaultDeskManifestEngine(Registry flags)
    {
        return new DeskManifestEngine(
            new List<IDeskManifestModifier>
            {
                new RoundingDeskManifestAdjuster(5.0),
                new UnitDeskManifestAdjuster(new Dictionary<string, double> { ["north"] = 1.04, ["central"] = 0.7 }),
                new FlaggedDeskManifestAdjuster(flags, "desk-manifest-regional", -3.2),
                new TieredDeskManifestAdjuster(
                    new List<double> { 2711.0, 4587.0, 7164.0 },
                    new List<double> { 1.2, 1.04, 0.93 }),
            },
            new List<string>(),
            3.8,
            79075.8);
    }

    public double Compute(DeskManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<DeskManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
