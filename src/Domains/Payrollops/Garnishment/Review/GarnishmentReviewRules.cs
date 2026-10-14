namespace Acme.Workforce.Payrollops.Garnishment.Review;

/// <summary>A rule a garnishment review must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentReviewRule
{
    List<string> Check(GarnishmentReview item);
}

public sealed record GarnishmentReviewRequiredCodeRule(string Message) : IGarnishmentReviewRule
{
    public List<string> Check(GarnishmentReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentReviewCostRangeRule(double Min, double Max) : IGarnishmentReviewRule
{
    public List<string> Check(GarnishmentReview item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentReviewCodePrefixRule(string Prefix) : IGarnishmentReviewRule
{
    public List<string> Check(GarnishmentReview item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentReviewRequiredManagerIdRule(string Message) : IGarnishmentReviewRule
{
    public List<string> Check(GarnishmentReview item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentReviewRequiredChannelRule(string Message) : IGarnishmentReviewRule
{
    public List<string> Check(GarnishmentReview item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentReviewCodeLengthRule(int MaxLength) : IGarnishmentReviewRule
{
    public List<string> Check(GarnishmentReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment review.</summary>
public sealed record GarnishmentReviewValidator(List<IGarnishmentReviewRule> Rules)
{
    public List<string> Validate(GarnishmentReview item)
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

    public void RequireValid(GarnishmentReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentReview> items)
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

/// <summary>Functions of garnishment review rules.</summary>
public static class GarnishmentReviewRules
{
    public static GarnishmentReviewValidator DefaultGarnishmentReviewValidator()
    {
        return new GarnishmentReviewValidator(
            new List<IGarnishmentReviewRule>
            {
                new GarnishmentReviewRequiredCodeRule("garnishment review needs an entry"),
                new GarnishmentReviewCostRangeRule(8.9, 1560.4),
                new GarnishmentReviewCodePrefixRule("REF"),
                new GarnishmentReviewRequiredManagerIdRule("garnishment review needs this field"),
                new GarnishmentReviewRequiredChannelRule("garnishment review needs this field"),
                new GarnishmentReviewCodeLengthRule(86400),
            });
    }
}
