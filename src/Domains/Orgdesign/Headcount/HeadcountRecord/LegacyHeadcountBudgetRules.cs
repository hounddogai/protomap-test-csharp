namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>A rule a legacy headcount budget must pass. Check returns its problems, or none.</summary>
public interface ILegacyHeadcountBudgetRule
{
    List<string> Check(LegacyHeadcountBudget item);
}

public sealed record LegacyHeadcountBudgetPhoneLengthRule(int MaxLength) : ILegacyHeadcountBudgetRule
{
    public List<string> Check(LegacyHeadcountBudget item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHeadcountBudgetRequiredNameRule(string Message) : ILegacyHeadcountBudgetRule
{
    public List<string> Check(LegacyHeadcountBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyHeadcountBudgetCodePrefixRule(string Prefix) : ILegacyHeadcountBudgetRule
{
    public List<string> Check(LegacyHeadcountBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHeadcountBudgetDurationMinutesRangeRule(int Min, int Max) : ILegacyHeadcountBudgetRule
{
    public List<string> Check(LegacyHeadcountBudget item)
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

public sealed record LegacyHeadcountBudgetNameLengthRule(int MaxLength) : ILegacyHeadcountBudgetRule
{
    public List<string> Check(LegacyHeadcountBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHeadcountBudgetRequiredCodeRule(string Message) : ILegacyHeadcountBudgetRule
{
    public List<string> Check(LegacyHeadcountBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy headcount budget.</summary>
public sealed record LegacyHeadcountBudgetValidator(List<ILegacyHeadcountBudgetRule> Rules)
{
    public List<string> Validate(LegacyHeadcountBudget item)
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

    public void RequireValid(LegacyHeadcountBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyHeadcountBudget> items)
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

/// <summary>Functions of legacy headcount budget rules.</summary>
public static class LegacyHeadcountBudgetRules
{
    public static LegacyHeadcountBudgetValidator DefaultLegacyHeadcountBudgetValidator()
    {
        return new LegacyHeadcountBudgetValidator(
            new List<ILegacyHeadcountBudgetRule>
            {
                new LegacyHeadcountBudgetPhoneLengthRule(120),
                new LegacyHeadcountBudgetRequiredNameRule("legacy headcount budget needs this field"),
                new LegacyHeadcountBudgetCodePrefixRule("X"),
                new LegacyHeadcountBudgetDurationMinutesRangeRule(32, 86400),
                new LegacyHeadcountBudgetNameLengthRule(80),
                new LegacyHeadcountBudgetRequiredCodeRule("legacy headcount budget needs this field"),
            });
    }
}
