namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled garnishment plan must pass. Check returns its problems, or none.</summary>
public interface IScheduledGarnishmentPlanRule
{
    List<string> Check(ScheduledGarnishmentPlan item);
}

public sealed record ScheduledGarnishmentPlanRequiredChannelRule(string Message) : IScheduledGarnishmentPlanRule
{
    public List<string> Check(ScheduledGarnishmentPlan item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledGarnishmentPlanRequiredSalaryBandRule(string Message) : IScheduledGarnishmentPlanRule
{
    public List<string> Check(ScheduledGarnishmentPlan item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledGarnishmentPlanRequiredCodeRule(string Message) : IScheduledGarnishmentPlanRule
{
    public List<string> Check(ScheduledGarnishmentPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledGarnishmentPlanAllowedChannelRule(List<string> Allowed) : IScheduledGarnishmentPlanRule
{
    public List<string> Check(ScheduledGarnishmentPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledGarnishmentPlanChannelLengthRule(int MaxLength) : IScheduledGarnishmentPlanRule
{
    public List<string> Check(ScheduledGarnishmentPlan item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledGarnishmentPlanVersionRangeRule(int Min, int Max) : IScheduledGarnishmentPlanRule
{
    public List<string> Check(ScheduledGarnishmentPlan item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled garnishment plan.</summary>
public sealed record ScheduledGarnishmentPlanValidator(List<IScheduledGarnishmentPlanRule> Rules)
{
    public List<string> Validate(ScheduledGarnishmentPlan item)
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

    public void RequireValid(ScheduledGarnishmentPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledGarnishmentPlan> items)
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

/// <summary>Functions of scheduled garnishment plan rules.</summary>
public static class ScheduledGarnishmentPlanRules
{
    public static ScheduledGarnishmentPlanValidator DefaultScheduledGarnishmentPlanValidator()
    {
        return new ScheduledGarnishmentPlanValidator(
            new List<IScheduledGarnishmentPlanRule>
            {
                new ScheduledGarnishmentPlanRequiredChannelRule("scheduled garnishment plan needs this field"),
                new ScheduledGarnishmentPlanRequiredSalaryBandRule("scheduled garnishment plan needs an entry"),
                new ScheduledGarnishmentPlanRequiredCodeRule("scheduled garnishment plan needs a value"),
                new ScheduledGarnishmentPlanAllowedChannelRule(new List<string> { "rolling" }),
                new ScheduledGarnishmentPlanChannelLengthRule(255),
                new ScheduledGarnishmentPlanVersionRangeRule(1000, 86400),
            });
    }
}
