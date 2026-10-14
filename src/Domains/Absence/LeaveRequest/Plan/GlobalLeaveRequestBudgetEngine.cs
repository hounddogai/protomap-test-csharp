namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global leave request budget calculation.</summary>
public interface IGlobalLeaveRequestBudgetAdjuster
{
    string Describe();
    double Adjust(GlobalLeaveRequestBudget item, double valueValue);
}

public sealed record GlobalLeaveRequestBudgetMaximumOffsetAdjuster(double Weight) : IGlobalLeaveRequestBudgetAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(GlobalLeaveRequestBudget item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record FlaggedGlobalLeaveRequestBudgetAdjuster(Registry Flags, string Flag, double Percent) : IGlobalLeaveRequestBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalLeaveRequestBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalLeaveRequestBudgetAdjuster(double Step) : IGlobalLeaveRequestBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalLeaveRequestBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global leave request budget steps in order and keeps a trace of each.</summary>
public sealed class GlobalLeaveRequestBudgetEngine(List<IGlobalLeaveRequestBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalLeaveRequestBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalLeaveRequestBudgetEngine DefaultGlobalLeaveRequestBudgetEngine(Registry flags)
    {
        return new GlobalLeaveRequestBudgetEngine(
            new List<IGlobalLeaveRequestBudgetAdjuster>
            {
                new GlobalLeaveRequestBudgetMaximumOffsetAdjuster(0.09),
                new FlaggedGlobalLeaveRequestBudgetAdjuster(flags, "global-leave-request-budget-tiered", 1.3),
                new RoundingGlobalLeaveRequestBudgetAdjuster(0.5),
            },
            new List<string>(),
            3.3,
            78762.8);
    }

    public double Compute(GlobalLeaveRequestBudget item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalLeaveRequestBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
