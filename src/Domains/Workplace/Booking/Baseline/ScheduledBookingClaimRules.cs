namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled booking claim must pass. Check returns its problems, or none.</summary>
public interface IScheduledBookingClaimRule
{
    List<string> Check(ScheduledBookingClaim item);
}

public sealed record ScheduledBookingClaimAttemptsRangeRule(int Min, int Max) : IScheduledBookingClaimRule
{
    public List<string> Check(ScheduledBookingClaim item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingClaimTitleLengthRule(int MaxLength) : IScheduledBookingClaimRule
{
    public List<string> Check(ScheduledBookingClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingClaimRequiredCategoryRule(string Message) : IScheduledBookingClaimRule
{
    public List<string> Check(ScheduledBookingClaim item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingClaimRequiredTitleRule(string Message) : IScheduledBookingClaimRule
{
    public List<string> Check(ScheduledBookingClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingClaimAllowedCategoryRule(List<string> Allowed) : IScheduledBookingClaimRule
{
    public List<string> Check(ScheduledBookingClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled booking claim.</summary>
public sealed record ScheduledBookingClaimValidator(List<IScheduledBookingClaimRule> Rules)
{
    public List<string> Validate(ScheduledBookingClaim item)
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

    public void RequireValid(ScheduledBookingClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBookingClaim> items)
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

/// <summary>Functions of scheduled booking claim rules.</summary>
public static class ScheduledBookingClaimRules
{
    public static ScheduledBookingClaimValidator DefaultScheduledBookingClaimValidator()
    {
        return new ScheduledBookingClaimValidator(
            new List<IScheduledBookingClaimRule>
            {
                new ScheduledBookingClaimAttemptsRangeRule(255, 3600),
                new ScheduledBookingClaimTitleLengthRule(255),
                new ScheduledBookingClaimRequiredCategoryRule("scheduled booking claim needs this field"),
                new ScheduledBookingClaimRequiredTitleRule("scheduled booking claim needs a value"),
                new ScheduledBookingClaimAllowedCategoryRule(new List<string>()),
            });
    }
}
