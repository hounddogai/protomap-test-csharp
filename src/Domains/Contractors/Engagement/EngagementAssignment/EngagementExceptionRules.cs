namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>A rule a engagement exception must pass. Check returns its problems, or none.</summary>
public interface IEngagementExceptionRule
{
    List<string> Check(EngagementException item);
}

public sealed record EngagementExceptionMaximumRangeRule(double Min, double Max) : IEngagementExceptionRule
{
    public List<string> Check(EngagementException item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementExceptionLocaleLengthRule(int MaxLength) : IEngagementExceptionRule
{
    public List<string> Check(EngagementException item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementExceptionCodeLengthRule(int MaxLength) : IEngagementExceptionRule
{
    public List<string> Check(EngagementException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EngagementExceptionRequiredChecksumRule(string Message) : IEngagementExceptionRule
{
    public List<string> Check(EngagementException item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EngagementExceptionRequiredCodeRule(string Message) : IEngagementExceptionRule
{
    public List<string> Check(EngagementException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a engagement exception.</summary>
public sealed record EngagementExceptionValidator(List<IEngagementExceptionRule> Rules)
{
    public List<string> Validate(EngagementException item)
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

    public void RequireValid(EngagementException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EngagementException> items)
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

/// <summary>Functions of engagement exception rules.</summary>
public static class EngagementExceptionRules
{
    public static EngagementExceptionValidator DefaultEngagementExceptionValidator()
    {
        return new EngagementExceptionValidator(
            new List<IEngagementExceptionRule>
            {
                new EngagementExceptionMaximumRangeRule(6.8, 1293.6),
                new EngagementExceptionLocaleLengthRule(16),
                new EngagementExceptionCodeLengthRule(86400),
                new EngagementExceptionRequiredChecksumRule("engagement exception needs a value"),
                new EngagementExceptionRequiredCodeRule("engagement exception needs this field"),
            });
    }
}
