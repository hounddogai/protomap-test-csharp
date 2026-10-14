namespace Acme.Workforce.Talent.Pool.Exception;

/// <summary>A rule a global pool exception must pass. Check returns its problems, or none.</summary>
public interface IGlobalPoolExceptionRule
{
    List<string> Check(GlobalPoolException item);
}

public sealed record GlobalPoolExceptionEndsAtRangeRule(int Min, int Max) : IGlobalPoolExceptionRule
{
    public List<string> Check(GlobalPoolException item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolExceptionTimezoneLengthRule(int MaxLength) : IGlobalPoolExceptionRule
{
    public List<string> Check(GlobalPoolException item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolExceptionCodePrefixRule(string Prefix) : IGlobalPoolExceptionRule
{
    public List<string> Check(GlobalPoolException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPoolExceptionRequiredCodeRule(string Message) : IGlobalPoolExceptionRule
{
    public List<string> Check(GlobalPoolException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global pool exception.</summary>
public sealed record GlobalPoolExceptionValidator(List<IGlobalPoolExceptionRule> Rules)
{
    public List<string> Validate(GlobalPoolException item)
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

    public void RequireValid(GlobalPoolException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPoolException> items)
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

/// <summary>Functions of global pool exception rules.</summary>
public static class GlobalPoolExceptionRules
{
    public static GlobalPoolExceptionValidator DefaultGlobalPoolExceptionValidator()
    {
        return new GlobalPoolExceptionValidator(
            new List<IGlobalPoolExceptionRule>
            {
                new GlobalPoolExceptionEndsAtRangeRule(255, 1000),
                new GlobalPoolExceptionTimezoneLengthRule(120),
                new GlobalPoolExceptionCodePrefixRule("REF"),
                new GlobalPoolExceptionRequiredCodeRule("global pool exception needs a value"),
            });
    }
}
