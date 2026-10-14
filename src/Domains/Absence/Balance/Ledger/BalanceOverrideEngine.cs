namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance override calculation.</summary>
public interface IBalanceOverrideStep
{
    string Describe();
    double Adjust(BalanceOverride item, double valueValue);
}

public sealed record BalanceOverrideExpiresAtOffsetAdjuster(double Weight) : IBalanceOverrideStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(BalanceOverride item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record FlaggedBalanceOverrideAdjuster(Registry Flags, string Flag, double Percent) : IBalanceOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBalanceOverrideAdjuster(double Percent) : IBalanceOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBalanceOverrideAdjuster(double Minimum) : IBalanceOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBalanceOverrideAdjuster(double Cap) : IBalanceOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBalanceOverrideAdjuster(List<double> Bounds, List<double> Rates) : IBalanceOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BalanceOverride item, double valueValue)
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

/// <summary>Runs the balance override steps in order and keeps a trace of each.</summary>
public sealed class BalanceOverrideEngine(List<IBalanceOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceOverrideEngine DefaultBalanceOverrideEngine(Registry flags)
    {
        return new BalanceOverrideEngine(
            new List<IBalanceOverrideStep>
            {
                new BalanceOverrideExpiresAtOffsetAdjuster(0.24),
                new FlaggedBalanceOverrideAdjuster(flags, "balance-override-tiered", -2.5),
                new PercentBalanceOverrideAdjuster(-3.8),
                new MinimumBalanceOverrideAdjuster(21.7),
                new CapBalanceOverrideAdjuster(9974.0),
                new TieredBalanceOverrideAdjuster(
                    new List<double> { 3264.0, 4279.0, 7840.0 },
                    new List<double> { 1.19, 0.91, 1.04 }),
            },
            new List<string>(),
            1.5,
            54355.4);
    }

    public double Price(BalanceOverride item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
