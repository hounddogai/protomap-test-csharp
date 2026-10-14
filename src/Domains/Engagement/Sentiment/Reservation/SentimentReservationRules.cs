namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a sentiment reservation must pass. Check returns its problems, or none.</summary>
public interface ISentimentReservationRule
{
    List<string> Check(SentimentReservation item);
}

public sealed record SentimentReservationAllowedReviewerRule(List<string> Allowed) : ISentimentReservationRule
{
    public List<string> Check(SentimentReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is stale" };
        }
        return new List<string>();
    }
}

public sealed record SentimentReservationRequiredNameRule(string Message) : ISentimentReservationRule
{
    public List<string> Check(SentimentReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SentimentReservationDiscountRangeRule(double Min, double Max) : ISentimentReservationRule
{
    public List<string> Check(SentimentReservation item)
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

/// <summary>Runs every rule of a sentiment reservation.</summary>
public sealed record SentimentReservationValidator(List<ISentimentReservationRule> Rules)
{
    public List<string> Validate(SentimentReservation item)
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

    public void RequireValid(SentimentReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SentimentReservation> items)
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

/// <summary>Functions of sentiment reservation rules.</summary>
public static class SentimentReservationRules
{
    public static SentimentReservationValidator DefaultSentimentReservationValidator()
    {
        return new SentimentReservationValidator(
            new List<ISentimentReservationRule>
            {
                new SentimentReservationAllowedReviewerRule(new List<string>()),
                new SentimentReservationRequiredNameRule("sentiment reservation needs an entry"),
                new SentimentReservationDiscountRangeRule(8.9, 4229.4),
            });
    }
}
