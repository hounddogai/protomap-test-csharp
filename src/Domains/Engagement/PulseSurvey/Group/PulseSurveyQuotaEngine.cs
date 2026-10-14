namespace Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>One step of the pulse survey quota calculation.</summary>
public interface IPulseSurveyQuotaModifier
{
    string Describe();
    double Adjust(PulseSurveyQuota item, double valueValue);
}

public sealed record CapPulseSurveyQuotaAdjuster(double Cap) : IPulseSurveyQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PulseSurveyQuotaAmountOffsetAdjuster(double Weight) : IPulseSurveyQuotaModifier
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(PulseSurveyQuota item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record MinimumPulseSurveyQuotaAdjuster(double Minimum) : IPulseSurveyQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveyQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pulse survey quota steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyQuotaEngine(List<IPulseSurveyQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyQuotaEngine DefaultPulseSurveyQuotaEngine()
    {
        return new PulseSurveyQuotaEngine(
            new List<IPulseSurveyQuotaModifier>
            {
                new CapPulseSurveyQuotaAdjuster(4924.0),
                new PulseSurveyQuotaAmountOffsetAdjuster(1.19),
                new MinimumPulseSurveyQuotaAdjuster(25.1),
            },
            new List<string>(),
            4.0,
            55034.7);
    }

    public double Price(PulseSurveyQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
