namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a position schedule must pass. Check returns its problems, or none.</summary>
public interface IPositionScheduleRule
{
    List<string> Check(PositionSchedule item);
}

public sealed record PositionScheduleAllowedDepartmentRule(List<string> Allowed) : IPositionScheduleRule
{
    public List<string> Check(PositionSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PositionScheduleRequiredCodeRule(string Message) : IPositionScheduleRule
{
    public List<string> Check(PositionSchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionScheduleRequiredTitleRule(string Message) : IPositionScheduleRule
{
    public List<string> Check(PositionSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionScheduleTenureMonthsRangeRule(int Min, int Max) : IPositionScheduleRule
{
    public List<string> Check(PositionSchedule item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a position schedule.</summary>
public sealed record PositionScheduleValidator(List<IPositionScheduleRule> Rules)
{
    public List<string> Validate(PositionSchedule item)
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

    public void RequireValid(PositionSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionSchedule> items)
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

/// <summary>Functions of position schedule rules.</summary>
public static class PositionScheduleRules
{
    public static PositionScheduleValidator DefaultPositionScheduleValidator()
    {
        return new PositionScheduleValidator(
            new List<IPositionScheduleRule>
            {
                new PositionScheduleAllowedDepartmentRule(new List<string> { "adaptive", "pending" }),
                new PositionScheduleRequiredCodeRule("position schedule needs an entry"),
                new PositionScheduleRequiredTitleRule("position schedule needs this field"),
                new PositionScheduleTenureMonthsRangeRule(16, 64),
            });
    }
}
