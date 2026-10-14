namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default return plan tier must pass. Check returns its problems, or none.</summary>
public interface IDefaultReturnPlanTierRule
{
    List<string> Check(DefaultReturnPlanTier item);
}

public sealed record DefaultReturnPlanTierCodeLengthRule(int MaxLength) : IDefaultReturnPlanTierRule
{
    public List<string> Check(DefaultReturnPlanTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanTierRequiredChannelRule(string Message) : IDefaultReturnPlanTierRule
{
    public List<string> Check(DefaultReturnPlanTier item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanTierAllowedChannelRule(List<string> Allowed) : IDefaultReturnPlanTierRule
{
    public List<string> Check(DefaultReturnPlanTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanTierLabelLengthRule(int MaxLength) : IDefaultReturnPlanTierRule
{
    public List<string> Check(DefaultReturnPlanTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanTierRequiredCodeRule(string Message) : IDefaultReturnPlanTierRule
{
    public List<string> Check(DefaultReturnPlanTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default return plan tier.</summary>
public sealed record DefaultReturnPlanTierValidator(List<IDefaultReturnPlanTierRule> Rules)
{
    public List<string> Validate(DefaultReturnPlanTier item)
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

    public void RequireValid(DefaultReturnPlanTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReturnPlanTier> items)
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

/// <summary>Functions of default return plan tier rules.</summary>
public static class DefaultReturnPlanTierRules
{
    public static DefaultReturnPlanTierValidator DefaultDefaultReturnPlanTierValidator()
    {
        return new DefaultReturnPlanTierValidator(
            new List<IDefaultReturnPlanTierRule>
            {
                new DefaultReturnPlanTierCodeLengthRule(32),
                new DefaultReturnPlanTierRequiredChannelRule("default return plan tier needs this field"),
                new DefaultReturnPlanTierAllowedChannelRule(new List<string> { "lenient", "adaptive", "scheduled" }),
                new DefaultReturnPlanTierLabelLengthRule(120),
                new DefaultReturnPlanTierRequiredCodeRule("default return plan tier needs a value"),
            });
    }
}
