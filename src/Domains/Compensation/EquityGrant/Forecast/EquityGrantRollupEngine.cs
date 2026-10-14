namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant rollup calculation.</summary>
public interface IEquityGrantRollupFactor
{
    string Describe();
    double Adjust(EquityGrantRollup item, double valueValue);
}

public sealed record FlaggedEquityGrantRollupAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEquityGrantRollupAdjuster(double Step) : IEquityGrantRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapEquityGrantRollupAdjuster(double Cap) : IEquityGrantRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record EquityGrantRollupLimitOffsetAdjuster(double Weight) : IEquityGrantRollupFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(EquityGrantRollup item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the equity grant rollup steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantRollupEngine(List<IEquityGrantRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantRollupEngine DefaultEquityGrantRollupEngine(Registry flags)
    {
        return new EquityGrantRollupEngine(
            new List<IEquityGrantRollupFactor>
            {
                new FlaggedEquityGrantRollupAdjuster(flags, "equity-grant-rollup-fallback", -7.2),
                new RoundingEquityGrantRollupAdjuster(0.5),
                new CapEquityGrantRollupAdjuster(48924.0),
                new EquityGrantRollupLimitOffsetAdjuster(1.52),
            },
            new List<string>(),
            2.3,
            48751.7);
    }

    public double Evaluate(EquityGrantRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
