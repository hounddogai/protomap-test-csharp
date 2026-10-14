namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run digest calculation.</summary>
public interface IPayRunDigestFactor
{
    string Describe();
    double Adjust(PayRunDigest item, double valueValue);
}

public sealed record RoundingPayRunDigestAdjuster(double Step) : IPayRunDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayRunDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPayRunDigestAdjuster(double Minimum) : IPayRunDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitlePayRunDigestAdjuster(Dictionary<string, double> Factors) : IPayRunDigestFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayRunDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPayRunDigestAdjuster(double Cap) : IPayRunDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pay run digest steps in order and keeps a trace of each.</summary>
public sealed class PayRunDigestEngine(List<IPayRunDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunDigestEngine DefaultPayRunDigestEngine()
    {
        return new PayRunDigestEngine(
            new List<IPayRunDigestFactor>
            {
                new RoundingPayRunDigestAdjuster(0.05),
                new MinimumPayRunDigestAdjuster(3.0),
                new TitlePayRunDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.85, ["south"] = 0.78, ["central"] = 1.04 }),
                new CapPayRunDigestAdjuster(20024.0),
            },
            new List<string>(),
            2.7,
            82595.5);
    }

    public double Run(PayRunDigest item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
