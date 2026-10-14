namespace Acme.Workforce.Talent.Pool.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool ledger calculation.</summary>
public interface IPoolLedgerFactor
{
    string Describe();
    double Adjust(PoolLedger item, double valueValue);
}

public sealed record TieredPoolLedgerAdjuster(List<double> Bounds, List<double> Rates) : IPoolLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolLedger item, double valueValue)
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

public sealed record RoundingPoolLedgerAdjuster(double Step) : IPoolLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPoolLedgerAdjuster(Registry Flags, string Flag, double Percent) : IPoolLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPoolLedgerAdjuster(double Minimum) : IPoolLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PoolLedgerMaximumOffsetAdjuster(double Weight) : IPoolLedgerFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(PoolLedger item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record CapPoolLedgerAdjuster(double Cap) : IPoolLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pool ledger steps in order and keeps a trace of each.</summary>
public sealed class PoolLedgerEngine(List<IPoolLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolLedgerEngine DefaultPoolLedgerEngine(Registry flags)
    {
        return new PoolLedgerEngine(
            new List<IPoolLedgerFactor>
            {
                new TieredPoolLedgerAdjuster(
                    new List<double> { 1037.0, 3707.0, 9859.0 },
                    new List<double> { 1.11, 0.82, 0.83 }),
                new RoundingPoolLedgerAdjuster(0.05),
                new FlaggedPoolLedgerAdjuster(flags, "pool-ledger-default", -6.6),
                new MinimumPoolLedgerAdjuster(7.0),
                new PoolLedgerMaximumOffsetAdjuster(0.97),
                new CapPoolLedgerAdjuster(23512.0),
            },
            new List<string>(),
            1.4,
            18122.7);
    }

    public double Price(PoolLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<PoolLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
