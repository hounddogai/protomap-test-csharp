namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy retro pay plan must pass. Check returns its problems, or none.</summary>
public interface ILegacyRetroPayPlanRule
{
    List<string> Check(LegacyRetroPayPlan item);
}

public sealed record LegacyRetroPayPlanRequiredTitleRule(string Message) : ILegacyRetroPayPlanRule
{
    public List<string> Check(LegacyRetroPayPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPayPlanRetriesRangeRule(int Min, int Max) : ILegacyRetroPayPlanRule
{
    public List<string> Check(LegacyRetroPayPlan item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyRetroPayPlanAllowedTitleRule(List<string> Allowed) : ILegacyRetroPayPlanRule
{
    public List<string> Check(LegacyRetroPayPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRetroPayPlanCapacityRangeRule(int Min, int Max) : ILegacyRetroPayPlanRule
{
    public List<string> Check(LegacyRetroPayPlan item)
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

/// <summary>Runs every rule of a legacy retro pay plan.</summary>
public sealed record LegacyRetroPayPlanValidator(List<ILegacyRetroPayPlanRule> Rules)
{
    public List<string> Validate(LegacyRetroPayPlan item)
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

    public void RequireValid(LegacyRetroPayPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRetroPayPlan> items)
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

/// <summary>Functions of legacy retro pay plan rules.</summary>
public static class LegacyRetroPayPlanRules
{
    public static LegacyRetroPayPlanValidator DefaultLegacyRetroPayPlanValidator()
    {
        return new LegacyRetroPayPlanValidator(
            new List<ILegacyRetroPayPlanRule>
            {
                new LegacyRetroPayPlanRequiredTitleRule("legacy retro pay plan needs an entry"),
                new LegacyRetroPayPlanRetriesRangeRule(1000, 86400),
                new LegacyRetroPayPlanAllowedTitleRule(new List<string>()),
                new LegacyRetroPayPlanCapacityRangeRule(32, 32),
            });
    }
}
