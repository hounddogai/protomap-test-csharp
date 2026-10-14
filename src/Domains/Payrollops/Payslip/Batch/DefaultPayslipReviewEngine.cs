namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default payslip review calculation.</summary>
public interface IDefaultPayslipReviewAdjuster
{
    string Describe();
    double Adjust(DefaultPayslipReview item, double valueValue);
}

public sealed record MinimumDefaultPayslipReviewAdjuster(double Minimum) : IDefaultPayslipReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPayslipReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingDefaultPayslipReviewAdjuster(double Step) : IDefaultPayslipReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPayslipReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleDefaultPayslipReviewAdjuster(Dictionary<string, double> Factors) : IDefaultPayslipReviewAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultPayslipReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default payslip review steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayslipReviewEngine(List<IDefaultPayslipReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayslipReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayslipReviewEngine DefaultDefaultPayslipReviewEngine()
    {
        return new DefaultPayslipReviewEngine(
            new List<IDefaultPayslipReviewAdjuster>
            {
                new MinimumDefaultPayslipReviewAdjuster(8.3),
                new RoundingDefaultPayslipReviewAdjuster(0.5),
                new TitleDefaultPayslipReviewAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.72, ["south"] = 1.15, ["retail"] = 1.11 }),
            },
            new List<string>(),
            0.4,
            11791.5);
    }

    public double Evaluate(DefaultPayslipReview item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayslipReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
