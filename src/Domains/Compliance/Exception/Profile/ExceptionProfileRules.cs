namespace Acme.Workforce.Compliance.Exception.Profile;

/// <summary>A rule a exception profile must pass. Check returns its problems, or none.</summary>
public interface IExceptionProfileRule
{
    List<string> Check(ExceptionProfile item);
}

public sealed record ExceptionProfileRequiredChannelRule(string Message) : IExceptionProfileRule
{
    public List<string> Check(ExceptionProfile item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionProfileNameLengthRule(int MaxLength) : IExceptionProfileRule
{
    public List<string> Check(ExceptionProfile item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionProfileRequiredCodeRule(string Message) : IExceptionProfileRule
{
    public List<string> Check(ExceptionProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception profile.</summary>
public sealed record ExceptionProfileValidator(List<IExceptionProfileRule> Rules)
{
    public List<string> Validate(ExceptionProfile item)
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

    public void RequireValid(ExceptionProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionProfile> items)
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

/// <summary>Functions of exception profile rules.</summary>
public static class ExceptionProfileRules
{
    public static ExceptionProfileValidator DefaultExceptionProfileValidator()
    {
        return new ExceptionProfileValidator(
            new List<IExceptionProfileRule>
            {
                new ExceptionProfileRequiredChannelRule("exception profile needs this field"),
                new ExceptionProfileNameLengthRule(120),
                new ExceptionProfileRequiredCodeRule("exception profile needs this field"),
            });
    }
}
