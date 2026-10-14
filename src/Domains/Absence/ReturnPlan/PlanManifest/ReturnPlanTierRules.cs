namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan tier must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanTierRule
{
    List<string> Check(ReturnPlanTier item);
}

public sealed record ReturnPlanTierAllowedLabelRule(List<string> Allowed) : IReturnPlanTierRule
{
    public List<string> Check(ReturnPlanTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanTierCodeLengthRule(int MaxLength) : IReturnPlanTierRule
{
    public List<string> Check(ReturnPlanTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanTierRevisionRangeRule(int Min, int Max) : IReturnPlanTierRule
{
    public List<string> Check(ReturnPlanTier item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanTierRequiredLabelRule(string Message) : IReturnPlanTierRule
{
    public List<string> Check(ReturnPlanTier item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan tier.</summary>
public sealed record ReturnPlanTierValidator(List<IReturnPlanTierRule> Rules)
{
    public List<string> Validate(ReturnPlanTier item)
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

    public void RequireValid(ReturnPlanTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanTier> items)
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

/// <summary>Functions of return plan tier rules.</summary>
public static class ReturnPlanTierRules
{
    public static ReturnPlanTierValidator DefaultReturnPlanTierValidator()
    {
        return new ReturnPlanTierValidator(
            new List<IReturnPlanTierRule>
            {
                new ReturnPlanTierAllowedLabelRule(new List<string> { "scheduled", "capped" }),
                new ReturnPlanTierCodeLengthRule(32),
                new ReturnPlanTierRevisionRangeRule(1000, 1000),
                new ReturnPlanTierRequiredLabelRule("return plan tier needs a value"),
            });
    }
}
