namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment review must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentReviewRule
{
    List<string> Check(AdjustmentReview item);
}

public sealed record AdjustmentReviewRequiredCodeRule(string Message) : IAdjustmentReviewRule
{
    public List<string> Check(AdjustmentReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentReviewCountryLengthRule(int MaxLength) : IAdjustmentReviewRule
{
    public List<string> Check(AdjustmentReview item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentReviewArchivedGuardRule(int Limit) : IAdjustmentReviewRule
{
    public List<string> Check(AdjustmentReview item)
    {
        if (item.Archived && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentReviewRequiredCountryRule(string Message) : IAdjustmentReviewRule
{
    public List<string> Check(AdjustmentReview item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentReviewAllowedCountryRule(List<string> Allowed) : IAdjustmentReviewRule
{
    public List<string> Check(AdjustmentReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment review.</summary>
public sealed record AdjustmentReviewValidator(List<IAdjustmentReviewRule> Rules)
{
    public List<string> Validate(AdjustmentReview item)
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

    public void RequireValid(AdjustmentReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentReview> items)
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

/// <summary>Functions of adjustment review rules.</summary>
public static class AdjustmentReviewRules
{
    public static AdjustmentReviewValidator DefaultAdjustmentReviewValidator()
    {
        return new AdjustmentReviewValidator(
            new List<IAdjustmentReviewRule>
            {
                new AdjustmentReviewRequiredCodeRule("adjustment review needs a value"),
                new AdjustmentReviewCountryLengthRule(80),
                new AdjustmentReviewArchivedGuardRule(255),
                new AdjustmentReviewRequiredCountryRule("adjustment review needs an entry"),
                new AdjustmentReviewAllowedCountryRule(new List<string> { "weighted", "primary" }),
            });
    }
}
