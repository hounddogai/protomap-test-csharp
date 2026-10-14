namespace Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>A rule a access badge plan must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgePlanRule
{
    List<string> Check(AccessBadgePlan item);
}

public sealed record AccessBadgePlanPhoneLengthRule(int MaxLength) : IAccessBadgePlanRule
{
    public List<string> Check(AccessBadgePlan item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePlanRequiredLocaleRule(string Message) : IAccessBadgePlanRule
{
    public List<string> Check(AccessBadgePlan item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePlanRequiredPhoneRule(string Message) : IAccessBadgePlanRule
{
    public List<string> Check(AccessBadgePlan item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePlanBatchSizeRangeRule(int Min, int Max) : IAccessBadgePlanRule
{
    public List<string> Check(AccessBadgePlan item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePlanLocaleLengthRule(int MaxLength) : IAccessBadgePlanRule
{
    public List<string> Check(AccessBadgePlan item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgePlanApprovedGuardRule(int Limit) : IAccessBadgePlanRule
{
    public List<string> Check(AccessBadgePlan item)
    {
        if (item.Approved && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge plan.</summary>
public sealed record AccessBadgePlanValidator(List<IAccessBadgePlanRule> Rules)
{
    public List<string> Validate(AccessBadgePlan item)
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

    public void RequireValid(AccessBadgePlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgePlan> items)
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

/// <summary>Functions of access badge plan rules.</summary>
public static class AccessBadgePlanRules
{
    public static AccessBadgePlanValidator DefaultAccessBadgePlanValidator()
    {
        return new AccessBadgePlanValidator(
            new List<IAccessBadgePlanRule>
            {
                new AccessBadgePlanPhoneLengthRule(64),
                new AccessBadgePlanRequiredLocaleRule("access badge plan needs an entry"),
                new AccessBadgePlanRequiredPhoneRule("access badge plan needs an entry"),
                new AccessBadgePlanBatchSizeRangeRule(255, 1000),
                new AccessBadgePlanLocaleLengthRule(120),
                new AccessBadgePlanApprovedGuardRule(80),
            });
    }
}
