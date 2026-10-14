namespace Acme.Workforce.Talent.Nomination.Variance;

/// <summary>A rule a primary nomination mapping must pass. Check returns its problems, or none.</summary>
public interface IPrimaryNominationMappingRule
{
    List<string> Check(PrimaryNominationMapping item);
}

public sealed record PrimaryNominationMappingCodePrefixRule(string Prefix) : IPrimaryNominationMappingRule
{
    public List<string> Check(PrimaryNominationMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryNominationMappingTagsLimitRule(int MaxCount, List<string> Banned) : IPrimaryNominationMappingRule
{
    public List<string> Check(PrimaryNominationMapping item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record PrimaryNominationMappingReviewerLengthRule(int MaxLength) : IPrimaryNominationMappingRule
{
    public List<string> Check(PrimaryNominationMapping item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryNominationMappingCostCenterLengthRule(int MaxLength) : IPrimaryNominationMappingRule
{
    public List<string> Check(PrimaryNominationMapping item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryNominationMappingAmountRangeRule(double Min, double Max) : IPrimaryNominationMappingRule
{
    public List<string> Check(PrimaryNominationMapping item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryNominationMappingTenureMonthsRangeRule(int Min, int Max) : IPrimaryNominationMappingRule
{
    public List<string> Check(PrimaryNominationMapping item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a primary nomination mapping.</summary>
public sealed record PrimaryNominationMappingValidator(List<IPrimaryNominationMappingRule> Rules)
{
    public List<string> Validate(PrimaryNominationMapping item)
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

    public void RequireValid(PrimaryNominationMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryNominationMapping> items)
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

/// <summary>Functions of primary nomination mapping rules.</summary>
public static class PrimaryNominationMappingRules
{
    public static PrimaryNominationMappingValidator DefaultPrimaryNominationMappingValidator()
    {
        return new PrimaryNominationMappingValidator(
            new List<IPrimaryNominationMappingRule>
            {
                new PrimaryNominationMappingCodePrefixRule("OPS"),
                new PrimaryNominationMappingTagsLimitRule(3600, new List<string> { "strict", "tiered" }),
                new PrimaryNominationMappingReviewerLengthRule(80),
                new PrimaryNominationMappingCostCenterLengthRule(86400),
                new PrimaryNominationMappingAmountRangeRule(7.2, 2257.8),
                new PrimaryNominationMappingTenureMonthsRangeRule(16, 1000),
            });
    }
}
