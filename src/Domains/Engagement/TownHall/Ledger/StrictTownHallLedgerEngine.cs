namespace Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>One step of the strict town hall ledger calculation.</summary>
public interface IStrictTownHallLedgerAdjuster
{
    string Describe();
    double Adjust(StrictTownHallLedger item, double valueValue);
}

public sealed record StrictTownHallLedgerLeaveDaysOffsetAdjuster(double Weight) : IStrictTownHallLedgerAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(StrictTownHallLedger item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record SourceStrictTownHallLedgerAdjuster(Dictionary<string, double> Factors) : IStrictTownHallLedgerAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(StrictTownHallLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictTownHallLedgerAdjuster(double Minimum) : IStrictTownHallLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictTownHallLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentStrictTownHallLedgerAdjuster(double Percent) : IStrictTownHallLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictTownHallLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict town hall ledger steps in order and keeps a trace of each.</summary>
public sealed class StrictTownHallLedgerEngine(List<IStrictTownHallLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictTownHallLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictTownHallLedgerEngine DefaultStrictTownHallLedgerEngine()
    {
        return new StrictTownHallLedgerEngine(
            new List<IStrictTownHallLedgerAdjuster>
            {
                new StrictTownHallLedgerLeaveDaysOffsetAdjuster(0.05),
                new SourceStrictTownHallLedgerAdjuster(new Dictionary<string, double> { ["north"] = 1.39 }),
                new MinimumStrictTownHallLedgerAdjuster(31.4),
                new PercentStrictTownHallLedgerAdjuster(-5.4),
            },
            new List<string>(),
            4.2,
            66506.5);
    }

    public double Compute(StrictTownHallLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictTownHallLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
