namespace Acme.Workforce.Orgdesign.Headcount.Assignment;

/// <summary>A rule a manual headcount tier must pass. Check returns its problems, or none.</summary>
public interface IManualHeadcountTierRule
{
    List<string> Check(ManualHeadcountTier item);
}

public sealed record ManualHeadcountTierUpdatedAtRangeRule(int Min, int Max) : IManualHeadcountTierRule
{
    public List<string> Check(ManualHeadcountTier item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualHeadcountTierRequiredEmployeeIdRule(string Message) : IManualHeadcountTierRule
{
    public List<string> Check(ManualHeadcountTier item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualHeadcountTierGraceDaysRangeRule(int Min, int Max) : IManualHeadcountTierRule
{
    public List<string> Check(ManualHeadcountTier item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualHeadcountTierRequiredCodeRule(string Message) : IManualHeadcountTierRule
{
    public List<string> Check(ManualHeadcountTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualHeadcountTierRequiredLocationRule(string Message) : IManualHeadcountTierRule
{
    public List<string> Check(ManualHeadcountTier item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualHeadcountTierCodePrefixRule(string Prefix) : IManualHeadcountTierRule
{
    public List<string> Check(ManualHeadcountTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual headcount tier.</summary>
public sealed record ManualHeadcountTierValidator(List<IManualHeadcountTierRule> Rules)
{
    public List<string> Validate(ManualHeadcountTier item)
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

    public void RequireValid(ManualHeadcountTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualHeadcountTier> items)
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

/// <summary>Functions of manual headcount tier rules.</summary>
public static class ManualHeadcountTierRules
{
    public static ManualHeadcountTierValidator DefaultManualHeadcountTierValidator()
    {
        return new ManualHeadcountTierValidator(
            new List<IManualHeadcountTierRule>
            {
                new ManualHeadcountTierUpdatedAtRangeRule(16, 86400),
                new ManualHeadcountTierRequiredEmployeeIdRule("manual headcount tier needs this field"),
                new ManualHeadcountTierGraceDaysRangeRule(1000, 3600),
                new ManualHeadcountTierRequiredCodeRule("manual headcount tier needs this field"),
                new ManualHeadcountTierRequiredLocationRule("manual headcount tier needs a value"),
                new ManualHeadcountTierCodePrefixRule("OPS"),
            });
    }
}
