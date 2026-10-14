namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default bonus pool allocation calculation.</summary>
public interface IDefaultBonusPoolAllocationAdjuster
{
    string Describe();
    double Adjust(DefaultBonusPoolAllocation item, double valueValue);
}

public sealed record DefaultBonusPoolAllocationPriorityOffsetAdjuster(double Weight) : IDefaultBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(DefaultBonusPoolAllocation item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record TeamDefaultBonusPoolAllocationAdjuster(Dictionary<string, double> Factors) : IDefaultBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(DefaultBonusPoolAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultBonusPoolAllocationAdjuster(double Step) : IDefaultBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultBonusPoolAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultBonusPoolAllocationAdjuster(double Percent) : IDefaultBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultBonusPoolAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultBonusPoolAllocationAdjuster(double Cap) : IDefaultBonusPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultBonusPoolAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default bonus pool allocation steps in order and keeps a trace of each.</summary>
public sealed class DefaultBonusPoolAllocationEngine(List<IDefaultBonusPoolAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultBonusPoolAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultBonusPoolAllocationEngine DefaultDefaultBonusPoolAllocationEngine()
    {
        return new DefaultBonusPoolAllocationEngine(
            new List<IDefaultBonusPoolAllocationAdjuster>
            {
                new DefaultBonusPoolAllocationPriorityOffsetAdjuster(0.72),
                new TeamDefaultBonusPoolAllocationAdjuster(new Dictionary<string, double> { ["east"] = 1.31 }),
                new RoundingDefaultBonusPoolAllocationAdjuster(1.0),
                new PercentDefaultBonusPoolAllocationAdjuster(4.0),
                new CapDefaultBonusPoolAllocationAdjuster(6484.0),
            },
            new List<string>(),
            1.7,
            30436.6);
    }

    public double Price(DefaultBonusPoolAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultBonusPoolAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
