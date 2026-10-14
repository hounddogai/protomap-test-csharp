namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global rate review must pass. Check returns its problems, or none.</summary>
public interface IGlobalRateReviewRule
{
    List<string> Check(GlobalRateReview item);
}

public sealed record GlobalRateReviewRequiredCodeRule(string Message) : IGlobalRateReviewRule
{
    public List<string> Check(GlobalRateReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalRateReviewAllowedCodeRule(List<string> Allowed) : IGlobalRateReviewRule
{
    public List<string> Check(GlobalRateReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record GlobalRateReviewLabelLengthRule(int MaxLength) : IGlobalRateReviewRule
{
    public List<string> Check(GlobalRateReview item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalRateReviewCodeLengthRule(int MaxLength) : IGlobalRateReviewRule
{
    public List<string> Check(GlobalRateReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global rate review.</summary>
public sealed record GlobalRateReviewValidator(List<IGlobalRateReviewRule> Rules)
{
    public List<string> Validate(GlobalRateReview item)
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

    public void RequireValid(GlobalRateReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalRateReview> items)
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

/// <summary>Functions of global rate review rules.</summary>
public static class GlobalRateReviewRules
{
    public static GlobalRateReviewValidator DefaultGlobalRateReviewValidator()
    {
        return new GlobalRateReviewValidator(
            new List<IGlobalRateReviewRule>
            {
                new GlobalRateReviewRequiredCodeRule("global rate review needs a value"),
                new GlobalRateReviewAllowedCodeRule(new List<string> { "default", "tiered", "adaptive" }),
                new GlobalRateReviewLabelLengthRule(255),
                new GlobalRateReviewCodeLengthRule(1000),
            });
    }
}
