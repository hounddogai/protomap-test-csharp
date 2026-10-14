namespace Acme.Workforce.Talent.SkillProfile.Target;

/// <summary>One step of the skill profile plan calculation.</summary>
public interface ISkillProfilePlanStep
{
    string Describe();
    double Adjust(SkillProfilePlan item, double valueValue);
}

public sealed record PercentSkillProfilePlanAdjuster(double Percent) : ISkillProfilePlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfilePlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SkillProfilePlanFloorValueOffsetAdjuster(double Weight) : ISkillProfilePlanStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(SkillProfilePlan item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record CapSkillProfilePlanAdjuster(double Cap) : ISkillProfilePlanStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfilePlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSkillProfilePlanAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfilePlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfilePlan item, double valueValue)
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

public sealed record MinimumSkillProfilePlanAdjuster(double Minimum) : ISkillProfilePlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfilePlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the skill profile plan steps in order and keeps a trace of each.</summary>
public sealed class SkillProfilePlanEngine(List<ISkillProfilePlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfilePlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfilePlanEngine DefaultSkillProfilePlanEngine()
    {
        return new SkillProfilePlanEngine(
            new List<ISkillProfilePlanStep>
            {
                new PercentSkillProfilePlanAdjuster(15.3),
                new SkillProfilePlanFloorValueOffsetAdjuster(0.53),
                new CapSkillProfilePlanAdjuster(9950.0),
                new TieredSkillProfilePlanAdjuster(
                    new List<double> { 53.0, 864.0, 6110.0, 8433.0 },
                    new List<double> { 0.82, 1.06, 1.05, 0.9 }),
                new MinimumSkillProfilePlanAdjuster(40.2),
            },
            new List<string>(),
            4.8,
            98308.9);
    }

    public double Price(SkillProfilePlan item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfilePlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
