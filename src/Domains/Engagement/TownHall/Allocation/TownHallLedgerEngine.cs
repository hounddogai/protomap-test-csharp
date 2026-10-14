namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall ledger calculation.</summary>
public interface ITownHallLedgerModifier
{
    string Describe();
    double Adjust(TownHallLedger item, double valueValue);
}

public sealed record NameTownHallLedgerAdjuster(Dictionary<string, double> Factors) : ITownHallLedgerModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TownHallLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTownHallLedgerAdjuster(double Step) : ITownHallLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTownHallLedgerAdjuster(double Cap) : ITownHallLedgerModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the town hall ledger steps in order and keeps a trace of each.</summary>
public sealed class TownHallLedgerEngine(List<ITownHallLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallLedgerEngine DefaultTownHallLedgerEngine()
    {
        return new TownHallLedgerEngine(
            new List<ITownHallLedgerModifier>
            {
                new NameTownHallLedgerAdjuster(new Dictionary<string, double> { ["retail"] = 0.73 }),
                new RoundingTownHallLedgerAdjuster(0.1),
                new CapTownHallLedgerAdjuster(33426.0),
            },
            new List<string>(),
            0.9,
            73940.9);
    }

    public double Evaluate(TownHallLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<TownHallLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
