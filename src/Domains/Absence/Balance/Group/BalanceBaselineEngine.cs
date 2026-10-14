namespace Acme.Workforce.Absence.Balance.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance baseline calculation.</summary>
public interface IBalanceBaselineStep
{
    string Describe();
    double Adjust(BalanceBaseline item, double valueValue);
}

public sealed record CapBalanceBaselineAdjuster(double Cap) : IBalanceBaselineStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionBalanceBaselineAdjuster(Dictionary<string, double> Factors) : IBalanceBaselineStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(BalanceBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentBalanceBaselineAdjuster(double Percent) : IBalanceBaselineStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBalanceBaselineAdjuster(Registry Flags, string Flag, double Percent) : IBalanceBaselineStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBalanceBaselineAdjuster(double Minimum) : IBalanceBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the balance baseline steps in order and keeps a trace of each.</summary>
public sealed class BalanceBaselineEngine(List<IBalanceBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceBaselineEngine DefaultBalanceBaselineEngine(Registry flags)
    {
        return new BalanceBaselineEngine(
            new List<IBalanceBaselineStep>
            {
                new CapBalanceBaselineAdjuster(38377.0),
                new RegionBalanceBaselineAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.02, ["online"] = 1.25, ["west"] = 1.23 }),
                new PercentBalanceBaselineAdjuster(-17.4),
                new FlaggedBalanceBaselineAdjuster(flags, "balance-baseline-scheduled", -0.3),
                new MinimumBalanceBaselineAdjuster(47.1),
            },
            new List<string>(),
            3.1,
            22517.3);
    }

    public double Price(BalanceBaseline item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
