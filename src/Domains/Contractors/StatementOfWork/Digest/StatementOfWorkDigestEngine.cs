namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the statement of work digest calculation.</summary>
public interface IStatementOfWorkDigestFactor
{
    string Describe();
    double Adjust(StatementOfWorkDigest item, double valueValue);
}

public sealed record MinimumStatementOfWorkDigestAdjuster(double Minimum) : IStatementOfWorkDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StatementOfWorkDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStatementOfWorkDigestAdjuster(double Cap) : IStatementOfWorkDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StatementOfWorkDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStatementOfWorkDigestAdjuster(double Step) : IStatementOfWorkDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StatementOfWorkDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record StatementOfWorkDigestPriorityOffsetAdjuster(double Weight) : IStatementOfWorkDigestFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(StatementOfWorkDigest item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record RegionStatementOfWorkDigestAdjuster(Dictionary<string, double> Factors) : IStatementOfWorkDigestFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(StatementOfWorkDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the statement of work digest steps in order and keeps a trace of each.</summary>
public sealed class StatementOfWorkDigestEngine(List<IStatementOfWorkDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStatementOfWorkDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StatementOfWorkDigestEngine DefaultStatementOfWorkDigestEngine()
    {
        return new StatementOfWorkDigestEngine(
            new List<IStatementOfWorkDigestFactor>
            {
                new MinimumStatementOfWorkDigestAdjuster(24.2),
                new CapStatementOfWorkDigestAdjuster(47957.0),
                new RoundingStatementOfWorkDigestAdjuster(0.1),
                new StatementOfWorkDigestPriorityOffsetAdjuster(0.06),
                new RegionStatementOfWorkDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.84, ["south"] = 0.92 }),
            },
            new List<string>(),
            4.5,
            71439.4);
    }

    public double Compute(StatementOfWorkDigest item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<StatementOfWorkDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
