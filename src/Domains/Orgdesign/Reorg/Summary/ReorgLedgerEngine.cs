namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the reorg ledger calculation.</summary>
public interface IReorgLedgerAdjuster
{
    string Describe();
    double Adjust(ReorgLedger item, double valueValue);
}

public sealed record MinimumReorgLedgerAdjuster(double Minimum) : IReorgLedgerAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameReorgLedgerAdjuster(Dictionary<string, double> Factors) : IReorgLedgerAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ReorgLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedReorgLedgerAdjuster(Registry Flags, string Flag, double Percent) : IReorgLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg ledger steps in order and keeps a trace of each.</summary>
public sealed class ReorgLedgerEngine(List<IReorgLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgLedgerEngine DefaultReorgLedgerEngine(Registry flags)
    {
        return new ReorgLedgerEngine(
            new List<IReorgLedgerAdjuster>
            {
                new MinimumReorgLedgerAdjuster(45.3),
                new NameReorgLedgerAdjuster(new Dictionary<string, double> { ["east"] = 0.92, ["north"] = 1.11 }),
                new FlaggedReorgLedgerAdjuster(flags, "reorg-ledger-primary", 2.8),
            },
            new List<string>(),
            1.8,
            30646.1);
    }

    public double Compute(ReorgLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<ReorgLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
