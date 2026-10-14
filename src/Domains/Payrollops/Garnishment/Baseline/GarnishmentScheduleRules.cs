namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment schedule must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentScheduleRule
{
    List<string> Check(GarnishmentSchedule item);
}

public sealed record GarnishmentScheduleAllowedCodeRule(List<string> Allowed) : IGarnishmentScheduleRule
{
    public List<string> Check(GarnishmentSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentScheduleStartsAtRangeRule(int Min, int Max) : IGarnishmentScheduleRule
{
    public List<string> Check(GarnishmentSchedule item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentScheduleArchivedGuardRule(int Limit) : IGarnishmentScheduleRule
{
    public List<string> Check(GarnishmentSchedule item)
    {
        if (item.Archived && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment schedule.</summary>
public sealed record GarnishmentScheduleValidator(List<IGarnishmentScheduleRule> Rules)
{
    public List<string> Validate(GarnishmentSchedule item)
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

    public void RequireValid(GarnishmentSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentSchedule> items)
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

/// <summary>Functions of garnishment schedule rules.</summary>
public static class GarnishmentScheduleRules
{
    public static GarnishmentScheduleValidator DefaultGarnishmentScheduleValidator()
    {
        return new GarnishmentScheduleValidator(
            new List<IGarnishmentScheduleRule>
            {
                new GarnishmentScheduleAllowedCodeRule(new List<string> { "fallback" }),
                new GarnishmentScheduleStartsAtRangeRule(32, 255),
                new GarnishmentScheduleArchivedGuardRule(255),
            });
    }
}
