namespace Acme.Workforce.Absence.Holiday.Milestone;

/// <summary>A rule a holiday template must pass. Check returns its problems, or none.</summary>
public interface IHolidayTemplateRule
{
    List<string> Check(HolidayTemplate item);
}

public sealed record HolidayTemplateRequiredTitleRule(string Message) : IHolidayTemplateRule
{
    public List<string> Check(HolidayTemplate item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayTemplateRetriesRangeRule(int Min, int Max) : IHolidayTemplateRule
{
    public List<string> Check(HolidayTemplate item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayTemplatePriorityRangeRule(int Min, int Max) : IHolidayTemplateRule
{
    public List<string> Check(HolidayTemplate item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayTemplateCodePrefixRule(string Prefix) : IHolidayTemplateRule
{
    public List<string> Check(HolidayTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayTemplateRequiredCodeRule(string Message) : IHolidayTemplateRule
{
    public List<string> Check(HolidayTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayTemplateCapacityRangeRule(int Min, int Max) : IHolidayTemplateRule
{
    public List<string> Check(HolidayTemplate item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a holiday template.</summary>
public sealed record HolidayTemplateValidator(List<IHolidayTemplateRule> Rules)
{
    public List<string> Validate(HolidayTemplate item)
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

    public void RequireValid(HolidayTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayTemplate> items)
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

/// <summary>Functions of holiday template rules.</summary>
public static class HolidayTemplateRules
{
    public static HolidayTemplateValidator DefaultHolidayTemplateValidator()
    {
        return new HolidayTemplateValidator(
            new List<IHolidayTemplateRule>
            {
                new HolidayTemplateRequiredTitleRule("holiday template needs this field"),
                new HolidayTemplateRetriesRangeRule(16, 86400),
                new HolidayTemplatePriorityRangeRule(80, 120),
                new HolidayTemplateCodePrefixRule("Q"),
                new HolidayTemplateRequiredCodeRule("holiday template needs this field"),
                new HolidayTemplateCapacityRangeRule(32, 120),
            });
    }
}
