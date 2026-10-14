namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling skill profile reservation calculation.</summary>
public interface IRollingSkillProfileReservationModifier
{
    string Describe();
    double Adjust(RollingSkillProfileReservation item, double valueValue);
}

public sealed record CapRollingSkillProfileReservationAdjuster(double Cap) : IRollingSkillProfileReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingSkillProfileReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RollingSkillProfileReservationCostOffsetAdjuster(double Weight) : IRollingSkillProfileReservationModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(RollingSkillProfileReservation item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record RoundingRollingSkillProfileReservationAdjuster(double Step) : IRollingSkillProfileReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingSkillProfileReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling skill profile reservation steps in order and keeps a trace of each.</summary>
public sealed class RollingSkillProfileReservationEngine(List<IRollingSkillProfileReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingSkillProfileReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingSkillProfileReservationEngine DefaultRollingSkillProfileReservationEngine()
    {
        return new RollingSkillProfileReservationEngine(
            new List<IRollingSkillProfileReservationModifier>
            {
                new CapRollingSkillProfileReservationAdjuster(20992.0),
                new RollingSkillProfileReservationCostOffsetAdjuster(0.46),
                new RoundingRollingSkillProfileReservationAdjuster(5.0),
            },
            new List<string>(),
            2.2,
            12968.7);
    }

    public double Price(RollingSkillProfileReservation item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingSkillProfileReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
