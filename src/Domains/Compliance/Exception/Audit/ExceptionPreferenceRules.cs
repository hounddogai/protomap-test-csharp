namespace Acme.Workforce.Compliance.Exception.Audit;

/// <summary>A rule a exception preference must pass. Check returns its problems, or none.</summary>
public interface IExceptionPreferenceRule
{
    List<string> Check(ExceptionPreference item);
}

public sealed record ExceptionPreferenceRegionLengthRule(int MaxLength) : IExceptionPreferenceRule
{
    public List<string> Check(ExceptionPreference item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionPreferenceCodePrefixRule(string Prefix) : IExceptionPreferenceRule
{
    public List<string> Check(ExceptionPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionPreferenceRequiredCodeRule(string Message) : IExceptionPreferenceRule
{
    public List<string> Check(ExceptionPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception preference.</summary>
public sealed record ExceptionPreferenceValidator(List<IExceptionPreferenceRule> Rules)
{
    public List<string> Validate(ExceptionPreference item)
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

    public void RequireValid(ExceptionPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionPreference> items)
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

/// <summary>Functions of exception preference rules.</summary>
public static class ExceptionPreferenceRules
{
    public static ExceptionPreferenceValidator DefaultExceptionPreferenceValidator()
    {
        return new ExceptionPreferenceValidator(
            new List<IExceptionPreferenceRule>
            {
                new ExceptionPreferenceRegionLengthRule(16),
                new ExceptionPreferenceCodePrefixRule("Q"),
                new ExceptionPreferenceRequiredCodeRule("exception preference needs a value"),
            });
    }
}
