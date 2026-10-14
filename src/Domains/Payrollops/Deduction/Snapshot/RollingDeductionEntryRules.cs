namespace Acme.Workforce.Payrollops.Deduction.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling deduction entry must pass. Check returns its problems, or none.</summary>
public interface IRollingDeductionEntryRule
{
    List<string> Check(RollingDeductionEntry item);
}

public sealed record RollingDeductionEntryCodePrefixRule(string Prefix) : IRollingDeductionEntryRule
{
    public List<string> Check(RollingDeductionEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingDeductionEntryAllowedTitleRule(List<string> Allowed) : IRollingDeductionEntryRule
{
    public List<string> Check(RollingDeductionEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RollingDeductionEntryRequiredCodeRule(string Message) : IRollingDeductionEntryRule
{
    public List<string> Check(RollingDeductionEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling deduction entry.</summary>
public sealed record RollingDeductionEntryValidator(List<IRollingDeductionEntryRule> Rules)
{
    public List<string> Validate(RollingDeductionEntry item)
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

    public void RequireValid(RollingDeductionEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingDeductionEntry> items)
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

/// <summary>Functions of rolling deduction entry rules.</summary>
public static class RollingDeductionEntryRules
{
    public static RollingDeductionEntryValidator DefaultRollingDeductionEntryValidator()
    {
        return new RollingDeductionEntryValidator(
            new List<IRollingDeductionEntryRule>
            {
                new RollingDeductionEntryCodePrefixRule("X"),
                new RollingDeductionEntryAllowedTitleRule(new List<string>()),
                new RollingDeductionEntryRequiredCodeRule("rolling deduction entry needs this field"),
            });
    }
}
