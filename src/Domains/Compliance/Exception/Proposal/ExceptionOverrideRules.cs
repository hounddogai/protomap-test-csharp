namespace Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>A rule a exception override must pass. Check returns its problems, or none.</summary>
public interface IExceptionOverrideRule
{
    List<string> Check(ExceptionOverride item);
}

public sealed record ExceptionOverrideRequiredCodeRule(string Message) : IExceptionOverrideRule
{
    public List<string> Check(ExceptionOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionOverridePinnedGuardRule(int Limit) : IExceptionOverrideRule
{
    public List<string> Check(ExceptionOverride item)
    {
        if (item.Pinned && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionOverridePhoneLengthRule(int MaxLength) : IExceptionOverrideRule
{
    public List<string> Check(ExceptionOverride item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception override.</summary>
public sealed record ExceptionOverrideValidator(List<IExceptionOverrideRule> Rules)
{
    public List<string> Validate(ExceptionOverride item)
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

    public void RequireValid(ExceptionOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionOverride> items)
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

/// <summary>Functions of exception override rules.</summary>
public static class ExceptionOverrideRules
{
    public static ExceptionOverrideValidator DefaultExceptionOverrideValidator()
    {
        return new ExceptionOverrideValidator(
            new List<IExceptionOverrideRule>
            {
                new ExceptionOverrideRequiredCodeRule("exception override needs an entry"),
                new ExceptionOverridePinnedGuardRule(86400),
                new ExceptionOverridePhoneLengthRule(80),
            });
    }
}
