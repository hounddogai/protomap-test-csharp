namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run profile calculation.</summary>
public interface IPayRunProfileStep
{
    string Describe();
    double Adjust(PayRunProfile item, double valueValue);
}

public sealed record TieredPayRunProfileAdjuster(List<double> Bounds, List<double> Rates) : IPayRunProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunProfile item, double valueValue)
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

public sealed record RoundingPayRunProfileAdjuster(double Step) : IPayRunProfileStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayRunProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitlePayRunProfileAdjuster(Dictionary<string, double> Factors) : IPayRunProfileStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PayRunProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PayRunProfileDiscountOffsetAdjuster(double Weight) : IPayRunProfileStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(PayRunProfile item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

/// <summary>Runs the pay run profile steps in order and keeps a trace of each.</summary>
public sealed class PayRunProfileEngine(List<IPayRunProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunProfileEngine DefaultPayRunProfileEngine()
    {
        return new PayRunProfileEngine(
            new List<IPayRunProfileStep>
            {
                new TieredPayRunProfileAdjuster(new List<double> { 1962.0, 3491.0 }, new List<double> { 0.84, 1.15 }),
                new RoundingPayRunProfileAdjuster(1.0),
                new TitlePayRunProfileAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.12, ["central"] = 1.12 }),
                new PayRunProfileDiscountOffsetAdjuster(0.13),
            },
            new List<string>(),
            3.1,
            54499.4);
    }

    public double Compute(PayRunProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<PayRunProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
