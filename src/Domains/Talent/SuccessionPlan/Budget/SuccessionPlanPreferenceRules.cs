namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan preference must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanPreferenceRule
{
    List<string> Check(SuccessionPlanPreference item);
}

public sealed record SuccessionPlanPreferenceThresholdRangeRule(double Min, double Max) : ISuccessionPlanPreferenceRule
{
    public List<string> Check(SuccessionPlanPreference item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanPreferenceFteRangeRule(double Min, double Max) : ISuccessionPlanPreferenceRule
{
    public List<string> Check(SuccessionPlanPreference item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanPreferenceRequiredLabelRule(string Message) : ISuccessionPlanPreferenceRule
{
    public List<string> Check(SuccessionPlanPreference item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanPreferenceAllowedTeamRule(List<string> Allowed) : ISuccessionPlanPreferenceRule
{
    public List<string> Check(SuccessionPlanPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan preference.</summary>
public sealed record SuccessionPlanPreferenceValidator(List<ISuccessionPlanPreferenceRule> Rules)
{
    public List<string> Validate(SuccessionPlanPreference item)
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

    public void RequireValid(SuccessionPlanPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanPreference> items)
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

/// <summary>Functions of succession plan preference rules.</summary>
public static class SuccessionPlanPreferenceRules
{
    public static SuccessionPlanPreferenceValidator DefaultSuccessionPlanPreferenceValidator()
    {
        return new SuccessionPlanPreferenceValidator(
            new List<ISuccessionPlanPreferenceRule>
            {
                new SuccessionPlanPreferenceThresholdRangeRule(3.2, 3892.2),
                new SuccessionPlanPreferenceFteRangeRule(6.5, 246.9),
                new SuccessionPlanPreferenceRequiredLabelRule("succession plan preference needs a value"),
                new SuccessionPlanPreferenceAllowedTeamRule(new List<string> { "default" }),
            });
    }
}
