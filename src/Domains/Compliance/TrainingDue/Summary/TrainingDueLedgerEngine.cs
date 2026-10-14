namespace Acme.Workforce.Compliance.TrainingDue.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due ledger calculation.</summary>
public interface ITrainingDueLedgerFactor
{
    string Describe();
    double Adjust(TrainingDueLedger item, double valueValue);
}

public sealed record CapTrainingDueLedgerAdjuster(double Cap) : ITrainingDueLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionTrainingDueLedgerAdjuster(Dictionary<string, double> Factors) : ITrainingDueLedgerFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TrainingDueLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTrainingDueLedgerAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTrainingDueLedgerAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueLedger item, double valueValue)
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

public sealed record RoundingTrainingDueLedgerAdjuster(double Step) : ITrainingDueLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTrainingDueLedgerAdjuster(double Percent) : ITrainingDueLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the training due ledger steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueLedgerEngine(List<ITrainingDueLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueLedgerEngine DefaultTrainingDueLedgerEngine(Registry flags)
    {
        return new TrainingDueLedgerEngine(
            new List<ITrainingDueLedgerFactor>
            {
                new CapTrainingDueLedgerAdjuster(34875.0),
                new RegionTrainingDueLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.88, ["central"] = 0.87, ["east"] = 1.31 }),
                new FlaggedTrainingDueLedgerAdjuster(flags, "training-due-ledger-legacy", 7.0),
                new TieredTrainingDueLedgerAdjuster(
                    new List<double> { 308.0, 3745.0 },
                    new List<double> { 1.11, 1.08 }),
                new RoundingTrainingDueLedgerAdjuster(0.5),
                new PercentTrainingDueLedgerAdjuster(3.6),
            },
            new List<string>(),
            4.3,
            63780.4);
    }

    public double Price(TrainingDueLedger item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
