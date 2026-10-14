namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default warranty cycle calculation.</summary>
public interface IDefaultWarrantyCycleAdjuster
{
    string Describe();
    double Adjust(DefaultWarrantyCycle item, double valueValue);
}

public sealed record FlaggedDefaultWarrantyCycleAdjuster(Registry Flags, string Flag, double Percent) : IDefaultWarrantyCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultWarrantyCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultWarrantyCycleAdjuster(double Cap) : IDefaultWarrantyCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultWarrantyCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultWarrantyCycleAdjuster(double Minimum) : IDefaultWarrantyCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultWarrantyCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default warranty cycle steps in order and keeps a trace of each.</summary>
public sealed class DefaultWarrantyCycleEngineExtra(List<IDefaultWarrantyCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultWarrantyCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public double Evaluate(DefaultWarrantyCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultWarrantyCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}

/// <summary>Functions of default warranty cycle engine.</summary>
public static class DefaultWarrantyCycleEngine
{
    public static DefaultWarrantyCycleEngineExtra DefaultDefaultWarrantyCycleEngine(Registry flags)
    {
        return new DefaultWarrantyCycleEngineExtra(
            new List<IDefaultWarrantyCycleAdjuster>
            {
                new FlaggedDefaultWarrantyCycleAdjuster(flags, "default-warranty-cycle-tiered", -5.3),
                new CapDefaultWarrantyCycleAdjuster(40508.0),
                new MinimumDefaultWarrantyCycleAdjuster(6.7),
            },
            new List<string>(),
            0.6,
            18003.3);
    }
}
