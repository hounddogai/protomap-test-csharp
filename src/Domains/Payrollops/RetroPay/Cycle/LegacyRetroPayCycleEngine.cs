namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy retro pay cycle calculation.</summary>
public interface ILegacyRetroPayCycleAdjuster
{
    string Describe();
    double Adjust(LegacyRetroPayCycle item, double valueValue);
}

public sealed record PercentLegacyRetroPayCycleAdjuster(double Percent) : ILegacyRetroPayCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyRetroPayCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyRetroPayCycleAdjuster(double Step) : ILegacyRetroPayCycleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyRetroPayCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyRetroPayCycleAdjuster(List<double> Bounds, List<double> Rates) : ILegacyRetroPayCycleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyRetroPayCycle item, double valueValue)
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

public sealed record LegacyRetroPayCycleAmountOffsetAdjuster(double Weight) : ILegacyRetroPayCycleAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LegacyRetroPayCycle item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record FlaggedLegacyRetroPayCycleAdjuster(Registry Flags, string Flag, double Percent) : ILegacyRetroPayCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyRetroPayCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy retro pay cycle steps in order and keeps a trace of each.</summary>
public sealed class LegacyRetroPayCycleEngine(List<ILegacyRetroPayCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyRetroPayCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyRetroPayCycleEngine DefaultLegacyRetroPayCycleEngine(Registry flags)
    {
        return new LegacyRetroPayCycleEngine(
            new List<ILegacyRetroPayCycleAdjuster>
            {
                new PercentLegacyRetroPayCycleAdjuster(24.5),
                new RoundingLegacyRetroPayCycleAdjuster(0.5),
                new TieredLegacyRetroPayCycleAdjuster(
                    new List<double> { 4214.0, 8775.0 },
                    new List<double> { 0.97, 0.99 }),
                new LegacyRetroPayCycleAmountOffsetAdjuster(0.3),
                new FlaggedLegacyRetroPayCycleAdjuster(flags, "legacy-retro-pay-cycle-default", 9.9),
            },
            new List<string>(),
            0.9,
            52001.0);
    }

    public double Compute(LegacyRetroPayCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyRetroPayCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
