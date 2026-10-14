namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span agreement calculation.</summary>
public interface ISpanAgreementStep
{
    string Describe();
    double Adjust(SpanAgreement item, double valueValue);
}

public sealed record DepartmentSpanAgreementAdjuster(Dictionary<string, double> Factors) : ISpanAgreementStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SpanAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSpanAgreementAdjuster(double Percent) : ISpanAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSpanAgreementAdjuster(double Minimum) : ISpanAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SpanAgreementEndsAtOffsetAdjuster(double Weight) : ISpanAgreementStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(SpanAgreement item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the span agreement steps in order and keeps a trace of each.</summary>
public sealed class SpanAgreementEngine(List<ISpanAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanAgreementEngine DefaultSpanAgreementEngine()
    {
        return new SpanAgreementEngine(
            new List<ISpanAgreementStep>
            {
                new DepartmentSpanAgreementAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.18, ["online"] = 1.28, ["north"] = 0.84 }),
                new PercentSpanAgreementAdjuster(-1.0),
                new MinimumSpanAgreementAdjuster(48.6),
                new SpanAgreementEndsAtOffsetAdjuster(0.54),
            },
            new List<string>(),
            0.6,
            82991.6);
    }

    public double Price(SpanAgreement item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SpanAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
