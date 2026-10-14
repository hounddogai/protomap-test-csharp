namespace Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>One step of the skill profile reservation calculation.</summary>
public interface ISkillProfileReservationModifier
{
    string Describe();
    double Adjust(SkillProfileReservation item, double valueValue);
}

public sealed record PercentSkillProfileReservationAdjuster(double Percent) : ISkillProfileReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSkillProfileReservationAdjuster(double Minimum) : ISkillProfileReservationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SkillProfileReservationRetriesOffsetAdjuster(double Weight) : ISkillProfileReservationModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(SkillProfileReservation item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

/// <summary>Runs the skill profile reservation steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileReservationEngine(List<ISkillProfileReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileReservationEngine DefaultSkillProfileReservationEngine()
    {
        return new SkillProfileReservationEngine(
            new List<ISkillProfileReservationModifier>
            {
                new PercentSkillProfileReservationAdjuster(5.2),
                new MinimumSkillProfileReservationAdjuster(15.6),
                new SkillProfileReservationRetriesOffsetAdjuster(0.13),
            },
            new List<string>(),
            3.3,
            67401.7);
    }

    public double Evaluate(SkillProfileReservation item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
