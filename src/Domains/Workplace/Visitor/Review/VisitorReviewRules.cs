namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor review must pass. Check returns its problems, or none.</summary>
public interface IVisitorReviewRule
{
    List<string> Check(VisitorReview item);
}

public sealed record VisitorReviewAllowedCodeRule(List<string> Allowed) : IVisitorReviewRule
{
    public List<string> Check(VisitorReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record VisitorReviewRequiredNameRule(string Message) : IVisitorReviewRule
{
    public List<string> Check(VisitorReview item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorReviewRequiredDescriptionRule(string Message) : IVisitorReviewRule
{
    public List<string> Check(VisitorReview item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorReviewDescriptionLengthRule(int MaxLength) : IVisitorReviewRule
{
    public List<string> Check(VisitorReview item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorReviewCodePrefixRule(string Prefix) : IVisitorReviewRule
{
    public List<string> Check(VisitorReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor review.</summary>
public sealed record VisitorReviewValidator(List<IVisitorReviewRule> Rules)
{
    public List<string> Validate(VisitorReview item)
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

    public void RequireValid(VisitorReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorReview> items)
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

/// <summary>Functions of visitor review rules.</summary>
public static class VisitorReviewRules
{
    public static VisitorReviewValidator DefaultVisitorReviewValidator()
    {
        return new VisitorReviewValidator(
            new List<IVisitorReviewRule>
            {
                new VisitorReviewAllowedCodeRule(new List<string> { "pending", "rolling" }),
                new VisitorReviewRequiredNameRule("visitor review needs an entry"),
                new VisitorReviewRequiredDescriptionRule("visitor review needs an entry"),
                new VisitorReviewDescriptionLengthRule(3600),
                new VisitorReviewCodePrefixRule("REF"),
            });
    }
}
