namespace Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>A rule a accrual rule must pass. Check returns its problems, or none.</summary>
public interface IAccrualRuleRule
{
    List<string> Check(AccrualRule item);
}

public sealed record AccrualRuleRequiredCodeRule(string Message) : IAccrualRuleRule
{
    public List<string> Check(AccrualRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualRuleRequiredNameRule(string Message) : IAccrualRuleRule
{
    public List<string> Check(AccrualRule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualRulePriorityRangeRule(int Min, int Max) : IAccrualRuleRule
{
    public List<string> Check(AccrualRule item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualRuleCodeLengthRule(int MaxLength) : IAccrualRuleRule
{
    public List<string> Check(AccrualRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualRuleNameLengthRule(int MaxLength) : IAccrualRuleRule
{
    public List<string> Check(AccrualRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualRuleRequiredSegmentRule(string Message) : IAccrualRuleRule
{
    public List<string> Check(AccrualRule item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual rule.</summary>
public sealed record AccrualRuleValidator(List<IAccrualRuleRule> Rules)
{
    public List<string> Validate(AccrualRule item)
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

    public void RequireValid(AccrualRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualRule> items)
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

/// <summary>Functions of accrual rule rules.</summary>
public static class AccrualRuleRules
{
    public static AccrualRuleValidator DefaultAccrualRuleValidator()
    {
        return new AccrualRuleValidator(
            new List<IAccrualRuleRule>
            {
                new AccrualRuleRequiredCodeRule("accrual rule needs an entry"),
                new AccrualRuleRequiredNameRule("accrual rule needs a value"),
                new AccrualRulePriorityRangeRule(32, 1000),
                new AccrualRuleCodeLengthRule(1000),
                new AccrualRuleNameLengthRule(255),
                new AccrualRuleRequiredSegmentRule("accrual rule needs an entry"),
            });
    }
}
