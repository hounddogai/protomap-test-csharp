namespace Acme.Workforce.Workplace.Booking.Plan;

/// <summary>A rule a default booking score must pass. Check returns its problems, or none.</summary>
public interface IDefaultBookingScoreRule
{
    List<string> Check(DefaultBookingScore item);
}

public sealed record DefaultBookingScoreCodeLengthRule(int MaxLength) : IDefaultBookingScoreRule
{
    public List<string> Check(DefaultBookingScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBookingScoreLocaleLengthRule(int MaxLength) : IDefaultBookingScoreRule
{
    public List<string> Check(DefaultBookingScore item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBookingScoreCodePrefixRule(string Prefix) : IDefaultBookingScoreRule
{
    public List<string> Check(DefaultBookingScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultBookingScoreRequiredCodeRule(string Message) : IDefaultBookingScoreRule
{
    public List<string> Check(DefaultBookingScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultBookingScoreRequiredNameRule(string Message) : IDefaultBookingScoreRule
{
    public List<string> Check(DefaultBookingScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default booking score.</summary>
public sealed record DefaultBookingScoreValidator(List<IDefaultBookingScoreRule> Rules)
{
    public List<string> Validate(DefaultBookingScore item)
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

    public void RequireValid(DefaultBookingScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultBookingScore> items)
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

/// <summary>Functions of default booking score rules.</summary>
public static class DefaultBookingScoreRules
{
    public static DefaultBookingScoreValidator DefaultDefaultBookingScoreValidator()
    {
        return new DefaultBookingScoreValidator(
            new List<IDefaultBookingScoreRule>
            {
                new DefaultBookingScoreCodeLengthRule(80),
                new DefaultBookingScoreLocaleLengthRule(32),
                new DefaultBookingScoreCodePrefixRule("OPS"),
                new DefaultBookingScoreRequiredCodeRule("default booking score needs an entry"),
                new DefaultBookingScoreRequiredNameRule("default booking score needs an entry"),
            });
    }
}
