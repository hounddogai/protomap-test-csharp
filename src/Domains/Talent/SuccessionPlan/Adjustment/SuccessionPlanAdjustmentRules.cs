namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan adjustment must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanAdjustmentRule
{
    List<string> Check(SuccessionPlanAdjustment item);
}

public sealed record SuccessionPlanAdjustmentCodeLengthRule(int MaxLength) : ISuccessionPlanAdjustmentRule
{
    public List<string> Check(SuccessionPlanAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAdjustmentRequiredTitleRule(string Message) : ISuccessionPlanAdjustmentRule
{
    public List<string> Check(SuccessionPlanAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanAdjustmentAllowedTitleRule(List<string> Allowed) : ISuccessionPlanAdjustmentRule
{
    public List<string> Check(SuccessionPlanAdjustment item)
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

/// <summary>Runs every rule of a succession plan adjustment.</summary>
public sealed record SuccessionPlanAdjustmentValidator(List<ISuccessionPlanAdjustmentRule> Rules)
{
    public List<string> Validate(SuccessionPlanAdjustment item)
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

    public void RequireValid(SuccessionPlanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanAdjustment> items)
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

/// <summary>Functions of succession plan adjustment rules.</summary>
public static class SuccessionPlanAdjustmentRules
{
    public static SuccessionPlanAdjustmentValidator DefaultSuccessionPlanAdjustmentValidator()
    {
        return new SuccessionPlanAdjustmentValidator(
            new List<ISuccessionPlanAdjustmentRule>
            {
                new SuccessionPlanAdjustmentCodeLengthRule(64),
                new SuccessionPlanAdjustmentRequiredTitleRule("succession plan adjustment needs a value"),
                new SuccessionPlanAdjustmentAllowedTitleRule(new List<string> { "rolling", "lenient", "rolling" }),
            });
    }
}
