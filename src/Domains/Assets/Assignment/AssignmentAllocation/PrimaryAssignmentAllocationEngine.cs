namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary assignment allocation calculation.</summary>
public interface IPrimaryAssignmentAllocationAdjuster
{
    string Describe();
    double Adjust(PrimaryAssignmentAllocation item, double valueValue);
}

public sealed record PrimaryAssignmentAllocationTenureMonthsOffsetAdjuster(double Weight) : IPrimaryAssignmentAllocationAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PrimaryAssignmentAllocation item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record NamePrimaryAssignmentAllocationAdjuster(Dictionary<string, double> Factors) : IPrimaryAssignmentAllocationAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryAssignmentAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPrimaryAssignmentAllocationAdjuster(double Percent) : IPrimaryAssignmentAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAssignmentAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimaryAssignmentAllocationAdjuster(double Step) : IPrimaryAssignmentAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryAssignmentAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary assignment allocation steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAssignmentAllocationEngine(List<IPrimaryAssignmentAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAssignmentAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAssignmentAllocationEngine DefaultPrimaryAssignmentAllocationEngine()
    {
        return new PrimaryAssignmentAllocationEngine(
            new List<IPrimaryAssignmentAllocationAdjuster>
            {
                new PrimaryAssignmentAllocationTenureMonthsOffsetAdjuster(1.04),
                new NamePrimaryAssignmentAllocationAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.9, ["online"] = 1.01, ["retail"] = 0.79 }),
                new PercentPrimaryAssignmentAllocationAdjuster(20.8),
                new RoundingPrimaryAssignmentAllocationAdjuster(0.05),
            },
            new List<string>(),
            3.1,
            83874.0);
    }

    public double Compute(PrimaryAssignmentAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAssignmentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
