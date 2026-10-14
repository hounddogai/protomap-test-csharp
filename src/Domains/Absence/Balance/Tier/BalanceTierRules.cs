namespace Acme.Workforce.Absence.Balance.Tier;

/// <summary>A rule a balance tier must pass. Check returns its problems, or none.</summary>
public interface IBalanceTierRule
{
    List<string> Check(BalanceTier item);
}

public sealed record BalanceTierBadgeLengthRule(int MaxLength) : IBalanceTierRule
{
    public List<string> Check(BalanceTier item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceTierLabelLengthRule(int MaxLength) : IBalanceTierRule
{
    public List<string> Check(BalanceTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceTierCodeLengthRule(int MaxLength) : IBalanceTierRule
{
    public List<string> Check(BalanceTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceTierEndsAtRangeRule(int Min, int Max) : IBalanceTierRule
{
    public List<string> Check(BalanceTier item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceTierDurationMinutesRangeRule(int Min, int Max) : IBalanceTierRule
{
    public List<string> Check(BalanceTier item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a balance tier.</summary>
public sealed record BalanceTierValidator(List<IBalanceTierRule> Rules)
{
    public List<string> Validate(BalanceTier item)
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

    public void RequireValid(BalanceTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceTier> items)
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

/// <summary>Functions of balance tier rules.</summary>
public static class BalanceTierRules
{
    public static BalanceTierValidator DefaultBalanceTierValidator()
    {
        return new BalanceTierValidator(
            new List<IBalanceTierRule>
            {
                new BalanceTierBadgeLengthRule(120),
                new BalanceTierLabelLengthRule(120),
                new BalanceTierCodeLengthRule(3600),
                new BalanceTierEndsAtRangeRule(16, 64),
                new BalanceTierDurationMinutesRangeRule(32, 80),
            });
    }
}
