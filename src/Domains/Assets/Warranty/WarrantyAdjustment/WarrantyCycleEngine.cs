namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty cycle calculation.</summary>
public interface IWarrantyCycleModifier
{
    string Describe();
    double Adjust(WarrantyCycle item, double valueValue);
}

public sealed record CapWarrantyCycleAdjuster(double Cap) : IWarrantyCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamWarrantyCycleAdjuster(Dictionary<string, double> Factors) : IWarrantyCycleModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(WarrantyCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWarrantyCycleAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWarrantyCycleAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyCycleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyCycle item, double valueValue)
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

public sealed record MinimumWarrantyCycleAdjuster(double Minimum) : IWarrantyCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WarrantyCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the warranty cycle steps in order and keeps a trace of each.</summary>
public sealed class WarrantyCycleEngine(List<IWarrantyCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyCycleEngine DefaultWarrantyCycleEngine(Registry flags)
    {
        return new WarrantyCycleEngine(
            new List<IWarrantyCycleModifier>
            {
                new CapWarrantyCycleAdjuster(16096.0),
                new TeamWarrantyCycleAdjuster(new Dictionary<string, double> { ["retail"] = 1.15 }),
                new FlaggedWarrantyCycleAdjuster(flags, "warranty-cycle-global", 7.3),
                new TieredWarrantyCycleAdjuster(
                    new List<double> { 3652.0, 5244.0, 6846.0 },
                    new List<double> { 0.83, 1.05, 1.17 }),
                new MinimumWarrantyCycleAdjuster(5.4),
            },
            new List<string>(),
            2.9,
            23717.5);
    }

    public double Compute(WarrantyCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
