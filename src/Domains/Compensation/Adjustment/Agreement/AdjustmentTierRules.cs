namespace Acme.Workforce.Compensation.Adjustment.Agreement;

/// <summary>A rule a adjustment tier must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentTierRule
{
    List<string> Check(AdjustmentTier item);
}

public sealed record AdjustmentTierLocaleLengthRule(int MaxLength) : IAdjustmentTierRule
{
    public List<string> Check(AdjustmentTier item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTierJobCodeLengthRule(int MaxLength) : IAdjustmentTierRule
{
    public List<string> Check(AdjustmentTier item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTierActiveGuardRule(int Limit) : IAdjustmentTierRule
{
    public List<string> Check(AdjustmentTier item)
    {
        if (item.Active && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTierRequiredTitleRule(string Message) : IAdjustmentTierRule
{
    public List<string> Check(AdjustmentTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTierWindowDaysRangeRule(int Min, int Max) : IAdjustmentTierRule
{
    public List<string> Check(AdjustmentTier item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentTierRequiredJobCodeRule(string Message) : IAdjustmentTierRule
{
    public List<string> Check(AdjustmentTier item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment tier.</summary>
public sealed record AdjustmentTierValidator(List<IAdjustmentTierRule> Rules)
{
    public List<string> Validate(AdjustmentTier item)
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

    public void RequireValid(AdjustmentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentTier> items)
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

/// <summary>Functions of adjustment tier rules.</summary>
public static class AdjustmentTierRules
{
    public static AdjustmentTierValidator DefaultAdjustmentTierValidator()
    {
        return new AdjustmentTierValidator(
            new List<IAdjustmentTierRule>
            {
                new AdjustmentTierLocaleLengthRule(1000),
                new AdjustmentTierJobCodeLengthRule(120),
                new AdjustmentTierActiveGuardRule(64),
                new AdjustmentTierRequiredTitleRule("adjustment tier needs a value"),
                new AdjustmentTierWindowDaysRangeRule(16, 32),
                new AdjustmentTierRequiredJobCodeRule("adjustment tier needs an entry"),
            });
    }
}
