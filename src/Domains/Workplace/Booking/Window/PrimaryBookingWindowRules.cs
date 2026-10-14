namespace Acme.Workforce.Workplace.Booking.Window;

/// <summary>A rule a primary booking window must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBookingWindowRule
{
    List<string> Check(PrimaryBookingWindow item);
}

public sealed record PrimaryBookingWindowCodeLengthRule(int MaxLength) : IPrimaryBookingWindowRule
{
    public List<string> Check(PrimaryBookingWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingWindowSkillLengthRule(int MaxLength) : IPrimaryBookingWindowRule
{
    public List<string> Check(PrimaryBookingWindow item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingWindowRequiredCodeRule(string Message) : IPrimaryBookingWindowRule
{
    public List<string> Check(PrimaryBookingWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBookingWindowRequiredSkillRule(string Message) : IPrimaryBookingWindowRule
{
    public List<string> Check(PrimaryBookingWindow item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary booking window.</summary>
public sealed record PrimaryBookingWindowValidator(List<IPrimaryBookingWindowRule> Rules)
{
    public List<string> Validate(PrimaryBookingWindow item)
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

    public void RequireValid(PrimaryBookingWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBookingWindow> items)
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

/// <summary>Functions of primary booking window rules.</summary>
public static class PrimaryBookingWindowRules
{
    public static PrimaryBookingWindowValidator DefaultPrimaryBookingWindowValidator()
    {
        return new PrimaryBookingWindowValidator(
            new List<IPrimaryBookingWindowRule>
            {
                new PrimaryBookingWindowCodeLengthRule(16),
                new PrimaryBookingWindowSkillLengthRule(80),
                new PrimaryBookingWindowRequiredCodeRule("primary booking window needs this field"),
                new PrimaryBookingWindowRequiredSkillRule("primary booking window needs a value"),
            });
    }
}
