namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy license seat window calculation.</summary>
public interface ILegacyLicenseSeatWindowAdjuster
{
    string Describe();
    double Adjust(LegacyLicenseSeatWindow item, double valueValue);
}

public sealed record TieredLegacyLicenseSeatWindowAdjuster(List<double> Bounds, List<double> Rates) : ILegacyLicenseSeatWindowAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyLicenseSeatWindow item, double valueValue)
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

public sealed record MinimumLegacyLicenseSeatWindowAdjuster(double Minimum) : ILegacyLicenseSeatWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyLicenseSeatWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyLicenseSeatWindowAdjuster(Registry Flags, string Flag, double Percent) : ILegacyLicenseSeatWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyLicenseSeatWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyLicenseSeatWindowPriorityOffsetAdjuster(double Weight) : ILegacyLicenseSeatWindowAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(LegacyLicenseSeatWindow item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record PercentLegacyLicenseSeatWindowAdjuster(double Percent) : ILegacyLicenseSeatWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyLicenseSeatWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleLegacyLicenseSeatWindowAdjuster(Dictionary<string, double> Factors) : ILegacyLicenseSeatWindowAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyLicenseSeatWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy license seat window steps in order and keeps a trace of each.</summary>
public sealed class LegacyLicenseSeatWindowEngine(List<ILegacyLicenseSeatWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyLicenseSeatWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyLicenseSeatWindowEngine DefaultLegacyLicenseSeatWindowEngine(Registry flags)
    {
        return new LegacyLicenseSeatWindowEngine(
            new List<ILegacyLicenseSeatWindowAdjuster>
            {
                new TieredLegacyLicenseSeatWindowAdjuster(
                    new List<double> { 104.0, 5213.0, 8737.0 },
                    new List<double> { 0.9, 1.1, 0.81 }),
                new MinimumLegacyLicenseSeatWindowAdjuster(8.5),
                new FlaggedLegacyLicenseSeatWindowAdjuster(flags, "legacy-license-seat-window-weighted", -3.0),
                new LegacyLicenseSeatWindowPriorityOffsetAdjuster(1.1),
                new PercentLegacyLicenseSeatWindowAdjuster(-11.8),
                new TitleLegacyLicenseSeatWindowAdjuster(new Dictionary<string, double> { ["central"] = 0.7 }),
            },
            new List<string>(),
            0.8,
            32777.7);
    }

    public double Price(LegacyLicenseSeatWindow item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyLicenseSeatWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
