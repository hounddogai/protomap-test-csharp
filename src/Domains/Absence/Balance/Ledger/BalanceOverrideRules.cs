namespace Acme.Workforce.Absence.Balance.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance override must pass. Check returns its problems, or none.</summary>
public interface IBalanceOverrideRule
{
    List<string> Check(BalanceOverride item);
}

public sealed record BalanceOverrideRegionLengthRule(int MaxLength) : IBalanceOverrideRule
{
    public List<string> Check(BalanceOverride item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceOverrideAllowedCodeRule(List<string> Allowed) : IBalanceOverrideRule
{
    public List<string> Check(BalanceOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record BalanceOverrideCodePrefixRule(string Prefix) : IBalanceOverrideRule
{
    public List<string> Check(BalanceOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceOverrideRequiredLocaleRule(string Message) : IBalanceOverrideRule
{
    public List<string> Check(BalanceOverride item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance override.</summary>
public sealed record BalanceOverrideValidator(List<IBalanceOverrideRule> Rules)
{
    public List<string> Validate(BalanceOverride item)
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

    public void RequireValid(BalanceOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceOverride> items)
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

/// <summary>Functions of balance override rules.</summary>
public static class BalanceOverrideRules
{
    public static BalanceOverrideValidator DefaultBalanceOverrideValidator()
    {
        return new BalanceOverrideValidator(
            new List<IBalanceOverrideRule>
            {
                new BalanceOverrideRegionLengthRule(255),
                new BalanceOverrideAllowedCodeRule(new List<string>()),
                new BalanceOverrideCodePrefixRule("X"),
                new BalanceOverrideRequiredLocaleRule("balance override needs a value"),
            });
    }
}
