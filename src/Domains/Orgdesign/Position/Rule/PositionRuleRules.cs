namespace Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>A rule a position rule must pass. Check returns its problems, or none.</summary>
public interface IPositionRuleRule
{
    List<string> Check(PositionRule item);
}

public sealed record PositionRuleRequiredSkillRule(string Message) : IPositionRuleRule
{
    public List<string> Check(PositionRule item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionRuleManagerIdLengthRule(int MaxLength) : IPositionRuleRule
{
    public List<string> Check(PositionRule item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionRuleSkillLengthRule(int MaxLength) : IPositionRuleRule
{
    public List<string> Check(PositionRule item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionRuleAmountRangeRule(double Min, double Max) : IPositionRuleRule
{
    public List<string> Check(PositionRule item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position rule.</summary>
public sealed record PositionRuleValidator(List<IPositionRuleRule> Rules)
{
    public List<string> Validate(PositionRule item)
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

    public void RequireValid(PositionRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionRule> items)
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

/// <summary>Functions of position rule rules.</summary>
public static class PositionRuleRules
{
    public static PositionRuleValidator DefaultPositionRuleValidator()
    {
        return new PositionRuleValidator(
            new List<IPositionRuleRule>
            {
                new PositionRuleRequiredSkillRule("position rule needs a value"),
                new PositionRuleManagerIdLengthRule(86400),
                new PositionRuleSkillLengthRule(86400),
                new PositionRuleAmountRangeRule(9.3, 76.0),
            });
    }
}
