namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling booking review must pass. Check returns its problems, or none.</summary>
public interface IRollingBookingReviewRule
{
    List<string> Check(RollingBookingReview item);
}

public sealed record RollingBookingReviewChannelLengthRule(int MaxLength) : IRollingBookingReviewRule
{
    public List<string> Check(RollingBookingReview item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBookingReviewRequiredChannelRule(string Message) : IRollingBookingReviewRule
{
    public List<string> Check(RollingBookingReview item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingBookingReviewAllowedNameRule(List<string> Allowed) : IRollingBookingReviewRule
{
    public List<string> Check(RollingBookingReview item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is stale" };
        }
        return new List<string>();
    }
}

public sealed record RollingBookingReviewNameLengthRule(int MaxLength) : IRollingBookingReviewRule
{
    public List<string> Check(RollingBookingReview item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingBookingReviewRequiredCodeRule(string Message) : IRollingBookingReviewRule
{
    public List<string> Check(RollingBookingReview item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling booking review.</summary>
public sealed record RollingBookingReviewValidator(List<IRollingBookingReviewRule> Rules)
{
    public List<string> Validate(RollingBookingReview item)
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

    public void RequireValid(RollingBookingReview item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingBookingReview> items)
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

/// <summary>Functions of rolling booking review rules.</summary>
public static class RollingBookingReviewRules
{
    public static RollingBookingReviewValidator DefaultRollingBookingReviewValidator()
    {
        return new RollingBookingReviewValidator(
            new List<IRollingBookingReviewRule>
            {
                new RollingBookingReviewChannelLengthRule(32),
                new RollingBookingReviewRequiredChannelRule("rolling booking review needs an entry"),
                new RollingBookingReviewAllowedNameRule(new List<string> { "fallback", "strict" }),
                new RollingBookingReviewNameLengthRule(64),
                new RollingBookingReviewRequiredCodeRule("rolling booking review needs this field"),
            });
    }
}
