namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay run estimate calculation.</summary>
public interface IPayRunEstimateFactor
{
    string Describe();
    double Adjust(PayRunEstimate item, double valueValue);
}

public sealed record FlaggedPayRunEstimateAdjuster(Registry Flags, string Flag, double Percent) : IPayRunEstimateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPayRunEstimateAdjuster(double Percent) : IPayRunEstimateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayRunEstimateAdjuster(double Cap) : IPayRunEstimateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamPayRunEstimateAdjuster(Dictionary<string, double> Factors) : IPayRunEstimateFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(PayRunEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PayRunEstimateCreatedAtOffsetAdjuster(double Weight) : IPayRunEstimateFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PayRunEstimate item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record MinimumPayRunEstimateAdjuster(double Minimum) : IPayRunEstimateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pay run estimate steps in order and keeps a trace of each.</summary>
public sealed class PayRunEstimateEngine(List<IPayRunEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunEstimateEngine DefaultPayRunEstimateEngine(Registry flags)
    {
        return new PayRunEstimateEngine(
            new List<IPayRunEstimateFactor>
            {
                new FlaggedPayRunEstimateAdjuster(flags, "pay-run-estimate-adaptive", -6.1),
                new PercentPayRunEstimateAdjuster(-4.0),
                new CapPayRunEstimateAdjuster(29339.0),
                new TeamPayRunEstimateAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.89, ["retail"] = 0.83 }),
                new PayRunEstimateCreatedAtOffsetAdjuster(1.23),
                new MinimumPayRunEstimateAdjuster(17.9),
            },
            new List<string>(),
            3.0,
            77829.9);
    }

    public double Evaluate(PayRunEstimate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
