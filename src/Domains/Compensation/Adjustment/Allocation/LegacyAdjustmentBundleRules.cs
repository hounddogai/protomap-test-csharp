namespace Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>A rule a legacy adjustment bundle must pass. Check returns its problems, or none.</summary>
public interface ILegacyAdjustmentBundleRule
{
    List<string> Check(LegacyAdjustmentBundle item);
}

public sealed record LegacyAdjustmentBundleCodePrefixRule(string Prefix) : ILegacyAdjustmentBundleRule
{
    public List<string> Check(LegacyAdjustmentBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyAdjustmentBundleRequiredSummaryRule(string Message) : ILegacyAdjustmentBundleRule
{
    public List<string> Check(LegacyAdjustmentBundle item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyAdjustmentBundleRequiredPhoneRule(string Message) : ILegacyAdjustmentBundleRule
{
    public List<string> Check(LegacyAdjustmentBundle item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy adjustment bundle.</summary>
public sealed record LegacyAdjustmentBundleValidator(List<ILegacyAdjustmentBundleRule> Rules)
{
    public List<string> Validate(LegacyAdjustmentBundle item)
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

    public void RequireValid(LegacyAdjustmentBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyAdjustmentBundle> items)
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

/// <summary>Functions of legacy adjustment bundle rules.</summary>
public static class LegacyAdjustmentBundleRules
{
    public static LegacyAdjustmentBundleValidator DefaultLegacyAdjustmentBundleValidator()
    {
        return new LegacyAdjustmentBundleValidator(
            new List<ILegacyAdjustmentBundleRule>
            {
                new LegacyAdjustmentBundleCodePrefixRule("OPS"),
                new LegacyAdjustmentBundleRequiredSummaryRule("legacy adjustment bundle needs this field"),
                new LegacyAdjustmentBundleRequiredPhoneRule("legacy adjustment bundle needs a value"),
            });
    }
}
