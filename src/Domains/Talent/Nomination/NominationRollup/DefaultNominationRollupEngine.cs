namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default nomination rollup calculation.</summary>
public interface IDefaultNominationRollupAdjuster
{
    string Describe();
    double Adjust(DefaultNominationRollup item, double valueValue);
}

public sealed record DefaultNominationRollupFloorValueOffsetAdjuster(double Weight) : IDefaultNominationRollupAdjuster
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(DefaultNominationRollup item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumDefaultNominationRollupAdjuster(double Minimum) : IDefaultNominationRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultNominationRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDefaultNominationRollupAdjuster(List<double> Bounds, List<double> Rates) : IDefaultNominationRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultNominationRollup item, double valueValue)
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

public sealed record RoundingDefaultNominationRollupAdjuster(double Step) : IDefaultNominationRollupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultNominationRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultNominationRollupAdjuster(Registry Flags, string Flag, double Percent) : IDefaultNominationRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultNominationRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default nomination rollup steps in order and keeps a trace of each.</summary>
public sealed class DefaultNominationRollupEngine(List<IDefaultNominationRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultNominationRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultNominationRollupEngine DefaultDefaultNominationRollupEngine(Registry flags)
    {
        return new DefaultNominationRollupEngine(
            new List<IDefaultNominationRollupAdjuster>
            {
                new DefaultNominationRollupFloorValueOffsetAdjuster(0.66),
                new MinimumDefaultNominationRollupAdjuster(35.5),
                new TieredDefaultNominationRollupAdjuster(
                    new List<double> { 287.0, 7082.0, 9568.0 },
                    new List<double> { 0.8, 1.09, 0.86 }),
                new RoundingDefaultNominationRollupAdjuster(1.0),
                new FlaggedDefaultNominationRollupAdjuster(flags, "default-nomination-rollup-pending", 7.0),
            },
            new List<string>(),
            1.3,
            95500.1);
    }

    public double Compute(DefaultNominationRollup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultNominationRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
