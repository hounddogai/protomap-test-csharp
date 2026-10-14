namespace Acme.Workforce.Absence.Balance.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered balance target calculation.</summary>
public interface ITieredBalanceTargetModifier
{
    string Describe();
    double Adjust(TieredBalanceTarget item, double valueValue);
}

public sealed record CapTieredBalanceTargetAdjuster(double Cap) : ITieredBalanceTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredBalanceTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTieredBalanceTargetAdjuster(double Minimum) : ITieredBalanceTargetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredBalanceTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTieredBalanceTargetAdjuster(List<double> Bounds, List<double> Rates) : ITieredBalanceTargetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredBalanceTarget item, double valueValue)
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

public sealed record TieredBalanceTargetWindowDaysOffsetAdjuster(double Weight) : ITieredBalanceTargetModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(TieredBalanceTarget item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record PercentTieredBalanceTargetAdjuster(double Percent) : ITieredBalanceTargetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredBalanceTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered balance target steps in order and keeps a trace of each.</summary>
public sealed class TieredBalanceTargetEngine(List<ITieredBalanceTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredBalanceTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredBalanceTargetEngine DefaultTieredBalanceTargetEngine()
    {
        return new TieredBalanceTargetEngine(
            new List<ITieredBalanceTargetModifier>
            {
                new CapTieredBalanceTargetAdjuster(10198.0),
                new MinimumTieredBalanceTargetAdjuster(16.4),
                new TieredTieredBalanceTargetAdjuster(
                    new List<double> { 4687.0, 7280.0 },
                    new List<double> { 1.17, 1.15 }),
                new TieredBalanceTargetWindowDaysOffsetAdjuster(0.56),
                new PercentTieredBalanceTargetAdjuster(-7.7),
            },
            new List<string>(),
            2.3,
            52728.1);
    }

    public double Price(TieredBalanceTarget item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredBalanceTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
