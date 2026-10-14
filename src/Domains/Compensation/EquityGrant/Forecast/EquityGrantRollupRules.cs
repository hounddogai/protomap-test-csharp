namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>A rule a equity grant rollup must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantRollupRule
{
    List<string> Check(EquityGrantRollup item);
}

public sealed record EquityGrantRollupApprovedGuardRule(int Limit) : IEquityGrantRollupRule
{
    public List<string> Check(EquityGrantRollup item)
    {
        if (item.Approved && item.Limit > Limit)
        {
            return new List<string> { $"limit {item.Limit} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRollupCodeLengthRule(int MaxLength) : IEquityGrantRollupRule
{
    public List<string> Check(EquityGrantRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRollupNameLengthRule(int MaxLength) : IEquityGrantRollupRule
{
    public List<string> Check(EquityGrantRollup item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantRollupCodePrefixRule(string Prefix) : IEquityGrantRollupRule
{
    public List<string> Check(EquityGrantRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant rollup.</summary>
public sealed record EquityGrantRollupValidator(List<IEquityGrantRollupRule> Rules)
{
    public List<string> Validate(EquityGrantRollup item)
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

    public void RequireValid(EquityGrantRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantRollup> items)
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

/// <summary>Functions of equity grant rollup rules.</summary>
public static class EquityGrantRollupRules
{
    public static EquityGrantRollupValidator DefaultEquityGrantRollupValidator()
    {
        return new EquityGrantRollupValidator(
            new List<IEquityGrantRollupRule>
            {
                new EquityGrantRollupApprovedGuardRule(64),
                new EquityGrantRollupCodeLengthRule(3600),
                new EquityGrantRollupNameLengthRule(120),
                new EquityGrantRollupCodePrefixRule("X"),
            });
    }
}
