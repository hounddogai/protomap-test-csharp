namespace Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>A rule a primary feedback notice must pass. Check returns its problems, or none.</summary>
public interface IPrimaryFeedbackNoticeRule
{
    List<string> Check(PrimaryFeedbackNotice item);
}

public sealed record PrimaryFeedbackNoticeCodePrefixRule(string Prefix) : IPrimaryFeedbackNoticeRule
{
    public List<string> Check(PrimaryFeedbackNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFeedbackNoticeTitleLengthRule(int MaxLength) : IPrimaryFeedbackNoticeRule
{
    public List<string> Check(PrimaryFeedbackNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFeedbackNoticeFloorValueRangeRule(double Min, double Max) : IPrimaryFeedbackNoticeRule
{
    public List<string> Check(PrimaryFeedbackNotice item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary feedback notice.</summary>
public sealed record PrimaryFeedbackNoticeValidator(List<IPrimaryFeedbackNoticeRule> Rules)
{
    public List<string> Validate(PrimaryFeedbackNotice item)
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

    public void RequireValid(PrimaryFeedbackNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryFeedbackNotice> items)
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

/// <summary>Functions of primary feedback notice rules.</summary>
public static class PrimaryFeedbackNoticeRules
{
    public static PrimaryFeedbackNoticeValidator DefaultPrimaryFeedbackNoticeValidator()
    {
        return new PrimaryFeedbackNoticeValidator(
            new List<IPrimaryFeedbackNoticeRule>
            {
                new PrimaryFeedbackNoticeCodePrefixRule("X"),
                new PrimaryFeedbackNoticeTitleLengthRule(32),
                new PrimaryFeedbackNoticeFloorValueRangeRule(8.2, 2942.3),
            });
    }
}
