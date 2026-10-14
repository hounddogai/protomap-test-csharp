namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual bundle calculation.</summary>
public interface IAccrualBundleStep
{
    string Describe();
    double Adjust(AccrualBundle item, double valueValue);
}

public sealed record CapAccrualBundleAdjuster(double Cap) : IAccrualBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAccrualBundleAdjuster(double Step) : IAccrualBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelAccrualBundleAdjuster(Dictionary<string, double> Factors) : IAccrualBundleStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(AccrualBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the accrual bundle steps in order and keeps a trace of each.</summary>
public sealed class AccrualBundleEngine(List<IAccrualBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualBundleEngine DefaultAccrualBundleEngine()
    {
        return new AccrualBundleEngine(
            new List<IAccrualBundleStep>
            {
                new CapAccrualBundleAdjuster(11780.0),
                new RoundingAccrualBundleAdjuster(0.1),
                new ChannelAccrualBundleAdjuster(new Dictionary<string, double> { ["west"] = 1.33 }),
            },
            new List<string>(),
            0.3,
            34910.7);
    }

    public double Price(AccrualBundle item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
