namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>One step of the regional statement of work digest calculation.</summary>
public interface IRegionalStatementOfWorkDigestStep
{
    string Describe();
    double Adjust(RegionalStatementOfWorkDigest item, double valueValue);
}

public sealed record PercentRegionalStatementOfWorkDigestAdjuster(double Percent) : IRegionalStatementOfWorkDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalStatementOfWorkDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalStatementOfWorkDigestAdjuster(double Cap) : IRegionalStatementOfWorkDigestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalStatementOfWorkDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalStatementOfWorkDigestGraceDaysOffsetAdjuster(double Weight) : IRegionalStatementOfWorkDigestStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(RegionalStatementOfWorkDigest item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the regional statement of work digest steps in order and keeps a trace of each.</summary>
public sealed class RegionalStatementOfWorkDigestEngine(List<IRegionalStatementOfWorkDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalStatementOfWorkDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalStatementOfWorkDigestEngine DefaultRegionalStatementOfWorkDigestEngine()
    {
        return new RegionalStatementOfWorkDigestEngine(
            new List<IRegionalStatementOfWorkDigestStep>
            {
                new PercentRegionalStatementOfWorkDigestAdjuster(18.5),
                new CapRegionalStatementOfWorkDigestAdjuster(27473.0),
                new RegionalStatementOfWorkDigestGraceDaysOffsetAdjuster(0.48),
            },
            new List<string>(),
            1.4,
            23580.5);
    }

    public double Compute(RegionalStatementOfWorkDigest item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalStatementOfWorkDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
