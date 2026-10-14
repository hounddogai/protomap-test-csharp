namespace Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>A rule a span review must pass. Check returns its problems, or none.</summary>
public interface ISpanReviewRule
{
    List<string> Check(SpanReview item);
}

public sealed record SpanReviewRequiredContactEmailRule(string Message) : ISpanReviewRule
{
    public List<string> Check(SpanReview item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanReviewRequiredNameRule(string Message) : ISpanReviewRule
{
    public List<string> Check(SpanReview item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanReviewNameLengthRule(int MaxLength) : ISpanReviewRule
{
    public List<string> Check(SpanReview item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanReviewContactEmailLengthRule(int MaxLength) : ISpanReviewRule
{
    public List<string> Check(SpanReview item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanReviewRequiredRegionRule(string Message) : ISpanReviewRule
{
    public List<string> Check(SpanReview item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a span review.</summary>
public sealed record SpanReviewValidator(List<ISpanReviewRule> Rules)
{
    public List<string> Validate(SpanReview item)
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

    public void RequireValid(SpanReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanReview> items)
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

/// <summary>Functions of span review rules.</summary>
public static class SpanReviewRules
{
    public static SpanReviewValidator DefaultSpanReviewValidator()
    {
        return new SpanReviewValidator(
            new List<ISpanReviewRule>
            {
                new SpanReviewRequiredContactEmailRule("span review needs an entry"),
                new SpanReviewRequiredNameRule("span review needs a value"),
                new SpanReviewNameLengthRule(120),
                new SpanReviewContactEmailLengthRule(80),
                new SpanReviewRequiredRegionRule("span review needs an entry"),
            });
    }
}
