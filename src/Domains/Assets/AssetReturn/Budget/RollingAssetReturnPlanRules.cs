namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling asset return plan must pass. Check returns its problems, or none.</summary>
public interface IRollingAssetReturnPlanRule
{
    List<string> Check(RollingAssetReturnPlan item);
}

public sealed record RollingAssetReturnPlanRetriesRangeRule(int Min, int Max) : IRollingAssetReturnPlanRule
{
    public List<string> Check(RollingAssetReturnPlan item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnPlanAllowedCodeRule(List<string> Allowed) : IRollingAssetReturnPlanRule
{
    public List<string> Check(RollingAssetReturnPlan item)
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

public sealed record RollingAssetReturnPlanGraceDaysRangeRule(int Min, int Max) : IRollingAssetReturnPlanRule
{
    public List<string> Check(RollingAssetReturnPlan item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingAssetReturnPlanRequiredLabelRule(string Message) : IRollingAssetReturnPlanRule
{
    public List<string> Check(RollingAssetReturnPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnPlanPhoneLengthRule(int MaxLength) : IRollingAssetReturnPlanRule
{
    public List<string> Check(RollingAssetReturnPlan item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnPlanRequiredPhoneRule(string Message) : IRollingAssetReturnPlanRule
{
    public List<string> Check(RollingAssetReturnPlan item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling asset return plan.</summary>
public sealed record RollingAssetReturnPlanValidator(List<IRollingAssetReturnPlanRule> Rules)
{
    public List<string> Validate(RollingAssetReturnPlan item)
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

    public void RequireValid(RollingAssetReturnPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAssetReturnPlan> items)
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

/// <summary>Functions of rolling asset return plan rules.</summary>
public static class RollingAssetReturnPlanRules
{
    public static RollingAssetReturnPlanValidator DefaultRollingAssetReturnPlanValidator()
    {
        return new RollingAssetReturnPlanValidator(
            new List<IRollingAssetReturnPlanRule>
            {
                new RollingAssetReturnPlanRetriesRangeRule(80, 120),
                new RollingAssetReturnPlanAllowedCodeRule(new List<string>()),
                new RollingAssetReturnPlanGraceDaysRangeRule(120, 3600),
                new RollingAssetReturnPlanRequiredLabelRule("rolling asset return plan needs a value"),
                new RollingAssetReturnPlanPhoneLengthRule(120),
                new RollingAssetReturnPlanRequiredPhoneRule("rolling asset return plan needs an entry"),
            });
    }
}
