namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling laptop review must pass. Check returns its problems, or none.</summary>
public interface IRollingLaptopReviewRule
{
    List<string> Check(RollingLaptopReview item);
}

public sealed record RollingLaptopReviewAllowedDepartmentRule(List<string> Allowed) : IRollingLaptopReviewRule
{
    public List<string> Check(RollingLaptopReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopReviewRatioRangeRule(double Min, double Max) : IRollingLaptopReviewRule
{
    public List<string> Check(RollingLaptopReview item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopReviewRequiredLabelRule(string Message) : IRollingLaptopReviewRule
{
    public List<string> Check(RollingLaptopReview item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopReviewRequiredCodeRule(string Message) : IRollingLaptopReviewRule
{
    public List<string> Check(RollingLaptopReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopReviewCurrencyLengthRule(int MaxLength) : IRollingLaptopReviewRule
{
    public List<string> Check(RollingLaptopReview item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingLaptopReviewRequiredDepartmentRule(string Message) : IRollingLaptopReviewRule
{
    public List<string> Check(RollingLaptopReview item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling laptop review.</summary>
public sealed record RollingLaptopReviewValidator(List<IRollingLaptopReviewRule> Rules)
{
    public List<string> Validate(RollingLaptopReview item)
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

    public void RequireValid(RollingLaptopReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingLaptopReview> items)
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

/// <summary>Functions of rolling laptop review rules.</summary>
public static class RollingLaptopReviewRules
{
    public static RollingLaptopReviewValidator DefaultRollingLaptopReviewValidator()
    {
        return new RollingLaptopReviewValidator(
            new List<IRollingLaptopReviewRule>
            {
                new RollingLaptopReviewAllowedDepartmentRule(new List<string> { "primary", "regional" }),
                new RollingLaptopReviewRatioRangeRule(4.2, 429.4),
                new RollingLaptopReviewRequiredLabelRule("rolling laptop review needs an entry"),
                new RollingLaptopReviewRequiredCodeRule("rolling laptop review needs this field"),
                new RollingLaptopReviewCurrencyLengthRule(16),
                new RollingLaptopReviewRequiredDepartmentRule("rolling laptop review needs this field"),
            });
    }
}
