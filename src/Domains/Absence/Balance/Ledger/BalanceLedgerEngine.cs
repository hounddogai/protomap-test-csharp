namespace Acme.Workforce.Absence.Balance.Ledger;

/// <summary>One step of the balance ledger calculation.</summary>
public interface IBalanceLedgerAdjuster
{
    string Describe();
    double Adjust(BalanceLedger item, double valueValue);
}

public sealed record MinimumBalanceLedgerAdjuster(double Minimum) : IBalanceLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBalanceLedgerAdjuster(double Percent) : IBalanceLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentBalanceLedgerAdjuster(Dictionary<string, double> Factors) : IBalanceLedgerAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BalanceLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BalanceLedgerDurationMinutesOffsetAdjuster(double Weight) : IBalanceLedgerAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(BalanceLedger item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the balance ledger steps in order and keeps a trace of each.</summary>
public sealed class BalanceLedgerEngine(List<IBalanceLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceLedgerEngine DefaultBalanceLedgerEngine()
    {
        return new BalanceLedgerEngine(
            new List<IBalanceLedgerAdjuster>
            {
                new MinimumBalanceLedgerAdjuster(1.5),
                new PercentBalanceLedgerAdjuster(16.9),
                new SegmentBalanceLedgerAdjuster(new Dictionary<string, double> { ["south"] = 1.13 }),
                new BalanceLedgerDurationMinutesOffsetAdjuster(0.82),
            },
            new List<string>(),
            4.9,
            2568.1);
    }

    public double Price(BalanceLedger item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
