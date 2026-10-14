namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global statement of work target calculation.</summary>
public interface IGlobalStatementOfWorkTargetAdjuster
{
    string Describe();
    double Adjust(GlobalStatementOfWorkTarget item, double valueValue);
}

public sealed record NameGlobalStatementOfWorkTargetAdjuster(Dictionary<string, double> Factors) : IGlobalStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(GlobalStatementOfWorkTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record GlobalStatementOfWorkTargetMaximumOffsetAdjuster(double Weight) : IGlobalStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(GlobalStatementOfWorkTarget item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentGlobalStatementOfWorkTargetAdjuster(double Percent) : IGlobalStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalStatementOfWorkTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalStatementOfWorkTargetAdjuster(double Minimum) : IGlobalStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalStatementOfWorkTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingGlobalStatementOfWorkTargetAdjuster(double Step) : IGlobalStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalStatementOfWorkTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedGlobalStatementOfWorkTargetAdjuster(Registry Flags, string Flag, double Percent) : IGlobalStatementOfWorkTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalStatementOfWorkTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global statement of work target steps in order and keeps a trace of each.</summary>
public sealed class GlobalStatementOfWorkTargetEngine(List<IGlobalStatementOfWorkTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalStatementOfWorkTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalStatementOfWorkTargetEngine DefaultGlobalStatementOfWorkTargetEngine(Registry flags)
    {
        return new GlobalStatementOfWorkTargetEngine(
            new List<IGlobalStatementOfWorkTargetAdjuster>
            {
                new NameGlobalStatementOfWorkTargetAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.17, ["west"] = 0.98, ["central"] = 0.73 }),
                new GlobalStatementOfWorkTargetMaximumOffsetAdjuster(1.24),
                new PercentGlobalStatementOfWorkTargetAdjuster(20.1),
                new MinimumGlobalStatementOfWorkTargetAdjuster(42.2),
                new RoundingGlobalStatementOfWorkTargetAdjuster(0.5),
                new FlaggedGlobalStatementOfWorkTargetAdjuster(flags, "global-statement-of-work-target-global", -6.7),
            },
            new List<string>(),
            4.8,
            89762.4);
    }

    public double Evaluate(GlobalStatementOfWorkTarget item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalStatementOfWorkTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
