namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual span plan calculation.</summary>
public interface IManualSpanPlanStep
{
    string Describe();
    double Adjust(ManualSpanPlan item, double valueValue);
}

public sealed record TieredManualSpanPlanAdjuster(List<double> Bounds, List<double> Rates) : IManualSpanPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualSpanPlan item, double valueValue)
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

public sealed record PercentManualSpanPlanAdjuster(double Percent) : IManualSpanPlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualSpanPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualSpanPlanAdjuster(double Minimum) : IManualSpanPlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualSpanPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the manual span plan steps in order and keeps a trace of each.</summary>
public sealed class ManualSpanPlanEngine(List<IManualSpanPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualSpanPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualSpanPlanEngine DefaultManualSpanPlanEngine()
    {
        return new ManualSpanPlanEngine(
            new List<IManualSpanPlanStep>
            {
                new TieredManualSpanPlanAdjuster(
                    new List<double> { 1940.0, 4376.0, 9211.0, 9250.0 },
                    new List<double> { 0.94, 0.81, 1.03, 0.91 }),
                new PercentManualSpanPlanAdjuster(-0.1),
                new MinimumManualSpanPlanAdjuster(6.7),
            },
            new List<string>(),
            2.3,
            30503.4);
    }

    public double Compute(ManualSpanPlan item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ManualSpanPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
