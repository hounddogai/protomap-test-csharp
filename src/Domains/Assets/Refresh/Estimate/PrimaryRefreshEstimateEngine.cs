namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary refresh estimate calculation.</summary>
public interface IPrimaryRefreshEstimateModifier
{
    string Describe();
    double Adjust(PrimaryRefreshEstimate item, double valueValue);
}

public sealed record PercentPrimaryRefreshEstimateAdjuster(double Percent) : IPrimaryRefreshEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryRefreshEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPrimaryRefreshEstimateAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryRefreshEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryRefreshEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPrimaryRefreshEstimateAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryRefreshEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryRefreshEstimate item, double valueValue)
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

public sealed record MinimumPrimaryRefreshEstimateAdjuster(double Minimum) : IPrimaryRefreshEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryRefreshEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PrimaryRefreshEstimateEndsAtOffsetAdjuster(double Weight) : IPrimaryRefreshEstimateModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(PrimaryRefreshEstimate item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the primary refresh estimate steps in order and keeps a trace of each.</summary>
public sealed class PrimaryRefreshEstimateEngine(List<IPrimaryRefreshEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryRefreshEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryRefreshEstimateEngine DefaultPrimaryRefreshEstimateEngine(Registry flags)
    {
        return new PrimaryRefreshEstimateEngine(
            new List<IPrimaryRefreshEstimateModifier>
            {
                new PercentPrimaryRefreshEstimateAdjuster(-6.4),
                new FlaggedPrimaryRefreshEstimateAdjuster(flags, "primary-refresh-estimate-rolling", -3.5),
                new TieredPrimaryRefreshEstimateAdjuster(
                    new List<double> { 1923.0, 4022.0, 5637.0, 9400.0 },
                    new List<double> { 1.08, 0.97, 0.83, 0.95 }),
                new MinimumPrimaryRefreshEstimateAdjuster(20.4),
                new PrimaryRefreshEstimateEndsAtOffsetAdjuster(1.13),
            },
            new List<string>(),
            4.3,
            83342.7);
    }

    public double Run(PrimaryRefreshEstimate item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryRefreshEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
