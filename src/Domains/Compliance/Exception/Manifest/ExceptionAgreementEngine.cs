namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception agreement calculation.</summary>
public interface IExceptionAgreementModifier
{
    string Describe();
    double Adjust(ExceptionAgreement item, double valueValue);
}

public sealed record PercentExceptionAgreementAdjuster(double Percent) : IExceptionAgreementModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredExceptionAgreementAdjuster(List<double> Bounds, List<double> Rates) : IExceptionAgreementModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionAgreement item, double valueValue)
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

public sealed record MinimumExceptionAgreementAdjuster(double Minimum) : IExceptionAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedExceptionAgreementAdjuster(Registry Flags, string Flag, double Percent) : IExceptionAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingExceptionAgreementAdjuster(double Step) : IExceptionAgreementModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the exception agreement steps in order and keeps a trace of each.</summary>
public sealed class ExceptionAgreementEngine(List<IExceptionAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionAgreementEngine DefaultExceptionAgreementEngine(Registry flags)
    {
        return new ExceptionAgreementEngine(
            new List<IExceptionAgreementModifier>
            {
                new PercentExceptionAgreementAdjuster(2.1),
                new TieredExceptionAgreementAdjuster(
                    new List<double> { 423.0, 1136.0, 4072.0 },
                    new List<double> { 0.9, 1.01, 0.87 }),
                new MinimumExceptionAgreementAdjuster(40.8),
                new FlaggedExceptionAgreementAdjuster(flags, "exception-agreement-seasonal", -8.0),
                new RoundingExceptionAgreementAdjuster(0.25),
            },
            new List<string>(),
            4.1,
            50387.5);
    }

    public double Price(ExceptionAgreement item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
