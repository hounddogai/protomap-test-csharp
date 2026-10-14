namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan policy calculation.</summary>
public interface IReturnPlanPolicyStep
{
    string Describe();
    double Adjust(ReturnPlanPolicy item, double valueValue);
}

public sealed record PercentReturnPlanPolicyAdjuster(double Percent) : IReturnPlanPolicyStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumReturnPlanPolicyAdjuster(double Minimum) : IReturnPlanPolicyStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedReturnPlanPolicyAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanPolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingReturnPlanPolicyAdjuster(double Step) : IReturnPlanPolicyStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapReturnPlanPolicyAdjuster(double Cap) : IReturnPlanPolicyStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredReturnPlanPolicyAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanPolicyStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanPolicy item, double valueValue)
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

/// <summary>Runs the return plan policy steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanPolicyEngine(List<IReturnPlanPolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanPolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanPolicyEngine DefaultReturnPlanPolicyEngine(Registry flags)
    {
        return new ReturnPlanPolicyEngine(
            new List<IReturnPlanPolicyStep>
            {
                new PercentReturnPlanPolicyAdjuster(11.5),
                new MinimumReturnPlanPolicyAdjuster(27.3),
                new FlaggedReturnPlanPolicyAdjuster(flags, "return-plan-policy-pending", -7.2),
                new RoundingReturnPlanPolicyAdjuster(0.25),
                new CapReturnPlanPolicyAdjuster(10651.0),
                new TieredReturnPlanPolicyAdjuster(
                    new List<double> { 209.0, 6970.0 },
                    new List<double> { 0.92, 0.94 }),
            },
            new List<string>(),
            0.2,
            75698.2);
    }

    public double Price(ReturnPlanPolicy item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
