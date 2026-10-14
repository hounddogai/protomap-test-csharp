namespace Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>A rule a default policy item must pass. Check returns its problems, or none.</summary>
public interface IDefaultPolicyItemRule
{
    List<string> Check(DefaultPolicyItem item);
}

public sealed record DefaultPolicyItemRequiredNameRule(string Message) : IDefaultPolicyItemRule
{
    public List<string> Check(DefaultPolicyItem item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyItemExternalRefLengthRule(int MaxLength) : IDefaultPolicyItemRule
{
    public List<string> Check(DefaultPolicyItem item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyItemApprovedGuardRule(int Limit) : IDefaultPolicyItemRule
{
    public List<string> Check(DefaultPolicyItem item)
    {
        if (item.Approved && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyItemRetriesRangeRule(int Min, int Max) : IDefaultPolicyItemRule
{
    public List<string> Check(DefaultPolicyItem item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyItemRequiredExternalRefRule(string Message) : IDefaultPolicyItemRule
{
    public List<string> Check(DefaultPolicyItem item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultPolicyItemCapacityRangeRule(int Min, int Max) : IDefaultPolicyItemRule
{
    public List<string> Check(DefaultPolicyItem item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a default policy item.</summary>
public sealed record DefaultPolicyItemValidator(List<IDefaultPolicyItemRule> Rules)
{
    public List<string> Validate(DefaultPolicyItem item)
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

    public void RequireValid(DefaultPolicyItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPolicyItem> items)
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

/// <summary>Functions of default policy item rules.</summary>
public static class DefaultPolicyItemRules
{
    public static DefaultPolicyItemValidator DefaultDefaultPolicyItemValidator()
    {
        return new DefaultPolicyItemValidator(
            new List<IDefaultPolicyItemRule>
            {
                new DefaultPolicyItemRequiredNameRule("default policy item needs a value"),
                new DefaultPolicyItemExternalRefLengthRule(1000),
                new DefaultPolicyItemApprovedGuardRule(64),
                new DefaultPolicyItemRetriesRangeRule(64, 86400),
                new DefaultPolicyItemRequiredExternalRefRule("default policy item needs this field"),
                new DefaultPolicyItemCapacityRangeRule(16, 64),
            });
    }
}
