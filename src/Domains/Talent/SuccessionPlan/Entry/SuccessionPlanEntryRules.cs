namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>A rule a succession plan entry must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanEntryRule
{
    List<string> Check(SuccessionPlanEntry item);
}

public sealed record SuccessionPlanEntryRequiredCodeRule(string Message) : ISuccessionPlanEntryRule
{
    public List<string> Check(SuccessionPlanEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanEntryRequiredTitleRule(string Message) : ISuccessionPlanEntryRule
{
    public List<string> Check(SuccessionPlanEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanEntryTimezoneLengthRule(int MaxLength) : ISuccessionPlanEntryRule
{
    public List<string> Check(SuccessionPlanEntry item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanEntryTitleLengthRule(int MaxLength) : ISuccessionPlanEntryRule
{
    public List<string> Check(SuccessionPlanEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanEntryMarginRangeRule(double Min, double Max) : ISuccessionPlanEntryRule
{
    public List<string> Check(SuccessionPlanEntry item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a succession plan entry.</summary>
public sealed record SuccessionPlanEntryValidator(List<ISuccessionPlanEntryRule> Rules)
{
    public List<string> Validate(SuccessionPlanEntry item)
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

    public void RequireValid(SuccessionPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanEntry> items)
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

/// <summary>Functions of succession plan entry rules.</summary>
public static class SuccessionPlanEntryRules
{
    public static SuccessionPlanEntryValidator DefaultSuccessionPlanEntryValidator()
    {
        return new SuccessionPlanEntryValidator(
            new List<ISuccessionPlanEntryRule>
            {
                new SuccessionPlanEntryRequiredCodeRule("succession plan entry needs this field"),
                new SuccessionPlanEntryRequiredTitleRule("succession plan entry needs this field"),
                new SuccessionPlanEntryTimezoneLengthRule(86400),
                new SuccessionPlanEntryTitleLengthRule(64),
                new SuccessionPlanEntryMarginRangeRule(0.1, 464.7),
            });
    }
}
