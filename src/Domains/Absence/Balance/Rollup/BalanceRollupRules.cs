namespace Acme.Workforce.Absence.Balance.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a balance rollup must pass. Check returns its problems, or none.</summary>
public interface IBalanceRollupRule
{
    List<string> Check(BalanceRollup item);
}

public sealed record BalanceRollupCodePrefixRule(string Prefix) : IBalanceRollupRule
{
    public List<string> Check(BalanceRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRollupArchivedGuardRule(double Limit) : IBalanceRollupRule
{
    public List<string> Check(BalanceRollup item)
    {
        if (item.Archived && item.Margin > Limit)
        {
            return new List<string> { $"margin {item.Margin} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRollupAllowedCodeRule(List<string> Allowed) : IBalanceRollupRule
{
    public List<string> Check(BalanceRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRollupMarginRangeRule(double Min, double Max) : IBalanceRollupRule
{
    public List<string> Check(BalanceRollup item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BalanceRollupUpdatedAtRangeRule(int Min, int Max) : IBalanceRollupRule
{
    public List<string> Check(BalanceRollup item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance rollup.</summary>
public sealed record BalanceRollupValidator(List<IBalanceRollupRule> Rules)
{
    public List<string> Validate(BalanceRollup item)
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

    public void RequireValid(BalanceRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceRollup> items)
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

/// <summary>Functions of balance rollup rules.</summary>
public static class BalanceRollupRules
{
    public static BalanceRollupValidator DefaultBalanceRollupValidator()
    {
        return new BalanceRollupValidator(
            new List<IBalanceRollupRule>
            {
                new BalanceRollupCodePrefixRule("Q"),
                new BalanceRollupArchivedGuardRule(0.3),
                new BalanceRollupAllowedCodeRule(new List<string>()),
                new BalanceRollupMarginRangeRule(6.8, 369.2),
                new BalanceRollupUpdatedAtRangeRule(32, 1000),
            });
    }
}
