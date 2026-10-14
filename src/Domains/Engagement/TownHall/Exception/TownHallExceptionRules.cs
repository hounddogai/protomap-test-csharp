namespace Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>A rule a town hall exception must pass. Check returns its problems, or none.</summary>
public interface ITownHallExceptionRule
{
    List<string> Check(TownHallException item);
}

public sealed record TownHallExceptionCodePrefixRule(string Prefix) : ITownHallExceptionRule
{
    public List<string> Check(TownHallException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallExceptionTimezoneLengthRule(int MaxLength) : ITownHallExceptionRule
{
    public List<string> Check(TownHallException item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallExceptionVersionRangeRule(int Min, int Max) : ITownHallExceptionRule
{
    public List<string> Check(TownHallException item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallExceptionRequiredUnitRule(string Message) : ITownHallExceptionRule
{
    public List<string> Check(TownHallException item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallExceptionRetriesRangeRule(int Min, int Max) : ITownHallExceptionRule
{
    public List<string> Check(TownHallException item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallExceptionCodeLengthRule(int MaxLength) : ITownHallExceptionRule
{
    public List<string> Check(TownHallException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall exception.</summary>
public sealed record TownHallExceptionValidator(List<ITownHallExceptionRule> Rules)
{
    public List<string> Validate(TownHallException item)
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

    public void RequireValid(TownHallException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallException> items)
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

/// <summary>Functions of town hall exception rules.</summary>
public static class TownHallExceptionRules
{
    public static TownHallExceptionValidator DefaultTownHallExceptionValidator()
    {
        return new TownHallExceptionValidator(
            new List<ITownHallExceptionRule>
            {
                new TownHallExceptionCodePrefixRule("ACM"),
                new TownHallExceptionTimezoneLengthRule(16),
                new TownHallExceptionVersionRangeRule(64, 86400),
                new TownHallExceptionRequiredUnitRule("town hall exception needs a value"),
                new TownHallExceptionRetriesRangeRule(16, 32),
                new TownHallExceptionCodeLengthRule(64),
            });
    }
}
