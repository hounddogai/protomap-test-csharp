namespace Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>A rule a pending garnishment tier must pass. Check returns its problems, or none.</summary>
public interface IPendingGarnishmentTierRule
{
    List<string> Check(PendingGarnishmentTier item);
}

public sealed record PendingGarnishmentTierRequiredTitleRule(string Message) : IPendingGarnishmentTierRule
{
    public List<string> Check(PendingGarnishmentTier item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingGarnishmentTierQuantityRangeRule(int Min, int Max) : IPendingGarnishmentTierRule
{
    public List<string> Check(PendingGarnishmentTier item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingGarnishmentTierCapacityRangeRule(int Min, int Max) : IPendingGarnishmentTierRule
{
    public List<string> Check(PendingGarnishmentTier item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingGarnishmentTierRequiredCodeRule(string Message) : IPendingGarnishmentTierRule
{
    public List<string> Check(PendingGarnishmentTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingGarnishmentTierCodeLengthRule(int MaxLength) : IPendingGarnishmentTierRule
{
    public List<string> Check(PendingGarnishmentTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingGarnishmentTierEndsAtRangeRule(int Min, int Max) : IPendingGarnishmentTierRule
{
    public List<string> Check(PendingGarnishmentTier item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending garnishment tier.</summary>
public sealed record PendingGarnishmentTierValidator(List<IPendingGarnishmentTierRule> Rules)
{
    public List<string> Validate(PendingGarnishmentTier item)
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

    public void RequireValid(PendingGarnishmentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingGarnishmentTier> items)
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

/// <summary>Functions of pending garnishment tier rules.</summary>
public static class PendingGarnishmentTierRules
{
    public static PendingGarnishmentTierValidator DefaultPendingGarnishmentTierValidator()
    {
        return new PendingGarnishmentTierValidator(
            new List<IPendingGarnishmentTierRule>
            {
                new PendingGarnishmentTierRequiredTitleRule("pending garnishment tier needs this field"),
                new PendingGarnishmentTierQuantityRangeRule(64, 3600),
                new PendingGarnishmentTierCapacityRangeRule(32, 86400),
                new PendingGarnishmentTierRequiredCodeRule("pending garnishment tier needs this field"),
                new PendingGarnishmentTierCodeLengthRule(1000),
                new PendingGarnishmentTierEndsAtRangeRule(16, 255),
            });
    }
}
