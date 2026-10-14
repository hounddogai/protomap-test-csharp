namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending pool allocation must pass. Check returns its problems, or none.</summary>
public interface IPendingPoolAllocationRule
{
    List<string> Check(PendingPoolAllocation item);
}

public sealed record PendingPoolAllocationUnitLengthRule(int MaxLength) : IPendingPoolAllocationRule
{
    public List<string> Check(PendingPoolAllocation item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPoolAllocationAllowedUnitRule(List<string> Allowed) : IPendingPoolAllocationRule
{
    public List<string> Check(PendingPoolAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PendingPoolAllocationCodePrefixRule(string Prefix) : IPendingPoolAllocationRule
{
    public List<string> Check(PendingPoolAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pool allocation.</summary>
public sealed record PendingPoolAllocationValidator(List<IPendingPoolAllocationRule> Rules)
{
    public List<string> Validate(PendingPoolAllocation item)
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

    public void RequireValid(PendingPoolAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPoolAllocation> items)
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

/// <summary>Functions of pending pool allocation rules.</summary>
public static class PendingPoolAllocationRules
{
    public static PendingPoolAllocationValidator DefaultPendingPoolAllocationValidator()
    {
        return new PendingPoolAllocationValidator(
            new List<IPendingPoolAllocationRule>
            {
                new PendingPoolAllocationUnitLengthRule(120),
                new PendingPoolAllocationAllowedUnitRule(new List<string>()),
                new PendingPoolAllocationCodePrefixRule("OPS"),
            });
    }
}
