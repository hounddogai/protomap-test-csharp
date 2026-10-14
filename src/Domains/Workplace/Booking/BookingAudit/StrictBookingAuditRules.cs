namespace Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>A rule a strict booking audit must pass. Check returns its problems, or none.</summary>
public interface IStrictBookingAuditRule
{
    List<string> Check(StrictBookingAudit item);
}

public sealed record StrictBookingAuditCodeLengthRule(int MaxLength) : IStrictBookingAuditRule
{
    public List<string> Check(StrictBookingAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBookingAuditCodePrefixRule(string Prefix) : IStrictBookingAuditRule
{
    public List<string> Check(StrictBookingAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictBookingAuditRequiredCodeRule(string Message) : IStrictBookingAuditRule
{
    public List<string> Check(StrictBookingAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictBookingAuditAmountRangeRule(double Min, double Max) : IStrictBookingAuditRule
{
    public List<string> Check(StrictBookingAudit item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictBookingAuditRequiredLabelRule(string Message) : IStrictBookingAuditRule
{
    public List<string> Check(StrictBookingAudit item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictBookingAuditManagerIdLengthRule(int MaxLength) : IStrictBookingAuditRule
{
    public List<string> Check(StrictBookingAudit item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict booking audit.</summary>
public sealed record StrictBookingAuditValidator(List<IStrictBookingAuditRule> Rules)
{
    public List<string> Validate(StrictBookingAudit item)
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

    public void RequireValid(StrictBookingAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictBookingAudit> items)
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

/// <summary>Functions of strict booking audit rules.</summary>
public static class StrictBookingAuditRules
{
    public static StrictBookingAuditValidator DefaultStrictBookingAuditValidator()
    {
        return new StrictBookingAuditValidator(
            new List<IStrictBookingAuditRule>
            {
                new StrictBookingAuditCodeLengthRule(120),
                new StrictBookingAuditCodePrefixRule("Q"),
                new StrictBookingAuditRequiredCodeRule("strict booking audit needs a value"),
                new StrictBookingAuditAmountRangeRule(4.4, 2591.2),
                new StrictBookingAuditRequiredLabelRule("strict booking audit needs an entry"),
                new StrictBookingAuditManagerIdLengthRule(64),
            });
    }
}
