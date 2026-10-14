namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>A rule a legacy bonus pool allocation must pass. Check returns its problems, or none.</summary>
public interface ILegacyBonusPoolAllocationRule
{
    List<string> Check(LegacyBonusPoolAllocation item);
}

public sealed record LegacyBonusPoolAllocationRequiredCodeRule(string Message) : ILegacyBonusPoolAllocationRule
{
    public List<string> Check(LegacyBonusPoolAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyBonusPoolAllocationSegmentLengthRule(int MaxLength) : ILegacyBonusPoolAllocationRule
{
    public List<string> Check(LegacyBonusPoolAllocation item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyBonusPoolAllocationCodeLengthRule(int MaxLength) : ILegacyBonusPoolAllocationRule
{
    public List<string> Check(LegacyBonusPoolAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy bonus pool allocation.</summary>
public sealed record LegacyBonusPoolAllocationValidator(List<ILegacyBonusPoolAllocationRule> Rules)
{
    public List<string> Validate(LegacyBonusPoolAllocation item)
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

    public void RequireValid(LegacyBonusPoolAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyBonusPoolAllocation> items)
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

/// <summary>Functions of legacy bonus pool allocation rules.</summary>
public static class LegacyBonusPoolAllocationRules
{
    public static LegacyBonusPoolAllocationValidator DefaultLegacyBonusPoolAllocationValidator()
    {
        return new LegacyBonusPoolAllocationValidator(
            new List<ILegacyBonusPoolAllocationRule>
            {
                new LegacyBonusPoolAllocationRequiredCodeRule("legacy bonus pool allocation needs an entry"),
                new LegacyBonusPoolAllocationSegmentLengthRule(86400),
                new LegacyBonusPoolAllocationCodeLengthRule(80),
            });
    }
}
