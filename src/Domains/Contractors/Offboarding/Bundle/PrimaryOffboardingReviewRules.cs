namespace Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>A rule a primary offboarding review must pass. Check returns its problems, or none.</summary>
public interface IPrimaryOffboardingReviewRule
{
    List<string> Check(PrimaryOffboardingReview item);
}

public sealed record PrimaryOffboardingReviewCodeLengthRule(int MaxLength) : IPrimaryOffboardingReviewRule
{
    public List<string> Check(PrimaryOffboardingReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOffboardingReviewSegmentLengthRule(int MaxLength) : IPrimaryOffboardingReviewRule
{
    public List<string> Check(PrimaryOffboardingReview item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOffboardingReviewCapacityRangeRule(int Min, int Max) : IPrimaryOffboardingReviewRule
{
    public List<string> Check(PrimaryOffboardingReview item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a primary offboarding review.</summary>
public sealed record PrimaryOffboardingReviewValidator(List<IPrimaryOffboardingReviewRule> Rules)
{
    public List<string> Validate(PrimaryOffboardingReview item)
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

    public void RequireValid(PrimaryOffboardingReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryOffboardingReview> items)
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

/// <summary>Functions of primary offboarding review rules.</summary>
public static class PrimaryOffboardingReviewRules
{
    public static PrimaryOffboardingReviewValidator DefaultPrimaryOffboardingReviewValidator()
    {
        return new PrimaryOffboardingReviewValidator(
            new List<IPrimaryOffboardingReviewRule>
            {
                new PrimaryOffboardingReviewCodeLengthRule(16),
                new PrimaryOffboardingReviewSegmentLengthRule(80),
                new PrimaryOffboardingReviewCapacityRangeRule(255, 3600),
            });
    }
}
