namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>One step of the default exception allocation calculation.</summary>
public interface IDefaultExceptionAllocationStep
{
    string Describe();
    double Adjust(DefaultExceptionAllocation item, double valueValue);
}

public sealed record NameDefaultExceptionAllocationAdjuster(Dictionary<string, double> Factors) : IDefaultExceptionAllocationStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(DefaultExceptionAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record DefaultExceptionAllocationVersionOffsetAdjuster(double Weight) : IDefaultExceptionAllocationStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(DefaultExceptionAllocation item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumDefaultExceptionAllocationAdjuster(double Minimum) : IDefaultExceptionAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultExceptionAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultExceptionAllocationAdjuster(double Percent) : IDefaultExceptionAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultExceptionAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultExceptionAllocationAdjuster(double Cap) : IDefaultExceptionAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultExceptionAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default exception allocation steps in order and keeps a trace of each.</summary>
public sealed class DefaultExceptionAllocationEngine(List<IDefaultExceptionAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultExceptionAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultExceptionAllocationEngine DefaultDefaultExceptionAllocationEngine()
    {
        return new DefaultExceptionAllocationEngine(
            new List<IDefaultExceptionAllocationStep>
            {
                new NameDefaultExceptionAllocationAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.76, ["south"] = 1.19 }),
                new DefaultExceptionAllocationVersionOffsetAdjuster(0.37),
                new MinimumDefaultExceptionAllocationAdjuster(12.5),
                new PercentDefaultExceptionAllocationAdjuster(5.0),
                new CapDefaultExceptionAllocationAdjuster(30855.0),
            },
            new List<string>(),
            1.8,
            10262.6);
    }

    public double Compute(DefaultExceptionAllocation item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultExceptionAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
