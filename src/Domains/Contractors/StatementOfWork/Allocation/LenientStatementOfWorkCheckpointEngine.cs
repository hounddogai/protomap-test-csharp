namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient statement of work checkpoint calculation.</summary>
public interface ILenientStatementOfWorkCheckpointFactor
{
    string Describe();
    double Adjust(LenientStatementOfWorkCheckpoint item, double valueValue);
}

public sealed record TieredLenientStatementOfWorkCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ILenientStatementOfWorkCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientStatementOfWorkCheckpoint item, double valueValue)
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

public sealed record RoundingLenientStatementOfWorkCheckpointAdjuster(double Step) : ILenientStatementOfWorkCheckpointFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientStatementOfWorkCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLenientStatementOfWorkCheckpointAdjuster(double Minimum) : ILenientStatementOfWorkCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientStatementOfWorkCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLenientStatementOfWorkCheckpointAdjuster(double Percent) : ILenientStatementOfWorkCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientStatementOfWorkCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLenientStatementOfWorkCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ILenientStatementOfWorkCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientStatementOfWorkCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient statement of work checkpoint steps in order and keeps a trace of each.</summary>
public sealed class LenientStatementOfWorkCheckpointEngine(List<ILenientStatementOfWorkCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientStatementOfWorkCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientStatementOfWorkCheckpointEngine DefaultLenientStatementOfWorkCheckpointEngine(Registry flags)
    {
        return new LenientStatementOfWorkCheckpointEngine(
            new List<ILenientStatementOfWorkCheckpointFactor>
            {
                new TieredLenientStatementOfWorkCheckpointAdjuster(
                    new List<double> { 1396.0, 2099.0 },
                    new List<double> { 1.03, 0.91 }),
                new RoundingLenientStatementOfWorkCheckpointAdjuster(0.1),
                new MinimumLenientStatementOfWorkCheckpointAdjuster(35.7),
                new PercentLenientStatementOfWorkCheckpointAdjuster(-1.9),
                new FlaggedLenientStatementOfWorkCheckpointAdjuster(
                    flags,
                    "lenient-statement-of-work-checkpoint-rolling",
                    9.3),
            },
            new List<string>(),
            4.1,
            59676.6);
    }

    public double Evaluate(LenientStatementOfWorkCheckpoint item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientStatementOfWorkCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
