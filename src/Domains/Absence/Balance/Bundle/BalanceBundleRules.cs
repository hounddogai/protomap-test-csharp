namespace Acme.Workforce.Absence.Balance.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance bundle must pass. Check returns its problems, or none.</summary>
public interface IBalanceBundleRule
{
    List<string> Check(BalanceBundle item);
}

public sealed record BalanceBundleChannelLengthRule(int MaxLength) : IBalanceBundleRule
{
    public List<string> Check(BalanceBundle item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBundleRequiredCodeRule(string Message) : IBalanceBundleRule
{
    public List<string> Check(BalanceBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BalanceBundleAllowedLabelRule(List<string> Allowed) : IBalanceBundleRule
{
    public List<string> Check(BalanceBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBundleDepartmentLengthRule(int MaxLength) : IBalanceBundleRule
{
    public List<string> Check(BalanceBundle item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBundleCodePrefixRule(string Prefix) : IBalanceBundleRule
{
    public List<string> Check(BalanceBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceBundleUpdatedAtRangeRule(int Min, int Max) : IBalanceBundleRule
{
    public List<string> Check(BalanceBundle item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance bundle.</summary>
public sealed record BalanceBundleValidator(List<IBalanceBundleRule> Rules)
{
    public List<string> Validate(BalanceBundle item)
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

    public void RequireValid(BalanceBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceBundle> items)
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

/// <summary>Functions of balance bundle rules.</summary>
public static class BalanceBundleRules
{
    public static BalanceBundleValidator DefaultBalanceBundleValidator()
    {
        return new BalanceBundleValidator(
            new List<IBalanceBundleRule>
            {
                new BalanceBundleChannelLengthRule(64),
                new BalanceBundleRequiredCodeRule("balance bundle needs this field"),
                new BalanceBundleAllowedLabelRule(new List<string> { "rolling", "regional" }),
                new BalanceBundleDepartmentLengthRule(32),
                new BalanceBundleCodePrefixRule("REF"),
                new BalanceBundleUpdatedAtRangeRule(32, 3600),
            });
    }
}
