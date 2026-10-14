namespace Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>A rule a default visitor rule must pass. Check returns its problems, or none.</summary>
public interface IDefaultVisitorRuleRule
{
    List<string> Check(DefaultVisitorRule item);
}

public sealed record DefaultVisitorRuleGraceDaysRangeRule(int Min, int Max) : IDefaultVisitorRuleRule
{
    public List<string> Check(DefaultVisitorRule item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorRuleCodeLengthRule(int MaxLength) : IDefaultVisitorRuleRule
{
    public List<string> Check(DefaultVisitorRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorRuleRequiredCodeRule(string Message) : IDefaultVisitorRuleRule
{
    public List<string> Check(DefaultVisitorRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorRuleRequiredEmployeeIdRule(string Message) : IDefaultVisitorRuleRule
{
    public List<string> Check(DefaultVisitorRule item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultVisitorRulePriorityRangeRule(int Min, int Max) : IDefaultVisitorRuleRule
{
    public List<string> Check(DefaultVisitorRule item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default visitor rule.</summary>
public sealed record DefaultVisitorRuleValidator(List<IDefaultVisitorRuleRule> Rules)
{
    public List<string> Validate(DefaultVisitorRule item)
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

    public void RequireValid(DefaultVisitorRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultVisitorRule> items)
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

/// <summary>Functions of default visitor rule rules.</summary>
public static class DefaultVisitorRuleRules
{
    public static DefaultVisitorRuleValidator DefaultDefaultVisitorRuleValidator()
    {
        return new DefaultVisitorRuleValidator(
            new List<IDefaultVisitorRuleRule>
            {
                new DefaultVisitorRuleGraceDaysRangeRule(64, 120),
                new DefaultVisitorRuleCodeLengthRule(86400),
                new DefaultVisitorRuleRequiredCodeRule("default visitor rule needs this field"),
                new DefaultVisitorRuleRequiredEmployeeIdRule("default visitor rule needs an entry"),
                new DefaultVisitorRulePriorityRangeRule(16, 3600),
            });
    }
}
