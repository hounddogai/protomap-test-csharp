namespace Acme.Workforce.Talent.Pool.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered pool exception must pass. Check returns its problems, or none.</summary>
public interface ITieredPoolExceptionRule
{
    List<string> Check(TieredPoolException item);
}

public sealed record TieredPoolExceptionNameLengthRule(int MaxLength) : ITieredPoolExceptionRule
{
    public List<string> Check(TieredPoolException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolExceptionSkillLengthRule(int MaxLength) : ITieredPoolExceptionRule
{
    public List<string> Check(TieredPoolException item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolExceptionArchivedGuardRule(int Limit) : ITieredPoolExceptionRule
{
    public List<string> Check(TieredPoolException item)
    {
        if (item.Archived && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolExceptionRequiredSkillRule(string Message) : ITieredPoolExceptionRule
{
    public List<string> Check(TieredPoolException item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolExceptionAllowedDepartmentRule(List<string> Allowed) : ITieredPoolExceptionRule
{
    public List<string> Check(TieredPoolException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record TieredPoolExceptionRequiredCodeRule(string Message) : ITieredPoolExceptionRule
{
    public List<string> Check(TieredPoolException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pool exception.</summary>
public sealed record TieredPoolExceptionValidator(List<ITieredPoolExceptionRule> Rules)
{
    public List<string> Validate(TieredPoolException item)
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

    public void RequireValid(TieredPoolException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPoolException> items)
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

/// <summary>Functions of tiered pool exception rules.</summary>
public static class TieredPoolExceptionRules
{
    public static TieredPoolExceptionValidator DefaultTieredPoolExceptionValidator()
    {
        return new TieredPoolExceptionValidator(
            new List<ITieredPoolExceptionRule>
            {
                new TieredPoolExceptionNameLengthRule(64),
                new TieredPoolExceptionSkillLengthRule(1000),
                new TieredPoolExceptionArchivedGuardRule(86400),
                new TieredPoolExceptionRequiredSkillRule("tiered pool exception needs an entry"),
                new TieredPoolExceptionAllowedDepartmentRule(new List<string> { "regional", "seasonal", "fallback" }),
                new TieredPoolExceptionRequiredCodeRule("tiered pool exception needs an entry"),
            });
    }
}
