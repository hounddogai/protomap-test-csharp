namespace Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>A rule a lenient return plan override must pass. Check returns its problems, or none.</summary>
public interface ILenientReturnPlanOverrideRule
{
    List<string> Check(LenientReturnPlanOverride item);
}

public sealed record LenientReturnPlanOverrideRequiredSourceRule(string Message) : ILenientReturnPlanOverrideRule
{
    public List<string> Check(LenientReturnPlanOverride item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientReturnPlanOverrideCodeLengthRule(int MaxLength) : ILenientReturnPlanOverrideRule
{
    public List<string> Check(LenientReturnPlanOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientReturnPlanOverrideTitleLengthRule(int MaxLength) : ILenientReturnPlanOverrideRule
{
    public List<string> Check(LenientReturnPlanOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientReturnPlanOverrideRequiredNameRule(string Message) : ILenientReturnPlanOverrideRule
{
    public List<string> Check(LenientReturnPlanOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientReturnPlanOverrideNameLengthRule(int MaxLength) : ILenientReturnPlanOverrideRule
{
    public List<string> Check(LenientReturnPlanOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient return plan override.</summary>
public sealed record LenientReturnPlanOverrideValidator(List<ILenientReturnPlanOverrideRule> Rules)
{
    public List<string> Validate(LenientReturnPlanOverride item)
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

    public void RequireValid(LenientReturnPlanOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientReturnPlanOverride> items)
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

/// <summary>Functions of lenient return plan override rules.</summary>
public static class LenientReturnPlanOverrideRules
{
    public static LenientReturnPlanOverrideValidator DefaultLenientReturnPlanOverrideValidator()
    {
        return new LenientReturnPlanOverrideValidator(
            new List<ILenientReturnPlanOverrideRule>
            {
                new LenientReturnPlanOverrideRequiredSourceRule("lenient return plan override needs a value"),
                new LenientReturnPlanOverrideCodeLengthRule(16),
                new LenientReturnPlanOverrideTitleLengthRule(120),
                new LenientReturnPlanOverrideRequiredNameRule("lenient return plan override needs an entry"),
                new LenientReturnPlanOverrideNameLengthRule(86400),
            });
    }
}
