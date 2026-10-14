namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary booking tier must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBookingTierRule
{
    List<string> Check(PrimaryBookingTier item);
}

public sealed record PrimaryBookingTierAllowedTitleRule(List<string> Allowed) : IPrimaryBookingTierRule
{
    public List<string> Check(PrimaryBookingTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingTierGradeLengthRule(int MaxLength) : IPrimaryBookingTierRule
{
    public List<string> Check(PrimaryBookingTier item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingTierRequiredExternalRefRule(string Message) : IPrimaryBookingTierRule
{
    public List<string> Check(PrimaryBookingTier item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingTierCodeLengthRule(int MaxLength) : IPrimaryBookingTierRule
{
    public List<string> Check(PrimaryBookingTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary booking tier.</summary>
public sealed record PrimaryBookingTierValidator(List<IPrimaryBookingTierRule> Rules)
{
    public List<string> Validate(PrimaryBookingTier item)
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

    public void RequireValid(PrimaryBookingTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBookingTier> items)
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

/// <summary>Functions of primary booking tier rules.</summary>
public static class PrimaryBookingTierRules
{
    public static PrimaryBookingTierValidator DefaultPrimaryBookingTierValidator()
    {
        return new PrimaryBookingTierValidator(
            new List<IPrimaryBookingTierRule>
            {
                new PrimaryBookingTierAllowedTitleRule(new List<string> { "manual" }),
                new PrimaryBookingTierGradeLengthRule(1000),
                new PrimaryBookingTierRequiredExternalRefRule("primary booking tier needs this field"),
                new PrimaryBookingTierCodeLengthRule(1000),
            });
    }
}
