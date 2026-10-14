namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default reorg rule must pass. Check returns its problems, or none.</summary>
public interface IDefaultReorgRuleRule
{
    List<string> Check(DefaultReorgRule item);
}

public sealed record DefaultReorgRuleRequiredCodeRule(string Message) : IDefaultReorgRuleRule
{
    public List<string> Check(DefaultReorgRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgRuleAttemptsRangeRule(int Min, int Max) : IDefaultReorgRuleRule
{
    public List<string> Check(DefaultReorgRule item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultReorgRuleDepartmentLengthRule(int MaxLength) : IDefaultReorgRuleRule
{
    public List<string> Check(DefaultReorgRule item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgRuleMinimumRangeRule(double Min, double Max) : IDefaultReorgRuleRule
{
    public List<string> Check(DefaultReorgRule item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultReorgRuleCodeLengthRule(int MaxLength) : IDefaultReorgRuleRule
{
    public List<string> Check(DefaultReorgRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReorgRuleAllowedCodeRule(List<string> Allowed) : IDefaultReorgRuleRule
{
    public List<string> Check(DefaultReorgRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default reorg rule.</summary>
public sealed record DefaultReorgRuleValidator(List<IDefaultReorgRuleRule> Rules)
{
    public List<string> Validate(DefaultReorgRule item)
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

    public void RequireValid(DefaultReorgRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReorgRule> items)
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

/// <summary>Functions of default reorg rule rules.</summary>
public static class DefaultReorgRuleRules
{
    public static DefaultReorgRuleValidator DefaultDefaultReorgRuleValidator()
    {
        return new DefaultReorgRuleValidator(
            new List<IDefaultReorgRuleRule>
            {
                new DefaultReorgRuleRequiredCodeRule("default reorg rule needs this field"),
                new DefaultReorgRuleAttemptsRangeRule(255, 255),
                new DefaultReorgRuleDepartmentLengthRule(120),
                new DefaultReorgRuleMinimumRangeRule(6.2, 1615.3),
                new DefaultReorgRuleCodeLengthRule(255),
                new DefaultReorgRuleAllowedCodeRule(new List<string> { "manual", "tiered", "global" }),
            });
    }
}
