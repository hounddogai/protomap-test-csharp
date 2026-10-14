namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile budget calculation.</summary>
public interface ISkillProfileBudgetAdjuster
{
    string Describe();
    double Adjust(SkillProfileBudget item, double valueValue);
}

public sealed record RoundingSkillProfileBudgetAdjuster(double Step) : ISkillProfileBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SkillProfileBudgetGraceDaysOffsetAdjuster(double Weight) : ISkillProfileBudgetAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(SkillProfileBudget item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record CapSkillProfileBudgetAdjuster(double Cap) : ISkillProfileBudgetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSkillProfileBudgetAdjuster(double Minimum) : ISkillProfileBudgetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSkillProfileBudgetAdjuster(double Percent) : ISkillProfileBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the skill profile budget steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileBudgetEngine(List<ISkillProfileBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileBudgetEngine DefaultSkillProfileBudgetEngine()
    {
        return new SkillProfileBudgetEngine(
            new List<ISkillProfileBudgetAdjuster>
            {
                new RoundingSkillProfileBudgetAdjuster(1.0),
                new SkillProfileBudgetGraceDaysOffsetAdjuster(1.68),
                new CapSkillProfileBudgetAdjuster(37106.0),
                new MinimumSkillProfileBudgetAdjuster(12.9),
                new PercentSkillProfileBudgetAdjuster(11.6),
            },
            new List<string>(),
            4.1,
            31983.3);
    }

    public double Run(SkillProfileBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
