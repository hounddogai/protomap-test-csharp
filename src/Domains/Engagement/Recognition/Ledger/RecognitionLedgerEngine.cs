namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the recognition ledger calculation.</summary>
public interface IRecognitionLedgerAdjuster
{
    string Describe();
    double Adjust(RecognitionLedger item, double valueValue);
}

public sealed record FlaggedRecognitionLedgerAdjuster(Registry Flags, string Flag, double Percent) : IRecognitionLedgerAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RecognitionLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RecognitionLedgerLeaveDaysOffsetAdjuster(double Weight) : IRecognitionLedgerAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(RecognitionLedger item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record UnitRecognitionLedgerAdjuster(Dictionary<string, double> Factors) : IRecognitionLedgerAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(RecognitionLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the recognition ledger steps in order and keeps a trace of each.</summary>
public sealed class RecognitionLedgerEngine(List<IRecognitionLedgerAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionLedgerAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionLedgerEngine DefaultRecognitionLedgerEngine(Registry flags)
    {
        return new RecognitionLedgerEngine(
            new List<IRecognitionLedgerAdjuster>
            {
                new FlaggedRecognitionLedgerAdjuster(flags, "recognition-ledger-weighted", 9.0),
                new RecognitionLedgerLeaveDaysOffsetAdjuster(1.36),
                new UnitRecognitionLedgerAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.14, ["north"] = 0.82, ["retail"] = 1.35 }),
            },
            new List<string>(),
            1.1,
            70455.2);
    }

    public double Evaluate(RecognitionLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
