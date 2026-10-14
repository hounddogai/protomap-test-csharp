namespace Acme.Workforce.Talent.Pool.Plan;

/// <summary>A rule a tiered pool rule must pass. Check returns its problems, or none.</summary>
public interface ITieredPoolRuleRule
{
    List<string> Check(TieredPoolRule item);
}

public sealed record TieredPoolRuleLabelLengthRule(int MaxLength) : ITieredPoolRuleRule
{
    public List<string> Check(TieredPoolRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolRuleCeilingRangeRule(double Min, double Max) : ITieredPoolRuleRule
{
    public List<string> Check(TieredPoolRule item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredPoolRuleUpdatedAtRangeRule(int Min, int Max) : ITieredPoolRuleRule
{
    public List<string> Check(TieredPoolRule item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredPoolRuleRequiredDepartmentRule(string Message) : ITieredPoolRuleRule
{
    public List<string> Check(TieredPoolRule item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolRuleRequiredChannelRule(string Message) : ITieredPoolRuleRule
{
    public List<string> Check(TieredPoolRule item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolRuleCodeLengthRule(int MaxLength) : ITieredPoolRuleRule
{
    public List<string> Check(TieredPoolRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pool rule.</summary>
public sealed record TieredPoolRuleValidator(List<ITieredPoolRuleRule> Rules)
{
    public List<string> Validate(TieredPoolRule item)
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

    public void RequireValid(TieredPoolRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPoolRule> items)
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

/// <summary>Functions of tiered pool rule rules.</summary>
public static class TieredPoolRuleRules
{
    public static TieredPoolRuleValidator DefaultTieredPoolRuleValidator()
    {
        return new TieredPoolRuleValidator(
            new List<ITieredPoolRuleRule>
            {
                new TieredPoolRuleLabelLengthRule(16),
                new TieredPoolRuleCeilingRangeRule(9.4, 4826.4),
                new TieredPoolRuleUpdatedAtRangeRule(32, 80),
                new TieredPoolRuleRequiredDepartmentRule("tiered pool rule needs a value"),
                new TieredPoolRuleRequiredChannelRule("tiered pool rule needs this field"),
                new TieredPoolRuleCodeLengthRule(32),
            });
    }
}
