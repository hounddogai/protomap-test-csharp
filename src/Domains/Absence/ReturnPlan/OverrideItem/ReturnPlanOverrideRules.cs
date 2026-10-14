namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>A rule a return plan override must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanOverrideRule
{
    List<string> Check(ReturnPlanOverride item);
}

public sealed record ReturnPlanOverrideOwnerIdLengthRule(int MaxLength) : IReturnPlanOverrideRule
{
    public List<string> Check(ReturnPlanOverride item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanOverrideArchivedGuardRule(int Limit) : IReturnPlanOverrideRule
{
    public List<string> Check(ReturnPlanOverride item)
    {
        if (item.Archived && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanOverrideRequiredLocaleRule(string Message) : IReturnPlanOverrideRule
{
    public List<string> Check(ReturnPlanOverride item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanOverrideRequiredOwnerIdRule(string Message) : IReturnPlanOverrideRule
{
    public List<string> Check(ReturnPlanOverride item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanOverrideCodePrefixRule(string Prefix) : IReturnPlanOverrideRule
{
    public List<string> Check(ReturnPlanOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan override.</summary>
public sealed record ReturnPlanOverrideValidator(List<IReturnPlanOverrideRule> Rules)
{
    public List<string> Validate(ReturnPlanOverride item)
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

    public void RequireValid(ReturnPlanOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanOverride> items)
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

/// <summary>Functions of return plan override rules.</summary>
public static class ReturnPlanOverrideRules
{
    public static ReturnPlanOverrideValidator DefaultReturnPlanOverrideValidator()
    {
        return new ReturnPlanOverrideValidator(
            new List<IReturnPlanOverrideRule>
            {
                new ReturnPlanOverrideOwnerIdLengthRule(1000),
                new ReturnPlanOverrideArchivedGuardRule(255),
                new ReturnPlanOverrideRequiredLocaleRule("return plan override needs this field"),
                new ReturnPlanOverrideRequiredOwnerIdRule("return plan override needs this field"),
                new ReturnPlanOverrideCodePrefixRule("OPS"),
            });
    }
}
