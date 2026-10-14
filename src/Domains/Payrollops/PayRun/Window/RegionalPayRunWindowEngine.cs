namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional pay run window calculation.</summary>
public interface IRegionalPayRunWindowStep
{
    string Describe();
    double Adjust(RegionalPayRunWindow item, double valueValue);
}

public sealed record LocaleRegionalPayRunWindowAdjuster(Dictionary<string, double> Factors) : IRegionalPayRunWindowStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RegionalPayRunWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRegionalPayRunWindowAdjuster(double Cap) : IRegionalPayRunWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalPayRunWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRegionalPayRunWindowAdjuster(double Minimum) : IRegionalPayRunWindowStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalPayRunWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRegionalPayRunWindowAdjuster(double Percent) : IRegionalPayRunWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalPayRunWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalPayRunWindowAdjuster(double Step) : IRegionalPayRunWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPayRunWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional pay run window steps in order and keeps a trace of each.</summary>
public sealed class RegionalPayRunWindowEngine(List<IRegionalPayRunWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPayRunWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPayRunWindowEngine DefaultRegionalPayRunWindowEngine()
    {
        return new RegionalPayRunWindowEngine(
            new List<IRegionalPayRunWindowStep>
            {
                new LocaleRegionalPayRunWindowAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.14, ["west"] = 0.89 }),
                new CapRegionalPayRunWindowAdjuster(8137.0),
                new MinimumRegionalPayRunWindowAdjuster(11.4),
                new PercentRegionalPayRunWindowAdjuster(-5.8),
                new RoundingRegionalPayRunWindowAdjuster(0.25),
            },
            new List<string>(),
            3.7,
            84316.4);
    }

    public double Compute(RegionalPayRunWindow item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPayRunWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
