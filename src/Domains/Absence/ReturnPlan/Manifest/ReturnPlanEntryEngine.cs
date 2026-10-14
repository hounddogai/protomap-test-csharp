namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan entry calculation.</summary>
public interface IReturnPlanEntryFactor
{
    string Describe();
    double Adjust(ReturnPlanEntry item, double valueValue);
}

public sealed record MinimumReturnPlanEntryAdjuster(double Minimum) : IReturnPlanEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapReturnPlanEntryAdjuster(double Cap) : IReturnPlanEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ReturnPlanEntryLeaveDaysOffsetAdjuster(double Weight) : IReturnPlanEntryFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(ReturnPlanEntry item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record RoundingReturnPlanEntryAdjuster(double Step) : IReturnPlanEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the return plan entry steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanEntryEngine(List<IReturnPlanEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanEntryEngine DefaultReturnPlanEntryEngine()
    {
        return new ReturnPlanEntryEngine(
            new List<IReturnPlanEntryFactor>
            {
                new MinimumReturnPlanEntryAdjuster(2.1),
                new CapReturnPlanEntryAdjuster(2034.0),
                new ReturnPlanEntryLeaveDaysOffsetAdjuster(0.34),
                new RoundingReturnPlanEntryAdjuster(0.1),
            },
            new List<string>(),
            4.0,
            29473.7);
    }

    public double Evaluate(ReturnPlanEntry item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
