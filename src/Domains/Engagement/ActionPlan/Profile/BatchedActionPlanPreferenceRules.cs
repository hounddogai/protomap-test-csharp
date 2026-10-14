namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched action plan preference must pass. Check returns its problems, or none.</summary>
public interface IBatchedActionPlanPreferenceRule
{
    List<string> Check(BatchedActionPlanPreference item);
}

public sealed record BatchedActionPlanPreferenceRequiredDescriptionRule(string Message) : IBatchedActionPlanPreferenceRule
{
    public List<string> Check(BatchedActionPlanPreference item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedActionPlanPreferenceAllowedTeamRule(List<string> Allowed) : IBatchedActionPlanPreferenceRule
{
    public List<string> Check(BatchedActionPlanPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is stale" };
        }
        return new List<string>();
    }
}

public sealed record BatchedActionPlanPreferenceRequiredTeamRule(string Message) : IBatchedActionPlanPreferenceRule
{
    public List<string> Check(BatchedActionPlanPreference item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedActionPlanPreferenceCodeLengthRule(int MaxLength) : IBatchedActionPlanPreferenceRule
{
    public List<string> Check(BatchedActionPlanPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedActionPlanPreferenceTenureMonthsRangeRule(int Min, int Max) : IBatchedActionPlanPreferenceRule
{
    public List<string> Check(BatchedActionPlanPreference item)
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

/// <summary>Runs every rule of a batched action plan preference.</summary>
public sealed record BatchedActionPlanPreferenceValidator(List<IBatchedActionPlanPreferenceRule> Rules)
{
    public List<string> Validate(BatchedActionPlanPreference item)
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

    public void RequireValid(BatchedActionPlanPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedActionPlanPreference> items)
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

/// <summary>Functions of batched action plan preference rules.</summary>
public static class BatchedActionPlanPreferenceRules
{
    public static BatchedActionPlanPreferenceValidator DefaultBatchedActionPlanPreferenceValidator()
    {
        return new BatchedActionPlanPreferenceValidator(
            new List<IBatchedActionPlanPreferenceRule>
            {
                new BatchedActionPlanPreferenceRequiredDescriptionRule(
                    "batched action plan preference needs this field"),
                new BatchedActionPlanPreferenceAllowedTeamRule(new List<string>()),
                new BatchedActionPlanPreferenceRequiredTeamRule("batched action plan preference needs this field"),
                new BatchedActionPlanPreferenceCodeLengthRule(16),
                new BatchedActionPlanPreferenceTenureMonthsRangeRule(255, 1000),
            });
    }
}
