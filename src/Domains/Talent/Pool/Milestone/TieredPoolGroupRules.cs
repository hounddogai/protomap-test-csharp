namespace Acme.Workforce.Talent.Pool.Milestone;

/// <summary>A rule a tiered pool group must pass. Check returns its problems, or none.</summary>
public interface ITieredPoolGroupRule
{
    List<string> Check(TieredPoolGroup item);
}

public sealed record TieredPoolGroupRetriesRangeRule(int Min, int Max) : ITieredPoolGroupRule
{
    public List<string> Check(TieredPoolGroup item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolGroupNameLengthRule(int MaxLength) : ITieredPoolGroupRule
{
    public List<string> Check(TieredPoolGroup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolGroupRequiredDepartmentRule(string Message) : ITieredPoolGroupRule
{
    public List<string> Check(TieredPoolGroup item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolGroupVisibleGuardRule(int Limit) : ITieredPoolGroupRule
{
    public List<string> Check(TieredPoolGroup item)
    {
        if (item.Visible && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pool group.</summary>
public sealed record TieredPoolGroupValidator(List<ITieredPoolGroupRule> Rules)
{
    public List<string> Validate(TieredPoolGroup item)
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

    public void RequireValid(TieredPoolGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPoolGroup> items)
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

/// <summary>Functions of tiered pool group rules.</summary>
public static class TieredPoolGroupRules
{
    public static TieredPoolGroupValidator DefaultTieredPoolGroupValidator()
    {
        return new TieredPoolGroupValidator(
            new List<ITieredPoolGroupRule>
            {
                new TieredPoolGroupRetriesRangeRule(80, 255),
                new TieredPoolGroupNameLengthRule(86400),
                new TieredPoolGroupRequiredDepartmentRule("tiered pool group needs a value"),
                new TieredPoolGroupVisibleGuardRule(3600),
            });
    }
}
