namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient equity grant ledger calculation.</summary>
public interface ILenientEquityGrantLedgerModifier
{
    string Describe();
    double Adjust(LenientEquityGrantLedger item, double valueValue);
}

public sealed record CapLenientEquityGrantLedgerAdjuster(double Cap) : ILenientEquityGrantLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientEquityGrantLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LenientEquityGrantLedgerAmountOffsetAdjuster(double Weight) : ILenientEquityGrantLedgerModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LenientEquityGrantLedger item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumLenientEquityGrantLedgerAdjuster(double Minimum) : ILenientEquityGrantLedgerModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientEquityGrantLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient equity grant ledger steps in order and keeps a trace of each.</summary>
public sealed class LenientEquityGrantLedgerEngine(List<ILenientEquityGrantLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientEquityGrantLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientEquityGrantLedgerEngine DefaultLenientEquityGrantLedgerEngine()
    {
        return new LenientEquityGrantLedgerEngine(
            new List<ILenientEquityGrantLedgerModifier>
            {
                new CapLenientEquityGrantLedgerAdjuster(3427.0),
                new LenientEquityGrantLedgerAmountOffsetAdjuster(0.17),
                new MinimumLenientEquityGrantLedgerAdjuster(1.3),
            },
            new List<string>(),
            2.7,
            36073.3);
    }

    public double Run(LenientEquityGrantLedger item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientEquityGrantLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
