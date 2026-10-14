namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling pool entry must pass. Check returns its problems, or none.</summary>
public interface IRollingPoolEntryRule
{
    List<string> Check(RollingPoolEntry item);
}

public sealed record RollingPoolEntryCategoryLengthRule(int MaxLength) : IRollingPoolEntryRule
{
    public List<string> Check(RollingPoolEntry item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolEntryRequiredTitleRule(string Message) : IRollingPoolEntryRule
{
    public List<string> Check(RollingPoolEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPoolEntryAllowedCategoryRule(List<string> Allowed) : IRollingPoolEntryRule
{
    public List<string> Check(RollingPoolEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pool entry.</summary>
public sealed record RollingPoolEntryValidator(List<IRollingPoolEntryRule> Rules)
{
    public List<string> Validate(RollingPoolEntry item)
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

    public void RequireValid(RollingPoolEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPoolEntry> items)
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

/// <summary>Functions of rolling pool entry rules.</summary>
public static class RollingPoolEntryRules
{
    public static RollingPoolEntryValidator DefaultRollingPoolEntryValidator()
    {
        return new RollingPoolEntryValidator(
            new List<IRollingPoolEntryRule>
            {
                new RollingPoolEntryCategoryLengthRule(80),
                new RollingPoolEntryRequiredTitleRule("rolling pool entry needs this field"),
                new RollingPoolEntryAllowedCategoryRule(new List<string> { "manual" }),
            });
    }
}
