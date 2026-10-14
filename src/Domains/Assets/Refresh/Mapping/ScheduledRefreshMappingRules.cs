namespace Acme.Workforce.Assets.Refresh.Mapping;

/// <summary>A rule a scheduled refresh mapping must pass. Check returns its problems, or none.</summary>
public interface IScheduledRefreshMappingRule
{
    List<string> Check(ScheduledRefreshMapping item);
}

public sealed record ScheduledRefreshMappingDiscountRangeRule(double Min, double Max) : IScheduledRefreshMappingRule
{
    public List<string> Check(ScheduledRefreshMapping item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledRefreshMappingRateRangeRule(double Min, double Max) : IScheduledRefreshMappingRule
{
    public List<string> Check(ScheduledRefreshMapping item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledRefreshMappingRequiredCodeRule(string Message) : IScheduledRefreshMappingRule
{
    public List<string> Check(ScheduledRefreshMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRefreshMappingLabelLengthRule(int MaxLength) : IScheduledRefreshMappingRule
{
    public List<string> Check(ScheduledRefreshMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledRefreshMappingMinimumRangeRule(double Min, double Max) : IScheduledRefreshMappingRule
{
    public List<string> Check(ScheduledRefreshMapping item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled refresh mapping.</summary>
public sealed record ScheduledRefreshMappingValidator(List<IScheduledRefreshMappingRule> Rules)
{
    public List<string> Validate(ScheduledRefreshMapping item)
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

    public void RequireValid(ScheduledRefreshMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledRefreshMapping> items)
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

/// <summary>Functions of scheduled refresh mapping rules.</summary>
public static class ScheduledRefreshMappingRules
{
    public static ScheduledRefreshMappingValidator DefaultScheduledRefreshMappingValidator()
    {
        return new ScheduledRefreshMappingValidator(
            new List<IScheduledRefreshMappingRule>
            {
                new ScheduledRefreshMappingDiscountRangeRule(6.4, 3529.8),
                new ScheduledRefreshMappingRateRangeRule(3.2, 3181.7),
                new ScheduledRefreshMappingRequiredCodeRule("scheduled refresh mapping needs an entry"),
                new ScheduledRefreshMappingLabelLengthRule(64),
                new ScheduledRefreshMappingMinimumRangeRule(3.3, 4774.8),
            });
    }
}
