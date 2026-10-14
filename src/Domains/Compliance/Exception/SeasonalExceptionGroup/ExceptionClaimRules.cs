namespace Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>A rule a exception claim must pass. Check returns its problems, or none.</summary>
public interface IExceptionClaimRule
{
    List<string> Check(ExceptionClaim item);
}

public sealed record ExceptionClaimRequiredReviewerRule(string Message) : IExceptionClaimRule
{
    public List<string> Check(ExceptionClaim item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionClaimRequiredNameRule(string Message) : IExceptionClaimRule
{
    public List<string> Check(ExceptionClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionClaimRequiredChannelRule(string Message) : IExceptionClaimRule
{
    public List<string> Check(ExceptionClaim item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionClaimCreatedAtRangeRule(int Min, int Max) : IExceptionClaimRule
{
    public List<string> Check(ExceptionClaim item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionClaimNameLengthRule(int MaxLength) : IExceptionClaimRule
{
    public List<string> Check(ExceptionClaim item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionClaimChannelLengthRule(int MaxLength) : IExceptionClaimRule
{
    public List<string> Check(ExceptionClaim item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception claim.</summary>
public sealed record ExceptionClaimValidator(List<IExceptionClaimRule> Rules)
{
    public List<string> Validate(ExceptionClaim item)
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

    public void RequireValid(ExceptionClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionClaim> items)
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

/// <summary>Functions of exception claim rules.</summary>
public static class ExceptionClaimRules
{
    public static ExceptionClaimValidator DefaultExceptionClaimValidator()
    {
        return new ExceptionClaimValidator(
            new List<IExceptionClaimRule>
            {
                new ExceptionClaimRequiredReviewerRule("exception claim needs an entry"),
                new ExceptionClaimRequiredNameRule("exception claim needs an entry"),
                new ExceptionClaimRequiredChannelRule("exception claim needs an entry"),
                new ExceptionClaimCreatedAtRangeRule(64, 64),
                new ExceptionClaimNameLengthRule(64),
                new ExceptionClaimChannelLengthRule(86400),
            });
    }
}
