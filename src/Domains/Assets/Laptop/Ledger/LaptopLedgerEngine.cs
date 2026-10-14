namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop ledger calculation.</summary>
public interface ILaptopLedgerFactor
{
    string Describe();
    double Adjust(LaptopLedger item, double valueValue);
}

public sealed record RoundingLaptopLedgerAdjuster(double Step) : ILaptopLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLaptopLedgerAdjuster(double Cap) : ILaptopLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLaptopLedgerAdjuster(Registry Flags, string Flag, double Percent) : ILaptopLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLaptopLedgerAdjuster(double Minimum) : ILaptopLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LaptopLedgerRatingOffsetAdjuster(double Weight) : ILaptopLedgerFactor
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(LaptopLedger item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the laptop ledger steps in order and keeps a trace of each.</summary>
public sealed class LaptopLedgerEngine(List<ILaptopLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopLedgerEngine DefaultLaptopLedgerEngine(Registry flags)
    {
        return new LaptopLedgerEngine(
            new List<ILaptopLedgerFactor>
            {
                new RoundingLaptopLedgerAdjuster(0.05),
                new CapLaptopLedgerAdjuster(40942.0),
                new FlaggedLaptopLedgerAdjuster(flags, "laptop-ledger-global", 8.6),
                new MinimumLaptopLedgerAdjuster(37.8),
                new LaptopLedgerRatingOffsetAdjuster(0.85),
            },
            new List<string>(),
            0.5,
            98216.3);
    }

    public double Run(LaptopLedger item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
