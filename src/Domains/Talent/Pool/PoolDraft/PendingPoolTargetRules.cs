namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending pool target must pass. Check returns its problems, or none.</summary>
public interface IPendingPoolTargetRule
{
    List<string> Check(PendingPoolTarget item);
}

public sealed record PendingPoolTargetCodeLengthRule(int MaxLength) : IPendingPoolTargetRule
{
    public List<string> Check(PendingPoolTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPoolTargetEmployeeIdLengthRule(int MaxLength) : IPendingPoolTargetRule
{
    public List<string> Check(PendingPoolTarget item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPoolTargetRequiredCodeRule(string Message) : IPendingPoolTargetRule
{
    public List<string> Check(PendingPoolTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingPoolTargetAllowedSkillRule(List<string> Allowed) : IPendingPoolTargetRule
{
    public List<string> Check(PendingPoolTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PendingPoolTargetActiveGuardRule(int Limit) : IPendingPoolTargetRule
{
    public List<string> Check(PendingPoolTarget item)
    {
        if (item.Active && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PendingPoolTargetRequiredEmployeeIdRule(string Message) : IPendingPoolTargetRule
{
    public List<string> Check(PendingPoolTarget item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pool target.</summary>
public sealed record PendingPoolTargetValidator(List<IPendingPoolTargetRule> Rules)
{
    public List<string> Validate(PendingPoolTarget item)
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

    public void RequireValid(PendingPoolTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPoolTarget> items)
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

/// <summary>Functions of pending pool target rules.</summary>
public static class PendingPoolTargetRules
{
    public static PendingPoolTargetValidator DefaultPendingPoolTargetValidator()
    {
        return new PendingPoolTargetValidator(
            new List<IPendingPoolTargetRule>
            {
                new PendingPoolTargetCodeLengthRule(3600),
                new PendingPoolTargetEmployeeIdLengthRule(120),
                new PendingPoolTargetRequiredCodeRule("pending pool target needs an entry"),
                new PendingPoolTargetAllowedSkillRule(new List<string> { "scheduled", "pending", "legacy" }),
                new PendingPoolTargetActiveGuardRule(32),
                new PendingPoolTargetRequiredEmployeeIdRule("pending pool target needs this field"),
            });
    }
}
