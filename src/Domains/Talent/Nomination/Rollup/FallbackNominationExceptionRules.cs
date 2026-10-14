namespace Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>A rule a fallback nomination exception must pass. Check returns its problems, or none.</summary>
public interface IFallbackNominationExceptionRule
{
    List<string> Check(FallbackNominationException item);
}

public sealed record FallbackNominationExceptionNameLengthRule(int MaxLength) : IFallbackNominationExceptionRule
{
    public List<string> Check(FallbackNominationException item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationExceptionUpdatedAtRangeRule(int Min, int Max) : IFallbackNominationExceptionRule
{
    public List<string> Check(FallbackNominationException item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationExceptionRequiredChannelRule(string Message) : IFallbackNominationExceptionRule
{
    public List<string> Check(FallbackNominationException item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackNominationExceptionBadgeLengthRule(int MaxLength) : IFallbackNominationExceptionRule
{
    public List<string> Check(FallbackNominationException item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback nomination exception.</summary>
public sealed record FallbackNominationExceptionValidator(List<IFallbackNominationExceptionRule> Rules)
{
    public List<string> Validate(FallbackNominationException item)
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

    public void RequireValid(FallbackNominationException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackNominationException> items)
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

/// <summary>Functions of fallback nomination exception rules.</summary>
public static class FallbackNominationExceptionRules
{
    public static FallbackNominationExceptionValidator DefaultFallbackNominationExceptionValidator()
    {
        return new FallbackNominationExceptionValidator(
            new List<IFallbackNominationExceptionRule>
            {
                new FallbackNominationExceptionNameLengthRule(120),
                new FallbackNominationExceptionUpdatedAtRangeRule(120, 3600),
                new FallbackNominationExceptionRequiredChannelRule("fallback nomination exception needs an entry"),
                new FallbackNominationExceptionBadgeLengthRule(120),
            });
    }
}
