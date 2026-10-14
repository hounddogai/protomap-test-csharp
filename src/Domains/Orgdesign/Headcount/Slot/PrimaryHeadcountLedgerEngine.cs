namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary headcount ledger calculation.</summary>
public interface IPrimaryHeadcountLedgerStep
{
    string Describe();
    double Adjust(PrimaryHeadcountLedger item, double valueValue);
}

public sealed record RoundingPrimaryHeadcountLedgerAdjuster(double Step) : IPrimaryHeadcountLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryHeadcountLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitlePrimaryHeadcountLedgerAdjuster(Dictionary<string, double> Factors) : IPrimaryHeadcountLedgerStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryHeadcountLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPrimaryHeadcountLedgerAdjuster(double Cap) : IPrimaryHeadcountLedgerStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryHeadcountLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary headcount ledger steps in order and keeps a trace of each.</summary>
public sealed class PrimaryHeadcountLedgerEngine(List<IPrimaryHeadcountLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryHeadcountLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryHeadcountLedgerEngine DefaultPrimaryHeadcountLedgerEngine()
    {
        return new PrimaryHeadcountLedgerEngine(
            new List<IPrimaryHeadcountLedgerStep>
            {
                new RoundingPrimaryHeadcountLedgerAdjuster(0.05),
                new TitlePrimaryHeadcountLedgerAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.38, ["west"] = 0.76, ["retail"] = 0.86 }),
                new CapPrimaryHeadcountLedgerAdjuster(40164.0),
            },
            new List<string>(),
            3.8,
            30706.9);
    }

    public double Evaluate(PrimaryHeadcountLedger item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryHeadcountLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
