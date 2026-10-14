namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan allocation calculation.</summary>
public interface IReturnPlanAllocationStep
{
    string Describe();
    double Adjust(ReturnPlanAllocation item, double valueValue);
}

public sealed record CapReturnPlanAllocationAdjuster(double Cap) : IReturnPlanAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ReturnPlanAllocationMinimumOffsetAdjuster(double Weight) : IReturnPlanAllocationStep
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(ReturnPlanAllocation item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record FlaggedReturnPlanAllocationAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReturnPlanAllocationAdjuster(double Step) : IReturnPlanAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleReturnPlanAllocationAdjuster(Dictionary<string, double> Factors) : IReturnPlanAllocationStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ReturnPlanAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumReturnPlanAllocationAdjuster(double Minimum) : IReturnPlanAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the return plan allocation steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanAllocationEngine(List<IReturnPlanAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanAllocationEngine DefaultReturnPlanAllocationEngine(Registry flags)
    {
        return new ReturnPlanAllocationEngine(
            new List<IReturnPlanAllocationStep>
            {
                new CapReturnPlanAllocationAdjuster(40425.0),
                new ReturnPlanAllocationMinimumOffsetAdjuster(0.95),
                new FlaggedReturnPlanAllocationAdjuster(flags, "return-plan-allocation-adaptive", 8.7),
                new RoundingReturnPlanAllocationAdjuster(0.5),
                new LocaleReturnPlanAllocationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.17, ["west"] = 1.04, ["north"] = 0.87 }),
                new MinimumReturnPlanAllocationAdjuster(46.2),
            },
            new List<string>(),
            1.1,
            77412.4);
    }

    public double Compute(ReturnPlanAllocation item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
