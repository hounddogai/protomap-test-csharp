namespace Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>A rule a default action plan preference must pass. Check returns its problems, or none.</summary>
public interface IDefaultActionPlanPreferenceRule
{
    List<string> Check(DefaultActionPlanPreference item);
}

public sealed record DefaultActionPlanPreferenceTenureMonthsRangeRule(int Min, int Max) : IDefaultActionPlanPreferenceRule
{
    public List<string> Check(DefaultActionPlanPreference item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultActionPlanPreferencePinnedGuardRule(int Limit) : IDefaultActionPlanPreferenceRule
{
    public List<string> Check(DefaultActionPlanPreference item)
    {
        if (item.Pinned && item.TenureMonths > Limit)
        {
            return new List<string> { $"tenure months {item.TenureMonths} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanPreferenceSourceLengthRule(int MaxLength) : IDefaultActionPlanPreferenceRule
{
    public List<string> Check(DefaultActionPlanPreference item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanPreferenceFteRangeRule(double Min, double Max) : IDefaultActionPlanPreferenceRule
{
    public List<string> Check(DefaultActionPlanPreference item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanPreferenceCapacityRangeRule(int Min, int Max) : IDefaultActionPlanPreferenceRule
{
    public List<string> Check(DefaultActionPlanPreference item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultActionPlanPreferenceRequiredNameRule(string Message) : IDefaultActionPlanPreferenceRule
{
    public List<string> Check(DefaultActionPlanPreference item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default action plan preference.</summary>
public sealed record DefaultActionPlanPreferenceValidator(List<IDefaultActionPlanPreferenceRule> Rules)
{
    public List<string> Validate(DefaultActionPlanPreference item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(DefaultActionPlanPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultActionPlanPreference> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of default action plan preference rules.</summary>
public static class DefaultActionPlanPreferenceRules
{
    public static DefaultActionPlanPreferenceValidator DefaultDefaultActionPlanPreferenceValidator()
    {
        return new DefaultActionPlanPreferenceValidator(
            new List<IDefaultActionPlanPreferenceRule>
            {
                new DefaultActionPlanPreferenceTenureMonthsRangeRule(32, 32),
                new DefaultActionPlanPreferencePinnedGuardRule(64),
                new DefaultActionPlanPreferenceSourceLengthRule(120),
                new DefaultActionPlanPreferenceFteRangeRule(9.0, 359.1),
                new DefaultActionPlanPreferenceCapacityRangeRule(32, 80),
                new DefaultActionPlanPreferenceRequiredNameRule("default action plan preference needs a value"),
            });
    }
}
