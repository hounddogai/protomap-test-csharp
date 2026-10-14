namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>One step of the retro pay allocation calculation.</summary>
public interface IRetroPayAllocationFactor
{
    string Describe();
    double Adjust(RetroPayAllocation item, double valueValue);
}

public sealed record MinimumRetroPayAllocationAdjuster(double Minimum) : IRetroPayAllocationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryRetroPayAllocationAdjuster(Dictionary<string, double> Factors) : IRetroPayAllocationFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RetroPayAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RetroPayAllocationEndsAtOffsetAdjuster(double Weight) : IRetroPayAllocationFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(RetroPayAllocation item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the retro pay allocation steps in order and keeps a trace of each.</summary>
public sealed class RetroPayAllocationEngine(List<IRetroPayAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayAllocationEngine DefaultRetroPayAllocationEngine()
    {
        return new RetroPayAllocationEngine(
            new List<IRetroPayAllocationFactor>
            {
                new MinimumRetroPayAllocationAdjuster(37.9),
                new CategoryRetroPayAllocationAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.89, ["west"] = 0.77 }),
                new RetroPayAllocationEndsAtOffsetAdjuster(1.03),
            },
            new List<string>(),
            1.6,
            97895.4);
    }

    public double Evaluate(RetroPayAllocation item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
