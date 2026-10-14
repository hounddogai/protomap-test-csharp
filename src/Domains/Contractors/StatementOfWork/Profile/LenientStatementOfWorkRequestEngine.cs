namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient statement of work request calculation.</summary>
public interface ILenientStatementOfWorkRequestStep
{
    string Describe();
    double Adjust(LenientStatementOfWorkRequest item, double valueValue);
}

public sealed record RoundingLenientStatementOfWorkRequestAdjuster(double Step) : ILenientStatementOfWorkRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientStatementOfWorkRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLenientStatementOfWorkRequestAdjuster(double Percent) : ILenientStatementOfWorkRequestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientStatementOfWorkRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientStatementOfWorkRequestAdjuster(List<double> Bounds, List<double> Rates) : ILenientStatementOfWorkRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientStatementOfWorkRequest item, double valueValue)
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

public sealed record FlaggedLenientStatementOfWorkRequestAdjuster(Registry Flags, string Flag, double Percent) : ILenientStatementOfWorkRequestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientStatementOfWorkRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientStatementOfWorkRequestAdjuster(double Cap) : ILenientStatementOfWorkRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientStatementOfWorkRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceLenientStatementOfWorkRequestAdjuster(Dictionary<string, double> Factors) : ILenientStatementOfWorkRequestStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LenientStatementOfWorkRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient statement of work request steps in order and keeps a trace of each.</summary>
public sealed class LenientStatementOfWorkRequestEngine(List<ILenientStatementOfWorkRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientStatementOfWorkRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientStatementOfWorkRequestEngine DefaultLenientStatementOfWorkRequestEngine(Registry flags)
    {
        return new LenientStatementOfWorkRequestEngine(
            new List<ILenientStatementOfWorkRequestStep>
            {
                new RoundingLenientStatementOfWorkRequestAdjuster(0.25),
                new PercentLenientStatementOfWorkRequestAdjuster(-8.4),
                new TieredLenientStatementOfWorkRequestAdjuster(
                    new List<double> { 1796.0, 5873.0 },
                    new List<double> { 0.96, 1.18 }),
                new FlaggedLenientStatementOfWorkRequestAdjuster(
                    flags,
                    "lenient-statement-of-work-request-fallback",
                    0.0),
                new CapLenientStatementOfWorkRequestAdjuster(32431.0),
                new SourceLenientStatementOfWorkRequestAdjuster(new Dictionary<string, double> { ["west"] = 1.23 }),
            },
            new List<string>(),
            3.9,
            47428.0);
    }

    public double Evaluate(LenientStatementOfWorkRequest item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientStatementOfWorkRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
