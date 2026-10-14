namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled booking template must pass. Check returns its problems, or none.</summary>
public interface IScheduledBookingTemplateRule
{
    List<string> Check(ScheduledBookingTemplate item);
}

public sealed record ScheduledBookingTemplateQuantityRangeRule(int Min, int Max) : IScheduledBookingTemplateRule
{
    public List<string> Check(ScheduledBookingTemplate item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledBookingTemplateAllowedCodeRule(List<string> Allowed) : IScheduledBookingTemplateRule
{
    public List<string> Check(ScheduledBookingTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingTemplateArchivedGuardRule(int Limit) : IScheduledBookingTemplateRule
{
    public List<string> Check(ScheduledBookingTemplate item)
    {
        if (item.Archived && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingTemplateCodeLengthRule(int MaxLength) : IScheduledBookingTemplateRule
{
    public List<string> Check(ScheduledBookingTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingTemplateCodePrefixRule(string Prefix) : IScheduledBookingTemplateRule
{
    public List<string> Check(ScheduledBookingTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledBookingTemplateRequiredNameRule(string Message) : IScheduledBookingTemplateRule
{
    public List<string> Check(ScheduledBookingTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled booking template.</summary>
public sealed record ScheduledBookingTemplateValidator(List<IScheduledBookingTemplateRule> Rules)
{
    public List<string> Validate(ScheduledBookingTemplate item)
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

    public void RequireValid(ScheduledBookingTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledBookingTemplate> items)
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

/// <summary>Functions of scheduled booking template rules.</summary>
public static class ScheduledBookingTemplateRules
{
    public static ScheduledBookingTemplateValidator DefaultScheduledBookingTemplateValidator()
    {
        return new ScheduledBookingTemplateValidator(
            new List<IScheduledBookingTemplateRule>
            {
                new ScheduledBookingTemplateQuantityRangeRule(80, 120),
                new ScheduledBookingTemplateAllowedCodeRule(new List<string> { "rolling", "strict" }),
                new ScheduledBookingTemplateArchivedGuardRule(120),
                new ScheduledBookingTemplateCodeLengthRule(64),
                new ScheduledBookingTemplateCodePrefixRule("ACM"),
                new ScheduledBookingTemplateRequiredNameRule("scheduled booking template needs an entry"),
            });
    }
}
