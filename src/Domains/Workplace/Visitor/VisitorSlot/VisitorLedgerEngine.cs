namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor ledger calculation.</summary>
public interface IVisitorLedgerStep
{
    string Describe();
    double Adjust(VisitorLedger item, double valueValue);
}

public sealed record RoundingVisitorLedgerAdjuster(double Step) : IVisitorLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record VisitorLedgerMaximumOffsetAdjuster(double Weight) : IVisitorLedgerStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(VisitorLedger item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record CapVisitorLedgerAdjuster(double Cap) : IVisitorLedgerStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleVisitorLedgerAdjuster(Dictionary<string, double> Factors) : IVisitorLedgerStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(VisitorLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredVisitorLedgerAdjuster(List<double> Bounds, List<double> Rates) : IVisitorLedgerStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorLedger item, double valueValue)
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

public sealed record PercentVisitorLedgerAdjuster(double Percent) : IVisitorLedgerStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the visitor ledger steps in order and keeps a trace of each.</summary>
public sealed class VisitorLedgerEngine(List<IVisitorLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorLedgerEngine DefaultVisitorLedgerEngine()
    {
        return new VisitorLedgerEngine(
            new List<IVisitorLedgerStep>
            {
                new RoundingVisitorLedgerAdjuster(0.1),
                new VisitorLedgerMaximumOffsetAdjuster(1.19),
                new CapVisitorLedgerAdjuster(26141.0),
                new TitleVisitorLedgerAdjuster(new Dictionary<string, double> { ["central"] = 0.7 }),
                new TieredVisitorLedgerAdjuster(
                    new List<double> { 7943.0, 8469.0, 9993.0 },
                    new List<double> { 1.12, 1.12, 1.2 }),
                new PercentVisitorLedgerAdjuster(-21.9),
            },
            new List<string>(),
            4.0,
            14585.5);
    }

    public double Price(VisitorLedger item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
