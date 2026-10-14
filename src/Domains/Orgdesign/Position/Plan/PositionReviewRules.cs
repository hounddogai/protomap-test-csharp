namespace Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>A rule a position review must pass. Check returns its problems, or none.</summary>
public interface IPositionReviewRule
{
    List<string> Check(PositionReview item);
}

public sealed record PositionReviewCodeLengthRule(int MaxLength) : IPositionReviewRule
{
    public List<string> Check(PositionReview item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionReviewChannelLengthRule(int MaxLength) : IPositionReviewRule
{
    public List<string> Check(PositionReview item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionReviewDiscountRangeRule(double Min, double Max) : IPositionReviewRule
{
    public List<string> Check(PositionReview item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionReviewRetriesRangeRule(int Min, int Max) : IPositionReviewRule
{
    public List<string> Check(PositionReview item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PositionReviewContactEmailLengthRule(int MaxLength) : IPositionReviewRule
{
    public List<string> Check(PositionReview item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionReviewRequiredContactEmailRule(string Message) : IPositionReviewRule
{
    public List<string> Check(PositionReview item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position review.</summary>
public sealed record PositionReviewValidator(List<IPositionReviewRule> Rules)
{
    public List<string> Validate(PositionReview item)
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

    public void RequireValid(PositionReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionReview> items)
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

/// <summary>Functions of position review rules.</summary>
public static class PositionReviewRules
{
    public static PositionReviewValidator DefaultPositionReviewValidator()
    {
        return new PositionReviewValidator(
            new List<IPositionReviewRule>
            {
                new PositionReviewCodeLengthRule(120),
                new PositionReviewChannelLengthRule(86400),
                new PositionReviewDiscountRangeRule(2.4, 3519.1),
                new PositionReviewRetriesRangeRule(16, 16),
                new PositionReviewContactEmailLengthRule(86400),
                new PositionReviewRequiredContactEmailRule("position review needs this field"),
            });
    }
}
