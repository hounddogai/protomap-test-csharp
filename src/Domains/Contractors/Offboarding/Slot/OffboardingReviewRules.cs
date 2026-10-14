namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding review must pass. Check returns its problems, or none.</summary>
public interface IOffboardingReviewRule
{
    List<string> Check(OffboardingReview item);
}

public sealed record OffboardingReviewAmountRangeRule(double Min, double Max) : IOffboardingReviewRule
{
    public List<string> Check(OffboardingReview item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingReviewRequiredCountryRule(string Message) : IOffboardingReviewRule
{
    public List<string> Check(OffboardingReview item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingReviewCodeLengthRule(int MaxLength) : IOffboardingReviewRule
{
    public List<string> Check(OffboardingReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingReviewAllowedCountryRule(List<string> Allowed) : IOffboardingReviewRule
{
    public List<string> Check(OffboardingReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding review.</summary>
public sealed record OffboardingReviewValidator(List<IOffboardingReviewRule> Rules)
{
    public List<string> Validate(OffboardingReview item)
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

    public void RequireValid(OffboardingReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingReview> items)
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

/// <summary>Functions of offboarding review rules.</summary>
public static class OffboardingReviewRules
{
    public static OffboardingReviewValidator DefaultOffboardingReviewValidator()
    {
        return new OffboardingReviewValidator(
            new List<IOffboardingReviewRule>
            {
                new OffboardingReviewAmountRangeRule(5.3, 2803.4),
                new OffboardingReviewRequiredCountryRule("offboarding review needs this field"),
                new OffboardingReviewCodeLengthRule(255),
                new OffboardingReviewAllowedCountryRule(new List<string> { "weighted", "adaptive" }),
            });
    }
}
