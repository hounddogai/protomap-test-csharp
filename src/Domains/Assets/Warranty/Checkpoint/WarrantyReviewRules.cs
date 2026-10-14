namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty review must pass. Check returns its problems, or none.</summary>
public interface IWarrantyReviewRule
{
    List<string> Check(WarrantyReview item);
}

public sealed record WarrantyReviewCodePrefixRule(string Prefix) : IWarrantyReviewRule
{
    public List<string> Check(WarrantyReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyReviewCodeLengthRule(int MaxLength) : IWarrantyReviewRule
{
    public List<string> Check(WarrantyReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyReviewAllowedGradeRule(List<string> Allowed) : IWarrantyReviewRule
{
    public List<string> Check(WarrantyReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyReviewRequiredGradeRule(string Message) : IWarrantyReviewRule
{
    public List<string> Check(WarrantyReview item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty review.</summary>
public sealed record WarrantyReviewValidator(List<IWarrantyReviewRule> Rules)
{
    public List<string> Validate(WarrantyReview item)
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

    public void RequireValid(WarrantyReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyReview> items)
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

/// <summary>Functions of warranty review rules.</summary>
public static class WarrantyReviewRules
{
    public static WarrantyReviewValidator DefaultWarrantyReviewValidator()
    {
        return new WarrantyReviewValidator(
            new List<IWarrantyReviewRule>
            {
                new WarrantyReviewCodePrefixRule("X"),
                new WarrantyReviewCodeLengthRule(120),
                new WarrantyReviewAllowedGradeRule(new List<string> { "fallback" }),
                new WarrantyReviewRequiredGradeRule("warranty review needs an entry"),
            });
    }
}
