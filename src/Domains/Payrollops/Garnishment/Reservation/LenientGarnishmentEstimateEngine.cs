namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient garnishment estimate calculation.</summary>
public interface ILenientGarnishmentEstimateAdjuster
{
    string Describe();
    double Adjust(LenientGarnishmentEstimate item, double valueValue);
}

public sealed record MinimumLenientGarnishmentEstimateAdjuster(double Minimum) : ILenientGarnishmentEstimateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientGarnishmentEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientGarnishmentEstimateAdjuster(Registry Flags, string Flag, double Percent) : ILenientGarnishmentEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientGarnishmentEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientGarnishmentEstimateAdjuster(double Step) : ILenientGarnishmentEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientGarnishmentEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient garnishment estimate steps in order and keeps a trace of each.</summary>
public sealed class LenientGarnishmentEstimateEngine(List<ILenientGarnishmentEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientGarnishmentEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientGarnishmentEstimateEngine DefaultLenientGarnishmentEstimateEngine(Registry flags)
    {
        return new LenientGarnishmentEstimateEngine(
            new List<ILenientGarnishmentEstimateAdjuster>
            {
                new MinimumLenientGarnishmentEstimateAdjuster(45.4),
                new FlaggedLenientGarnishmentEstimateAdjuster(flags, "lenient-garnishment-estimate-scheduled", 1.1),
                new RoundingLenientGarnishmentEstimateAdjuster(5.0),
            },
            new List<string>(),
            3.1,
            17608.8);
    }

    public double Compute(LenientGarnishmentEstimate item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientGarnishmentEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
