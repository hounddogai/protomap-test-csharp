namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay claim calculation.</summary>
public interface IRetroPayClaimModifier
{
    string Describe();
    double Adjust(RetroPayClaim item, double valueValue);
}

public sealed record RoundingRetroPayClaimAdjuster(double Step) : IRetroPayClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RetroPayClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRetroPayClaimAdjuster(double Minimum) : IRetroPayClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RetroPayClaimRateOffsetAdjuster(double Weight) : IRetroPayClaimModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RetroPayClaim item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record SourceRetroPayClaimAdjuster(Dictionary<string, double> Factors) : IRetroPayClaimModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(RetroPayClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the retro pay claim steps in order and keeps a trace of each.</summary>
public sealed class RetroPayClaimEngine(List<IRetroPayClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayClaimEngine DefaultRetroPayClaimEngine()
    {
        return new RetroPayClaimEngine(
            new List<IRetroPayClaimModifier>
            {
                new RoundingRetroPayClaimAdjuster(0.25),
                new MinimumRetroPayClaimAdjuster(0.8),
                new RetroPayClaimRateOffsetAdjuster(1.73),
                new SourceRetroPayClaimAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.39, ["west"] = 0.84 }),
            },
            new List<string>(),
            4.6,
            39986.2);
    }

    public double Price(RetroPayClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
