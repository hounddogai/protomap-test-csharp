namespace Acme.Workforce.Talent.Pool.Summary;

/// <summary>A rule a strict pool rule must pass. Check returns its problems, or none.</summary>
public interface IStrictPoolRuleRule
{
    List<string> Check(StrictPoolRule item);
}

public sealed record StrictPoolRuleManagerIdLengthRule(int MaxLength) : IStrictPoolRuleRule
{
    public List<string> Check(StrictPoolRule item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPoolRuleDurationMinutesRangeRule(int Min, int Max) : IStrictPoolRuleRule
{
    public List<string> Check(StrictPoolRule item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictPoolRuleMinimumRangeRule(double Min, double Max) : IStrictPoolRuleRule
{
    public List<string> Check(StrictPoolRule item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict pool rule.</summary>
public sealed record StrictPoolRuleValidator(List<IStrictPoolRuleRule> Rules)
{
    public List<string> Validate(StrictPoolRule item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(StrictPoolRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPoolRule> items)
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

/// <summary>Functions of strict pool rule rules.</summary>
public static class StrictPoolRuleRules
{
    public static StrictPoolRuleValidator DefaultStrictPoolRuleValidator()
    {
        return new StrictPoolRuleValidator(
            new List<IStrictPoolRuleRule>
            {
                new StrictPoolRuleManagerIdLengthRule(16),
                new StrictPoolRuleDurationMinutesRangeRule(1000, 3600),
                new StrictPoolRuleMinimumRangeRule(0.4, 4578.4),
            });
    }
}
