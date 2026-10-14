namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient statement of work digest calculation.</summary>
public interface ILenientStatementOfWorkDigestAdjuster
{
    string Describe();
    double Adjust(LenientStatementOfWorkDigest item, double valueValue);
}

public sealed record FlaggedLenientStatementOfWorkDigestAdjuster(Registry Flags, string Flag, double Percent) : ILenientStatementOfWorkDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientStatementOfWorkDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleLenientStatementOfWorkDigestAdjuster(Dictionary<string, double> Factors) : ILenientStatementOfWorkDigestAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LenientStatementOfWorkDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLenientStatementOfWorkDigestAdjuster(double Cap) : ILenientStatementOfWorkDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientStatementOfWorkDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLenientStatementOfWorkDigestAdjuster(double Step) : ILenientStatementOfWorkDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientStatementOfWorkDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient statement of work digest steps in order and keeps a trace of each.</summary>
public sealed class LenientStatementOfWorkDigestEngine(List<ILenientStatementOfWorkDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientStatementOfWorkDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientStatementOfWorkDigestEngine DefaultLenientStatementOfWorkDigestEngine(Registry flags)
    {
        return new LenientStatementOfWorkDigestEngine(
            new List<ILenientStatementOfWorkDigestAdjuster>
            {
                new FlaggedLenientStatementOfWorkDigestAdjuster(
                    flags,
                    "lenient-statement-of-work-digest-strict",
                    8.5),
                new TitleLenientStatementOfWorkDigestAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.91, ["east"] = 1.28, ["online"] = 1.32 }),
                new CapLenientStatementOfWorkDigestAdjuster(2688.0),
                new RoundingLenientStatementOfWorkDigestAdjuster(1.0),
            },
            new List<string>(),
            3.9,
            77141.5);
    }

    public double Run(LenientStatementOfWorkDigest item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientStatementOfWorkDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
