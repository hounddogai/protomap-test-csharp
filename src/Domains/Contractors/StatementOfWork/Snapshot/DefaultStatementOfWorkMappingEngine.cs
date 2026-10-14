namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default statement of work mapping calculation.</summary>
public interface IDefaultStatementOfWorkMappingFactor
{
    string Describe();
    double Adjust(DefaultStatementOfWorkMapping item, double valueValue);
}

public sealed record CapDefaultStatementOfWorkMappingAdjuster(double Cap) : IDefaultStatementOfWorkMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultStatementOfWorkMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDefaultStatementOfWorkMappingAdjuster(List<double> Bounds, List<double> Rates) : IDefaultStatementOfWorkMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultStatementOfWorkMapping item, double valueValue)
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

public sealed record FlaggedDefaultStatementOfWorkMappingAdjuster(Registry Flags, string Flag, double Percent) : IDefaultStatementOfWorkMappingFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultStatementOfWorkMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultStatementOfWorkMappingAdjuster(double Minimum) : IDefaultStatementOfWorkMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultStatementOfWorkMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default statement of work mapping steps in order and keeps a trace of each.</summary>
public sealed class DefaultStatementOfWorkMappingEngine(List<IDefaultStatementOfWorkMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultStatementOfWorkMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultStatementOfWorkMappingEngine DefaultDefaultStatementOfWorkMappingEngine(Registry flags)
    {
        return new DefaultStatementOfWorkMappingEngine(
            new List<IDefaultStatementOfWorkMappingFactor>
            {
                new CapDefaultStatementOfWorkMappingAdjuster(17263.0),
                new TieredDefaultStatementOfWorkMappingAdjuster(
                    new List<double> { 670.0, 8439.0, 9883.0 },
                    new List<double> { 1.06, 0.82, 0.87 }),
                new FlaggedDefaultStatementOfWorkMappingAdjuster(
                    flags,
                    "default-statement-of-work-mapping-tiered",
                    9.8),
                new MinimumDefaultStatementOfWorkMappingAdjuster(29.1),
            },
            new List<string>(),
            1.2,
            92417.1);
    }

    public double Price(DefaultStatementOfWorkMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultStatementOfWorkMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
