namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered pay band schedule must pass. Check returns its problems, or none.</summary>
public interface ITieredPayBandScheduleRule
{
    List<string> Check(TieredPayBandSchedule item);
}

public sealed record TieredPayBandScheduleRequiredNameRule(string Message) : ITieredPayBandScheduleRule
{
    public List<string> Check(TieredPayBandSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandScheduleExternalRefLengthRule(int MaxLength) : ITieredPayBandScheduleRule
{
    public List<string> Check(TieredPayBandSchedule item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayBandScheduleAllowedEmployeeIdRule(List<string> Allowed) : ITieredPayBandScheduleRule
{
    public List<string> Check(TieredPayBandSchedule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.EmployeeId)))
        {
            return new List<string> { $"employee id {item.EmployeeId} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pay band schedule.</summary>
public sealed record TieredPayBandScheduleValidator(List<ITieredPayBandScheduleRule> Rules)
{
    public List<string> Validate(TieredPayBandSchedule item)
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

    public void RequireValid(TieredPayBandSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPayBandSchedule> items)
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

/// <summary>Functions of tiered pay band schedule rules.</summary>
public static class TieredPayBandScheduleRules
{
    public static TieredPayBandScheduleValidator DefaultTieredPayBandScheduleValidator()
    {
        return new TieredPayBandScheduleValidator(
            new List<ITieredPayBandScheduleRule>
            {
                new TieredPayBandScheduleRequiredNameRule("tiered pay band schedule needs this field"),
                new TieredPayBandScheduleExternalRefLengthRule(16),
                new TieredPayBandScheduleAllowedEmployeeIdRule(new List<string> { "strict", "tiered", "manual" }),
            });
    }
}
