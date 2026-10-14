namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>A rule a reorg allocation must pass. Check returns its problems, or none.</summary>
public interface IReorgAllocationRule
{
    List<string> Check(ReorgAllocation item);
}

public sealed record ReorgAllocationRequiredManagerIdRule(string Message) : IReorgAllocationRule
{
    public List<string> Check(ReorgAllocation item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgAllocationCodeLengthRule(int MaxLength) : IReorgAllocationRule
{
    public List<string> Check(ReorgAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAllocationSkillLengthRule(int MaxLength) : IReorgAllocationRule
{
    public List<string> Check(ReorgAllocation item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgAllocationRequiredSkillRule(string Message) : IReorgAllocationRule
{
    public List<string> Check(ReorgAllocation item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgAllocationRequiredNameRule(string Message) : IReorgAllocationRule
{
    public List<string> Check(ReorgAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg allocation.</summary>
public sealed record ReorgAllocationValidator(List<IReorgAllocationRule> Rules)
{
    public List<string> Validate(ReorgAllocation item)
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

    public void RequireValid(ReorgAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgAllocation> items)
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

/// <summary>Functions of reorg allocation rules.</summary>
public static class ReorgAllocationRules
{
    public static ReorgAllocationValidator DefaultReorgAllocationValidator()
    {
        return new ReorgAllocationValidator(
            new List<IReorgAllocationRule>
            {
                new ReorgAllocationRequiredManagerIdRule("reorg allocation needs this field"),
                new ReorgAllocationCodeLengthRule(3600),
                new ReorgAllocationSkillLengthRule(3600),
                new ReorgAllocationRequiredSkillRule("reorg allocation needs this field"),
                new ReorgAllocationRequiredNameRule("reorg allocation needs this field"),
            });
    }
}
