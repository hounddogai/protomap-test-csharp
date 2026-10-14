namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg entry must pass. Check returns its problems, or none.</summary>
public interface IReorgEntryRule
{
    List<string> Check(ReorgEntry item);
}

public sealed record ReorgEntryRequiredCodeRule(string Message) : IReorgEntryRule
{
    public List<string> Check(ReorgEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgEntryAllowedCostCenterRule(List<string> Allowed) : IReorgEntryRule
{
    public List<string> Check(ReorgEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ReorgEntryCodeLengthRule(int MaxLength) : IReorgEntryRule
{
    public List<string> Check(ReorgEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg entry.</summary>
public sealed record ReorgEntryValidator(List<IReorgEntryRule> Rules)
{
    public List<string> Validate(ReorgEntry item)
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

    public void RequireValid(ReorgEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgEntry> items)
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

/// <summary>Functions of reorg entry rules.</summary>
public static class ReorgEntryRules
{
    public static ReorgEntryValidator DefaultReorgEntryValidator()
    {
        return new ReorgEntryValidator(
            new List<IReorgEntryRule>
            {
                new ReorgEntryRequiredCodeRule("reorg entry needs a value"),
                new ReorgEntryAllowedCostCenterRule(new List<string>()),
                new ReorgEntryCodeLengthRule(120),
            });
    }
}
