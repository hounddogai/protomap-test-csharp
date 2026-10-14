namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance window calculation.</summary>
public interface IBalanceWindowModifier
{
    string Describe();
    double Adjust(BalanceWindow item, double valueValue);
}

public sealed record NameBalanceWindowAdjuster(Dictionary<string, double> Factors) : IBalanceWindowModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BalanceWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBalanceWindowAdjuster(List<double> Bounds, List<double> Rates) : IBalanceWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BalanceWindow item, double valueValue)
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

public sealed record RoundingBalanceWindowAdjuster(double Step) : IBalanceWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBalanceWindowAdjuster(Registry Flags, string Flag, double Percent) : IBalanceWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBalanceWindowAdjuster(double Percent) : IBalanceWindowModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the balance window steps in order and keeps a trace of each.</summary>
public sealed class BalanceWindowEngine(List<IBalanceWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceWindowEngine DefaultBalanceWindowEngine(Registry flags)
    {
        return new BalanceWindowEngine(
            new List<IBalanceWindowModifier>
            {
                new NameBalanceWindowAdjuster(new Dictionary<string, double> { ["retail"] = 1.38 }),
                new TieredBalanceWindowAdjuster(
                    new List<double> { 2774.0, 5742.0, 5750.0, 6390.0 },
                    new List<double> { 0.96, 0.89, 1.17, 0.87 }),
                new RoundingBalanceWindowAdjuster(0.1),
                new FlaggedBalanceWindowAdjuster(flags, "balance-window-scheduled", -5.6),
                new PercentBalanceWindowAdjuster(14.2),
            },
            new List<string>(),
            3.7,
            86096.5);
    }

    public double Compute(BalanceWindow item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
