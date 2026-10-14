namespace Acme.Workforce.Absence.Policy.Budget;

/// <summary>A rule a lenient policy quota must pass. Check returns its problems, or none.</summary>
public interface ILenientPolicyQuotaRule
{
    List<string> Check(LenientPolicyQuota item);
}

public sealed record LenientPolicyQuotaCodeLengthRule(int MaxLength) : ILenientPolicyQuotaRule
{
    public List<string> Check(LenientPolicyQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyQuotaCodePrefixRule(string Prefix) : ILenientPolicyQuotaRule
{
    public List<string> Check(LenientPolicyQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyQuotaSegmentLengthRule(int MaxLength) : ILenientPolicyQuotaRule
{
    public List<string> Check(LenientPolicyQuota item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyQuotaRatioRangeRule(double Min, double Max) : ILenientPolicyQuotaRule
{
    public List<string> Check(LenientPolicyQuota item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient policy quota.</summary>
public sealed record LenientPolicyQuotaValidator(List<ILenientPolicyQuotaRule> Rules)
{
    public List<string> Validate(LenientPolicyQuota item)
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

    public void RequireValid(LenientPolicyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPolicyQuota> items)
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

/// <summary>Functions of lenient policy quota rules.</summary>
public static class LenientPolicyQuotaRules
{
    public static LenientPolicyQuotaValidator DefaultLenientPolicyQuotaValidator()
    {
        return new LenientPolicyQuotaValidator(
            new List<ILenientPolicyQuotaRule>
            {
                new LenientPolicyQuotaCodeLengthRule(3600),
                new LenientPolicyQuotaCodePrefixRule("OPS"),
                new LenientPolicyQuotaSegmentLengthRule(64),
                new LenientPolicyQuotaRatioRangeRule(6.9, 450.8),
            });
    }
}
