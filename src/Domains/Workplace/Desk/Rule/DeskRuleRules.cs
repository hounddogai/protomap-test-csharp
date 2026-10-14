namespace Acme.Workforce.Workplace.Desk.Rule;

/// <summary>A rule a desk rule must pass. Check returns its problems, or none.</summary>
public interface IDeskRuleRule
{
    List<string> Check(DeskRule item);
}

public sealed record DeskRuleWeightRangeRule(double Min, double Max) : IDeskRuleRule
{
    public List<string> Check(DeskRule item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskRuleNameLengthRule(int MaxLength) : IDeskRuleRule
{
    public List<string> Check(DeskRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskRuleGraceDaysRangeRule(int Min, int Max) : IDeskRuleRule
{
    public List<string> Check(DeskRule item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskRuleCodeLengthRule(int MaxLength) : IDeskRuleRule
{
    public List<string> Check(DeskRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskRuleSkillLengthRule(int MaxLength) : IDeskRuleRule
{
    public List<string> Check(DeskRule item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk rule.</summary>
public sealed record DeskRuleValidator(List<IDeskRuleRule> Rules)
{
    public List<string> Validate(DeskRule item)
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

    public void RequireValid(DeskRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskRule> items)
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

/// <summary>Functions of desk rule rules.</summary>
public static class DeskRuleRules
{
    public static DeskRuleValidator DefaultDeskRuleValidator()
    {
        return new DeskRuleValidator(
            new List<IDeskRuleRule>
            {
                new DeskRuleWeightRangeRule(2.0, 1729.5),
                new DeskRuleNameLengthRule(120),
                new DeskRuleGraceDaysRangeRule(32, 80),
                new DeskRuleCodeLengthRule(64),
                new DeskRuleSkillLengthRule(32),
            });
    }
}
