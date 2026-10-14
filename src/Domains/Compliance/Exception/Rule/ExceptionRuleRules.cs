namespace Acme.Workforce.Compliance.Exception.Rule;

/// <summary>A rule a exception rule must pass. Check returns its problems, or none.</summary>
public interface IExceptionRuleRule
{
    List<string> Check(ExceptionRule item);
}

public sealed record ExceptionRuleFteRangeRule(double Min, double Max) : IExceptionRuleRule
{
    public List<string> Check(ExceptionRule item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRuleRequiredTitleRule(string Message) : IExceptionRuleRule
{
    public List<string> Check(ExceptionRule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRuleRequiredRegionRule(string Message) : IExceptionRuleRule
{
    public List<string> Check(ExceptionRule item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRuleRequiredReviewerRule(string Message) : IExceptionRuleRule
{
    public List<string> Check(ExceptionRule item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRuleCodeLengthRule(int MaxLength) : IExceptionRuleRule
{
    public List<string> Check(ExceptionRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionRuleRequiredCodeRule(string Message) : IExceptionRuleRule
{
    public List<string> Check(ExceptionRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception rule.</summary>
public sealed record ExceptionRuleValidator(List<IExceptionRuleRule> Rules)
{
    public List<string> Validate(ExceptionRule item)
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

    public void RequireValid(ExceptionRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionRule> items)
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

/// <summary>Functions of exception rule rules.</summary>
public static class ExceptionRuleRules
{
    public static ExceptionRuleValidator DefaultExceptionRuleValidator()
    {
        return new ExceptionRuleValidator(
            new List<IExceptionRuleRule>
            {
                new ExceptionRuleFteRangeRule(8.2, 837.4),
                new ExceptionRuleRequiredTitleRule("exception rule needs this field"),
                new ExceptionRuleRequiredRegionRule("exception rule needs a value"),
                new ExceptionRuleRequiredReviewerRule("exception rule needs a value"),
                new ExceptionRuleCodeLengthRule(80),
                new ExceptionRuleRequiredCodeRule("exception rule needs a value"),
            });
    }
}
