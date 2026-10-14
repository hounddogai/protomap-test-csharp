namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>A rule a primary bonus pool group must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBonusPoolGroupRule
{
    List<string> Check(PrimaryBonusPoolGroup item);
}

public sealed record PrimaryBonusPoolGroupCodeLengthRule(int MaxLength) : IPrimaryBonusPoolGroupRule
{
    public List<string> Check(PrimaryBonusPoolGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolGroupEmployeeIdLengthRule(int MaxLength) : IPrimaryBonusPoolGroupRule
{
    public List<string> Check(PrimaryBonusPoolGroup item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolGroupRequiredEmployeeIdRule(string Message) : IPrimaryBonusPoolGroupRule
{
    public List<string> Check(PrimaryBonusPoolGroup item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolGroupRequiredNameRule(string Message) : IPrimaryBonusPoolGroupRule
{
    public List<string> Check(PrimaryBonusPoolGroup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary bonus pool group.</summary>
public sealed record PrimaryBonusPoolGroupValidator(List<IPrimaryBonusPoolGroupRule> Rules)
{
    public List<string> Validate(PrimaryBonusPoolGroup item)
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

    public void RequireValid(PrimaryBonusPoolGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBonusPoolGroup> items)
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

/// <summary>Functions of primary bonus pool group rules.</summary>
public static class PrimaryBonusPoolGroupRules
{
    public static PrimaryBonusPoolGroupValidator DefaultPrimaryBonusPoolGroupValidator()
    {
        return new PrimaryBonusPoolGroupValidator(
            new List<IPrimaryBonusPoolGroupRule>
            {
                new PrimaryBonusPoolGroupCodeLengthRule(3600),
                new PrimaryBonusPoolGroupEmployeeIdLengthRule(32),
                new PrimaryBonusPoolGroupRequiredEmployeeIdRule("primary bonus pool group needs an entry"),
                new PrimaryBonusPoolGroupRequiredNameRule("primary bonus pool group needs this field"),
            });
    }
}
