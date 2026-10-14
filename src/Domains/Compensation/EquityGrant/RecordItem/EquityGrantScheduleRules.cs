namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a equity grant schedule must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantScheduleRule
{
    List<string> Check(EquityGrantSchedule item);
}

public sealed record EquityGrantScheduleAllowedTitleRule(List<string> Allowed) : IEquityGrantScheduleRule
{
    public List<string> Check(EquityGrantSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantScheduleMarginRangeRule(double Min, double Max) : IEquityGrantScheduleRule
{
    public List<string> Check(EquityGrantSchedule item)
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

public sealed record EquityGrantScheduleRequiredCodeRule(string Message) : IEquityGrantScheduleRule
{
    public List<string> Check(EquityGrantSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant schedule.</summary>
public sealed record EquityGrantScheduleValidator(List<IEquityGrantScheduleRule> Rules)
{
    public List<string> Validate(EquityGrantSchedule item)
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

    public void RequireValid(EquityGrantSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantSchedule> items)
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

/// <summary>Functions of equity grant schedule rules.</summary>
public static class EquityGrantScheduleRules
{
    public static EquityGrantScheduleValidator DefaultEquityGrantScheduleValidator()
    {
        return new EquityGrantScheduleValidator(
            new List<IEquityGrantScheduleRule>
            {
                new EquityGrantScheduleAllowedTitleRule(new List<string> { "rolling" }),
                new EquityGrantScheduleMarginRangeRule(3.0, 3937.9),
                new EquityGrantScheduleRequiredCodeRule("equity grant schedule needs a value"),
            });
    }
}
