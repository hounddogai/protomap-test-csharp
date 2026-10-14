namespace Acme.Workforce.Compliance.Exception.Rule;

/// <summary>A rule a exception segment must pass. Check returns its problems, or none.</summary>
public interface IExceptionSegmentRule
{
    List<string> Check(ExceptionSegment item);
}

public sealed record ExceptionSegmentTitleLengthRule(int MaxLength) : IExceptionSegmentRule
{
    public List<string> Check(ExceptionSegment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSegmentRequiredSourceRule(string Message) : IExceptionSegmentRule
{
    public List<string> Check(ExceptionSegment item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSegmentRequiredTitleRule(string Message) : IExceptionSegmentRule
{
    public List<string> Check(ExceptionSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSegmentCodeLengthRule(int MaxLength) : IExceptionSegmentRule
{
    public List<string> Check(ExceptionSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSegmentCodePrefixRule(string Prefix) : IExceptionSegmentRule
{
    public List<string> Check(ExceptionSegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception segment.</summary>
public sealed record ExceptionSegmentValidator(List<IExceptionSegmentRule> Rules)
{
    public List<string> Validate(ExceptionSegment item)
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

    public void RequireValid(ExceptionSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionSegment> items)
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

/// <summary>Functions of exception segment rules.</summary>
public static class ExceptionSegmentRules
{
    public static ExceptionSegmentValidator DefaultExceptionSegmentValidator()
    {
        return new ExceptionSegmentValidator(
            new List<IExceptionSegmentRule>
            {
                new ExceptionSegmentTitleLengthRule(32),
                new ExceptionSegmentRequiredSourceRule("exception segment needs this field"),
                new ExceptionSegmentRequiredTitleRule("exception segment needs a value"),
                new ExceptionSegmentCodeLengthRule(16),
                new ExceptionSegmentCodePrefixRule("X"),
            });
    }
}
