namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default license seat mapping calculation.</summary>
public interface IDefaultLicenseSeatMappingAdjuster
{
    string Describe();
    double Adjust(DefaultLicenseSeatMapping item, double valueValue);
}

public sealed record PercentDefaultLicenseSeatMappingAdjuster(double Percent) : IDefaultLicenseSeatMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultLicenseSeatMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleDefaultLicenseSeatMappingAdjuster(Dictionary<string, double> Factors) : IDefaultLicenseSeatMappingAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultLicenseSeatMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDefaultLicenseSeatMappingAdjuster(List<double> Bounds, List<double> Rates) : IDefaultLicenseSeatMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultLicenseSeatMapping item, double valueValue)
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

public sealed record MinimumDefaultLicenseSeatMappingAdjuster(double Minimum) : IDefaultLicenseSeatMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultLicenseSeatMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultLicenseSeatMappingAdjuster(Registry Flags, string Flag, double Percent) : IDefaultLicenseSeatMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultLicenseSeatMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDefaultLicenseSeatMappingAdjuster(double Step) : IDefaultLicenseSeatMappingAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultLicenseSeatMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the default license seat mapping steps in order and keeps a trace of each.</summary>
public sealed class DefaultLicenseSeatMappingEngine(List<IDefaultLicenseSeatMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultLicenseSeatMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultLicenseSeatMappingEngine DefaultDefaultLicenseSeatMappingEngine(Registry flags)
    {
        return new DefaultLicenseSeatMappingEngine(
            new List<IDefaultLicenseSeatMappingAdjuster>
            {
                new PercentDefaultLicenseSeatMappingAdjuster(-22.7),
                new TitleDefaultLicenseSeatMappingAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.81, ["south"] = 1.16, ["north"] = 1.16 }),
                new TieredDefaultLicenseSeatMappingAdjuster(
                    new List<double> { 4342.0, 7100.0, 7480.0, 8117.0 },
                    new List<double> { 0.94, 0.89, 1.11, 1.08 }),
                new MinimumDefaultLicenseSeatMappingAdjuster(13.1),
                new FlaggedDefaultLicenseSeatMappingAdjuster(flags, "default-license-seat-mapping-regional", 9.8),
                new RoundingDefaultLicenseSeatMappingAdjuster(0.05),
            },
            new List<string>(),
            1.9,
            79689.0);
    }

    public double Price(DefaultLicenseSeatMapping item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultLicenseSeatMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
