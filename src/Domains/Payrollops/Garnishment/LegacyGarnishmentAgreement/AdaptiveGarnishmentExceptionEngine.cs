namespace Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive garnishment exception calculation.</summary>
public interface IAdaptiveGarnishmentExceptionFactor
{
    string Describe();
    double Adjust(AdaptiveGarnishmentException item, double valueValue);
}

public sealed record CapAdaptiveGarnishmentExceptionAdjuster(double Cap) : IAdaptiveGarnishmentExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveGarnishmentException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdaptiveGarnishmentExceptionAdjuster(double Step) : IAdaptiveGarnishmentExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveGarnishmentException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAdaptiveGarnishmentExceptionAdjuster(double Minimum) : IAdaptiveGarnishmentExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveGarnishmentException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdaptiveGarnishmentExceptionEndsAtOffsetAdjuster(double Weight) : IAdaptiveGarnishmentExceptionFactor
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(AdaptiveGarnishmentException item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

/// <summary>Runs the adaptive garnishment exception steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveGarnishmentExceptionEngine(List<IAdaptiveGarnishmentExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveGarnishmentExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveGarnishmentExceptionEngine DefaultAdaptiveGarnishmentExceptionEngine()
    {
        return new AdaptiveGarnishmentExceptionEngine(
            new List<IAdaptiveGarnishmentExceptionFactor>
            {
                new CapAdaptiveGarnishmentExceptionAdjuster(17012.0),
                new RoundingAdaptiveGarnishmentExceptionAdjuster(0.25),
                new MinimumAdaptiveGarnishmentExceptionAdjuster(21.4),
                new AdaptiveGarnishmentExceptionEndsAtOffsetAdjuster(1.07),
            },
            new List<string>(),
            3.6,
            86509.1);
    }

    public double Price(AdaptiveGarnishmentException item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveGarnishmentException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
