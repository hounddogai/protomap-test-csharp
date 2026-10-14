namespace Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>A rule a skill profile metric must pass. Check returns its problems, or none.</summary>
public interface ISkillProfileMetricRule
{
    List<string> Check(SkillProfileMetric item);
}

public sealed record SkillProfileMetricThresholdRangeRule(double Min, double Max) : ISkillProfileMetricRule
{
    public List<string> Check(SkillProfileMetric item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileMetricVisibleGuardRule(double Limit) : ISkillProfileMetricRule
{
    public List<string> Check(SkillProfileMetric item)
    {
        if (item.Visible && item.Hours > Limit)
        {
            return new List<string> { $"hours {item.Hours} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileMetricLocaleLengthRule(int MaxLength) : ISkillProfileMetricRule
{
    public List<string> Check(SkillProfileMetric item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileMetricRequiredLocaleRule(string Message) : ISkillProfileMetricRule
{
    public List<string> Check(SkillProfileMetric item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SkillProfileMetricFloorValueRangeRule(double Min, double Max) : ISkillProfileMetricRule
{
    public List<string> Check(SkillProfileMetric item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SkillProfileMetricRequiredTitleRule(string Message) : ISkillProfileMetricRule
{
    public List<string> Check(SkillProfileMetric item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a skill profile metric.</summary>
public sealed record SkillProfileMetricValidator(List<ISkillProfileMetricRule> Rules)
{
    public List<string> Validate(SkillProfileMetric item)
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

    public void RequireValid(SkillProfileMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SkillProfileMetric> items)
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

/// <summary>Functions of skill profile metric rules.</summary>
public static class SkillProfileMetricRules
{
    public static SkillProfileMetricValidator DefaultSkillProfileMetricValidator()
    {
        return new SkillProfileMetricValidator(
            new List<ISkillProfileMetricRule>
            {
                new SkillProfileMetricThresholdRangeRule(3.6, 1451.4),
                new SkillProfileMetricVisibleGuardRule(10.0),
                new SkillProfileMetricLocaleLengthRule(80),
                new SkillProfileMetricRequiredLocaleRule("skill profile metric needs an entry"),
                new SkillProfileMetricFloorValueRangeRule(2.7, 3248.0),
                new SkillProfileMetricRequiredTitleRule("skill profile metric needs a value"),
            });
    }
}
