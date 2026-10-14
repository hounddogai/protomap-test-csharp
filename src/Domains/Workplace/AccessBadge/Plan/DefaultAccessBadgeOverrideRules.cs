namespace Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>A rule a default access badge override must pass. Check returns its problems, or none.</summary>
public interface IDefaultAccessBadgeOverrideRule
{
    List<string> Check(DefaultAccessBadgeOverride item);
}

public sealed record DefaultAccessBadgeOverrideDiscountRangeRule(double Min, double Max) : IDefaultAccessBadgeOverrideRule
{
    public List<string> Check(DefaultAccessBadgeOverride item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultAccessBadgeOverrideCodeLengthRule(int MaxLength) : IDefaultAccessBadgeOverrideRule
{
    public List<string> Check(DefaultAccessBadgeOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeOverrideRequiredJobCodeRule(string Message) : IDefaultAccessBadgeOverrideRule
{
    public List<string> Check(DefaultAccessBadgeOverride item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeOverrideRequiredCodeRule(string Message) : IDefaultAccessBadgeOverrideRule
{
    public List<string> Check(DefaultAccessBadgeOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultAccessBadgeOverrideRequiredBadgeRule(string Message) : IDefaultAccessBadgeOverrideRule
{
    public List<string> Check(DefaultAccessBadgeOverride item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default access badge override.</summary>
public sealed record DefaultAccessBadgeOverrideValidator(List<IDefaultAccessBadgeOverrideRule> Rules)
{
    public List<string> Validate(DefaultAccessBadgeOverride item)
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

    public void RequireValid(DefaultAccessBadgeOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultAccessBadgeOverride> items)
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

/// <summary>Functions of default access badge override rules.</summary>
public static class DefaultAccessBadgeOverrideRules
{
    public static DefaultAccessBadgeOverrideValidator DefaultDefaultAccessBadgeOverrideValidator()
    {
        return new DefaultAccessBadgeOverrideValidator(
            new List<IDefaultAccessBadgeOverrideRule>
            {
                new DefaultAccessBadgeOverrideDiscountRangeRule(9.3, 884.9),
                new DefaultAccessBadgeOverrideCodeLengthRule(64),
                new DefaultAccessBadgeOverrideRequiredJobCodeRule("default access badge override needs a value"),
                new DefaultAccessBadgeOverrideRequiredCodeRule("default access badge override needs a value"),
                new DefaultAccessBadgeOverrideRequiredBadgeRule("default access badge override needs an entry"),
            });
    }
}
