namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

/// <summary>A rule a manual exception group must pass. Check returns its problems, or none.</summary>
public interface IManualExceptionGroupRule
{
    List<string> Check(ManualExceptionGroup item);
}

public sealed record ManualExceptionGroupCodeLengthRule(int MaxLength) : IManualExceptionGroupRule
{
    public List<string> Check(ManualExceptionGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualExceptionGroupRequiredNameRule(string Message) : IManualExceptionGroupRule
{
    public List<string> Check(ManualExceptionGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualExceptionGroupPinnedGuardRule(int Limit) : IManualExceptionGroupRule
{
    public List<string> Check(ManualExceptionGroup item)
    {
        if (item.Pinned && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record ManualExceptionGroupManagerIdLengthRule(int MaxLength) : IManualExceptionGroupRule
{
    public List<string> Check(ManualExceptionGroup item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual exception group.</summary>
public sealed record ManualExceptionGroupValidator(List<IManualExceptionGroupRule> Rules)
{
    public List<string> Validate(ManualExceptionGroup item)
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

    public void RequireValid(ManualExceptionGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualExceptionGroup> items)
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

/// <summary>Functions of manual exception group rules.</summary>
public static class ManualExceptionGroupRules
{
    public static ManualExceptionGroupValidator DefaultManualExceptionGroupValidator()
    {
        return new ManualExceptionGroupValidator(
            new List<IManualExceptionGroupRule>
            {
                new ManualExceptionGroupCodeLengthRule(32),
                new ManualExceptionGroupRequiredNameRule("manual exception group needs an entry"),
                new ManualExceptionGroupPinnedGuardRule(16),
                new ManualExceptionGroupManagerIdLengthRule(3600),
            });
    }
}
