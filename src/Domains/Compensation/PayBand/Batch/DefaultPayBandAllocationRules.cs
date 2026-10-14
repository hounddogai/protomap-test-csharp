namespace Acme.Workforce.Compensation.PayBand.Batch;

/// <summary>A rule a default pay band allocation must pass. Check returns its problems, or none.</summary>
public interface IDefaultPayBandAllocationRule
{
    List<string> Check(DefaultPayBandAllocation item);
}

public sealed record DefaultPayBandAllocationCodeLengthRule(int MaxLength) : IDefaultPayBandAllocationRule
{
    public List<string> Check(DefaultPayBandAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayBandAllocationCodePrefixRule(string Prefix) : IDefaultPayBandAllocationRule
{
    public List<string> Check(DefaultPayBandAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultPayBandAllocationVersionRangeRule(int Min, int Max) : IDefaultPayBandAllocationRule
{
    public List<string> Check(DefaultPayBandAllocation item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultPayBandAllocationCategoryLengthRule(int MaxLength) : IDefaultPayBandAllocationRule
{
    public List<string> Check(DefaultPayBandAllocation item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default pay band allocation.</summary>
public sealed record DefaultPayBandAllocationValidator(List<IDefaultPayBandAllocationRule> Rules)
{
    public List<string> Validate(DefaultPayBandAllocation item)
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

    public void RequireValid(DefaultPayBandAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultPayBandAllocation> items)
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

/// <summary>Functions of default pay band allocation rules.</summary>
public static class DefaultPayBandAllocationRules
{
    public static DefaultPayBandAllocationValidator DefaultDefaultPayBandAllocationValidator()
    {
        return new DefaultPayBandAllocationValidator(
            new List<IDefaultPayBandAllocationRule>
            {
                new DefaultPayBandAllocationCodeLengthRule(32),
                new DefaultPayBandAllocationCodePrefixRule("ACM"),
                new DefaultPayBandAllocationVersionRangeRule(32, 3600),
                new DefaultPayBandAllocationCategoryLengthRule(16),
            });
    }
}
