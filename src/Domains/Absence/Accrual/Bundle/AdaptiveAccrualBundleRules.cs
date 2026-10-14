namespace Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>A rule a adaptive accrual bundle must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAccrualBundleRule
{
    List<string> Check(AdaptiveAccrualBundle item);
}

public sealed record AdaptiveAccrualBundleRequiredLabelRule(string Message) : IAdaptiveAccrualBundleRule
{
    public List<string> Check(AdaptiveAccrualBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAccrualBundleBadgeLengthRule(int MaxLength) : IAdaptiveAccrualBundleRule
{
    public List<string> Check(AdaptiveAccrualBundle item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAccrualBundleCodePrefixRule(string Prefix) : IAdaptiveAccrualBundleRule
{
    public List<string> Check(AdaptiveAccrualBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive accrual bundle.</summary>
public sealed record AdaptiveAccrualBundleValidator(List<IAdaptiveAccrualBundleRule> Rules)
{
    public List<string> Validate(AdaptiveAccrualBundle item)
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

    public void RequireValid(AdaptiveAccrualBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAccrualBundle> items)
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

/// <summary>Functions of adaptive accrual bundle rules.</summary>
public static class AdaptiveAccrualBundleRules
{
    public static AdaptiveAccrualBundleValidator DefaultAdaptiveAccrualBundleValidator()
    {
        return new AdaptiveAccrualBundleValidator(
            new List<IAdaptiveAccrualBundleRule>
            {
                new AdaptiveAccrualBundleRequiredLabelRule("adaptive accrual bundle needs a value"),
                new AdaptiveAccrualBundleBadgeLengthRule(16),
                new AdaptiveAccrualBundleCodePrefixRule("REF"),
            });
    }
}
