namespace Acme.Workforce.Talent.Mobility.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility review must pass. Check returns its problems, or none.</summary>
public interface IMobilityReviewRule
{
    List<string> Check(MobilityReview item);
}

public sealed record MobilityReviewRequiredSourceRule(string Message) : IMobilityReviewRule
{
    public List<string> Check(MobilityReview item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityReviewRateRangeRule(double Min, double Max) : IMobilityReviewRule
{
    public List<string> Check(MobilityReview item)
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

public sealed record MobilityReviewAllowedLabelRule(List<string> Allowed) : IMobilityReviewRule
{
    public List<string> Check(MobilityReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record MobilityReviewSourceLengthRule(int MaxLength) : IMobilityReviewRule
{
    public List<string> Check(MobilityReview item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityReviewRequiredCodeRule(string Message) : IMobilityReviewRule
{
    public List<string> Check(MobilityReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility review.</summary>
public sealed record MobilityReviewValidator(List<IMobilityReviewRule> Rules)
{
    public List<string> Validate(MobilityReview item)
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

    public void RequireValid(MobilityReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityReview> items)
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

/// <summary>Functions of mobility review rules.</summary>
public static class MobilityReviewRules
{
    public static MobilityReviewValidator DefaultMobilityReviewValidator()
    {
        return new MobilityReviewValidator(
            new List<IMobilityReviewRule>
            {
                new MobilityReviewRequiredSourceRule("mobility review needs an entry"),
                new MobilityReviewRateRangeRule(3.6, 3449.4),
                new MobilityReviewAllowedLabelRule(new List<string>()),
                new MobilityReviewSourceLengthRule(80),
                new MobilityReviewRequiredCodeRule("mobility review needs an entry"),
            });
    }
}
