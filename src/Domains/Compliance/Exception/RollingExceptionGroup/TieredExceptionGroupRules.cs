namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>A rule a tiered exception group must pass. Check returns its problems, or none.</summary>
public interface ITieredExceptionGroupRule
{
    List<string> Check(TieredExceptionGroup item);
}

public sealed record TieredExceptionGroupRequiredCodeRule(string Message) : ITieredExceptionGroupRule
{
    public List<string> Check(TieredExceptionGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredExceptionGroupDurationMinutesRangeRule(int Min, int Max) : ITieredExceptionGroupRule
{
    public List<string> Check(TieredExceptionGroup item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredExceptionGroupLabelLengthRule(int MaxLength) : ITieredExceptionGroupRule
{
    public List<string> Check(TieredExceptionGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredExceptionGroupCodeLengthRule(int MaxLength) : ITieredExceptionGroupRule
{
    public List<string> Check(TieredExceptionGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered exception group.</summary>
public sealed record TieredExceptionGroupValidator(List<ITieredExceptionGroupRule> Rules)
{
    public List<string> Validate(TieredExceptionGroup item)
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

    public void RequireValid(TieredExceptionGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredExceptionGroup> items)
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

/// <summary>Functions of tiered exception group rules.</summary>
public static class TieredExceptionGroupRules
{
    public static TieredExceptionGroupValidator DefaultTieredExceptionGroupValidator()
    {
        return new TieredExceptionGroupValidator(
            new List<ITieredExceptionGroupRule>
            {
                new TieredExceptionGroupRequiredCodeRule("tiered exception group needs an entry"),
                new TieredExceptionGroupDurationMinutesRangeRule(16, 16),
                new TieredExceptionGroupLabelLengthRule(86400),
                new TieredExceptionGroupCodeLengthRule(3600),
            });
    }
}
