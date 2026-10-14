namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped holiday manifest calculation.</summary>
public interface ICappedHolidayManifestAdjuster
{
    string Describe();
    double Adjust(CappedHolidayManifest item, double valueValue);
}

public sealed record CapCappedHolidayManifestAdjuster(double Cap) : ICappedHolidayManifestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedHolidayManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedHolidayManifestAdjuster(Registry Flags, string Flag, double Percent) : ICappedHolidayManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedHolidayManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCappedHolidayManifestAdjuster(double Percent) : ICappedHolidayManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedHolidayManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedHolidayManifestAdjuster(List<double> Bounds, List<double> Rates) : ICappedHolidayManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedHolidayManifest item, double valueValue)
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

public sealed record RoundingCappedHolidayManifestAdjuster(double Step) : ICappedHolidayManifestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedHolidayManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCappedHolidayManifestAdjuster(double Minimum) : ICappedHolidayManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedHolidayManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the capped holiday manifest steps in order and keeps a trace of each.</summary>
public sealed class CappedHolidayManifestEngine(List<ICappedHolidayManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedHolidayManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedHolidayManifestEngine DefaultCappedHolidayManifestEngine(Registry flags)
    {
        return new CappedHolidayManifestEngine(
            new List<ICappedHolidayManifestAdjuster>
            {
                new CapCappedHolidayManifestAdjuster(26272.0),
                new FlaggedCappedHolidayManifestAdjuster(flags, "capped-holiday-manifest-fallback", 1.9),
                new PercentCappedHolidayManifestAdjuster(11.7),
                new TieredCappedHolidayManifestAdjuster(
                    new List<double> { 2065.0, 3633.0, 7981.0, 8186.0 },
                    new List<double> { 1.09, 0.91, 0.84, 1.15 }),
                new RoundingCappedHolidayManifestAdjuster(0.25),
                new MinimumCappedHolidayManifestAdjuster(47.6),
            },
            new List<string>(),
            3.7,
            91740.3);
    }

    public double Evaluate(CappedHolidayManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedHolidayManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
