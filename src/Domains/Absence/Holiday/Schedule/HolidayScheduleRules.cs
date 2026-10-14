namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday schedule must pass. Check returns its problems, or none.</summary>
public interface IHolidayScheduleRule
{
    List<string> Check(HolidaySchedule item);
}

public sealed record HolidayScheduleRatioRangeRule(double Min, double Max) : IHolidayScheduleRule
{
    public List<string> Check(HolidaySchedule item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayScheduleCodePrefixRule(string Prefix) : IHolidayScheduleRule
{
    public List<string> Check(HolidaySchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayScheduleAllowedSourceRule(List<string> Allowed) : IHolidayScheduleRule
{
    public List<string> Check(HolidaySchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday schedule.</summary>
public sealed record HolidayScheduleValidator(List<IHolidayScheduleRule> Rules)
{
    public List<string> Validate(HolidaySchedule item)
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

    public void RequireValid(HolidaySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidaySchedule> items)
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

/// <summary>Functions of holiday schedule rules.</summary>
public static class HolidayScheduleRules
{
    public static HolidayScheduleValidator DefaultHolidayScheduleValidator()
    {
        return new HolidayScheduleValidator(
            new List<IHolidayScheduleRule>
            {
                new HolidayScheduleRatioRangeRule(6.0, 2473.6),
                new HolidayScheduleCodePrefixRule("OPS"),
                new HolidayScheduleAllowedSourceRule(new List<string>()),
            });
    }
}
