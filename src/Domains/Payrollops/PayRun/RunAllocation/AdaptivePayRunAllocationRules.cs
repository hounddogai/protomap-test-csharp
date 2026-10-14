namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive pay run allocation must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePayRunAllocationRule
{
    List<string> Check(AdaptivePayRunAllocation item);
}

public sealed record AdaptivePayRunAllocationRateRangeRule(double Min, double Max) : IAdaptivePayRunAllocationRule
{
    public List<string> Check(AdaptivePayRunAllocation item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptivePayRunAllocationCodeLengthRule(int MaxLength) : IAdaptivePayRunAllocationRule
{
    public List<string> Check(AdaptivePayRunAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayRunAllocationAllowedSummaryRule(List<string> Allowed) : IAdaptivePayRunAllocationRule
{
    public List<string> Check(AdaptivePayRunAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayRunAllocationTitleLengthRule(int MaxLength) : IAdaptivePayRunAllocationRule
{
    public List<string> Check(AdaptivePayRunAllocation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayRunAllocationCodePrefixRule(string Prefix) : IAdaptivePayRunAllocationRule
{
    public List<string> Check(AdaptivePayRunAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePayRunAllocationRequiredSummaryRule(string Message) : IAdaptivePayRunAllocationRule
{
    public List<string> Check(AdaptivePayRunAllocation item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive pay run allocation.</summary>
public sealed record AdaptivePayRunAllocationValidator(List<IAdaptivePayRunAllocationRule> Rules)
{
    public List<string> Validate(AdaptivePayRunAllocation item)
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

    public void RequireValid(AdaptivePayRunAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePayRunAllocation> items)
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

/// <summary>Functions of adaptive pay run allocation rules.</summary>
public static class AdaptivePayRunAllocationRules
{
    public static AdaptivePayRunAllocationValidator DefaultAdaptivePayRunAllocationValidator()
    {
        return new AdaptivePayRunAllocationValidator(
            new List<IAdaptivePayRunAllocationRule>
            {
                new AdaptivePayRunAllocationRateRangeRule(1.7, 3454.2),
                new AdaptivePayRunAllocationCodeLengthRule(255),
                new AdaptivePayRunAllocationAllowedSummaryRule(new List<string> { "primary", "seasonal" }),
                new AdaptivePayRunAllocationTitleLengthRule(16),
                new AdaptivePayRunAllocationCodePrefixRule("OPS"),
                new AdaptivePayRunAllocationRequiredSummaryRule("adaptive pay run allocation needs an entry"),
            });
    }
}
