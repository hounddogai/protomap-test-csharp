namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment notice calculation.</summary>
public interface IGarnishmentNoticeFactor
{
    string Describe();
    double Adjust(GarnishmentNotice item, double valueValue);
}

public sealed record MinimumGarnishmentNoticeAdjuster(double Minimum) : IGarnishmentNoticeFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GarnishmentNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingGarnishmentNoticeAdjuster(double Step) : IGarnishmentNoticeFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapGarnishmentNoticeAdjuster(double Cap) : IGarnishmentNoticeFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the garnishment notice steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentNoticeEngine(List<IGarnishmentNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentNoticeEngine DefaultGarnishmentNoticeEngine()
    {
        return new GarnishmentNoticeEngine(
            new List<IGarnishmentNoticeFactor>
            {
                new MinimumGarnishmentNoticeAdjuster(0.8),
                new RoundingGarnishmentNoticeAdjuster(0.05),
                new CapGarnishmentNoticeAdjuster(3757.0),
            },
            new List<string>(),
            3.9,
            63063.7);
    }

    public double Compute(GarnishmentNotice item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
