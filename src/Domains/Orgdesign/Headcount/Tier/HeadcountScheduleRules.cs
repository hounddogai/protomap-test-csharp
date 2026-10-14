namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount schedule must pass. Check returns its problems, or none.</summary>
public interface IHeadcountScheduleRule
{
    List<string> Check(HeadcountSchedule item);
}

public sealed record HeadcountScheduleUnitLengthRule(int MaxLength) : IHeadcountScheduleRule
{
    public List<string> Check(HeadcountSchedule item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountScheduleRequiredUnitRule(string Message) : IHeadcountScheduleRule
{
    public List<string> Check(HeadcountSchedule item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountScheduleDiscountRangeRule(double Min, double Max) : IHeadcountScheduleRule
{
    public List<string> Check(HeadcountSchedule item)
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

public sealed record HeadcountScheduleCodePrefixRule(string Prefix) : IHeadcountScheduleRule
{
    public List<string> Check(HeadcountSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountScheduleAllowedCodeRule(List<string> Allowed) : IHeadcountScheduleRule
{
    public List<string> Check(HeadcountSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount schedule.</summary>
public sealed record HeadcountScheduleValidator(List<IHeadcountScheduleRule> Rules)
{
    public List<string> Validate(HeadcountSchedule item)
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

    public void RequireValid(HeadcountSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountSchedule> items)
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

/// <summary>Functions of headcount schedule rules.</summary>
public static class HeadcountScheduleRules
{
    public static HeadcountScheduleValidator DefaultHeadcountScheduleValidator()
    {
        return new HeadcountScheduleValidator(
            new List<IHeadcountScheduleRule>
            {
                new HeadcountScheduleUnitLengthRule(16),
                new HeadcountScheduleRequiredUnitRule("headcount schedule needs a value"),
                new HeadcountScheduleDiscountRangeRule(4.5, 4793.1),
                new HeadcountScheduleCodePrefixRule("ACM"),
                new HeadcountScheduleAllowedCodeRule(new List<string> { "legacy", "capped" }),
            });
    }
}
