namespace Acme.Workforce.Workplace.AccessBadge.Budget;

/// <summary>A rule a access badge cycle must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeCycleRule
{
    List<string> Check(AccessBadgeCycle item);
}

public sealed record AccessBadgeCycleRequiredCodeRule(string Message) : IAccessBadgeCycleRule
{
    public List<string> Check(AccessBadgeCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeCycleTitleLengthRule(int MaxLength) : IAccessBadgeCycleRule
{
    public List<string> Check(AccessBadgeCycle item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeCycleRequiredCurrencyRule(string Message) : IAccessBadgeCycleRule
{
    public List<string> Check(AccessBadgeCycle item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge cycle.</summary>
public sealed record AccessBadgeCycleValidator(List<IAccessBadgeCycleRule> Rules)
{
    public List<string> Validate(AccessBadgeCycle item)
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

    public void RequireValid(AccessBadgeCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeCycle> items)
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

/// <summary>Functions of access badge cycle rules.</summary>
public static class AccessBadgeCycleRules
{
    public static AccessBadgeCycleValidator DefaultAccessBadgeCycleValidator()
    {
        return new AccessBadgeCycleValidator(
            new List<IAccessBadgeCycleRule>
            {
                new AccessBadgeCycleRequiredCodeRule("access badge cycle needs this field"),
                new AccessBadgeCycleTitleLengthRule(1000),
                new AccessBadgeCycleRequiredCurrencyRule("access badge cycle needs an entry"),
            });
    }
}
