namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional attestation schedule calculation.</summary>
public interface IRegionalAttestationScheduleModifier
{
    string Describe();
    double Adjust(RegionalAttestationSchedule item, double valueValue);
}

public sealed record RegionalAttestationScheduleMaximumOffsetAdjuster(double Weight) : IRegionalAttestationScheduleModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(RegionalAttestationSchedule item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentRegionalAttestationScheduleAdjuster(double Percent) : IRegionalAttestationScheduleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalAttestationSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentRegionalAttestationScheduleAdjuster(Dictionary<string, double> Factors) : IRegionalAttestationScheduleModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(RegionalAttestationSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRegionalAttestationScheduleAdjuster(double Step) : IRegionalAttestationScheduleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalAttestationSchedule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional attestation schedule steps in order and keeps a trace of each.</summary>
public sealed class RegionalAttestationScheduleEngine(List<IRegionalAttestationScheduleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAttestationScheduleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAttestationScheduleEngine DefaultRegionalAttestationScheduleEngine()
    {
        return new RegionalAttestationScheduleEngine(
            new List<IRegionalAttestationScheduleModifier>
            {
                new RegionalAttestationScheduleMaximumOffsetAdjuster(1.81),
                new PercentRegionalAttestationScheduleAdjuster(-5.8),
                new DepartmentRegionalAttestationScheduleAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.92, ["retail"] = 1.19 }),
                new RoundingRegionalAttestationScheduleAdjuster(0.1),
            },
            new List<string>(),
            2.4,
            92224.7);
    }

    public double Compute(RegionalAttestationSchedule item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalAttestationSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
