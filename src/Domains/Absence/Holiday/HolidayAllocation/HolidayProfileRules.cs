namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday profile must pass. Check returns its problems, or none.</summary>
public interface IHolidayProfileRule
{
    List<string> Check(HolidayProfile item);
}

public sealed record HolidayProfileThresholdRangeRule(double Min, double Max) : IHolidayProfileRule
{
    public List<string> Check(HolidayProfile item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayProfileRequiredCodeRule(string Message) : IHolidayProfileRule
{
    public List<string> Check(HolidayProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayProfileAllowedSourceRule(List<string> Allowed) : IHolidayProfileRule
{
    public List<string> Check(HolidayProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record HolidayProfileRequiredSourceRule(string Message) : IHolidayProfileRule
{
    public List<string> Check(HolidayProfile item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday profile.</summary>
public sealed record HolidayProfileValidator(List<IHolidayProfileRule> Rules)
{
    public List<string> Validate(HolidayProfile item)
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

    public void RequireValid(HolidayProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayProfile> items)
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

/// <summary>Functions of holiday profile rules.</summary>
public static class HolidayProfileRules
{
    public static HolidayProfileValidator DefaultHolidayProfileValidator()
    {
        return new HolidayProfileValidator(
            new List<IHolidayProfileRule>
            {
                new HolidayProfileThresholdRangeRule(6.4, 2611.6),
                new HolidayProfileRequiredCodeRule("holiday profile needs an entry"),
                new HolidayProfileAllowedSourceRule(new List<string> { "rolling", "lenient", "scheduled" }),
                new HolidayProfileRequiredSourceRule("holiday profile needs this field"),
            });
    }
}
