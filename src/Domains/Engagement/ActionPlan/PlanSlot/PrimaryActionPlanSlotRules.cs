namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>A rule a primary action plan slot must pass. Check returns its problems, or none.</summary>
public interface IPrimaryActionPlanSlotRule
{
    List<string> Check(PrimaryActionPlanSlot item);
}

public sealed record PrimaryActionPlanSlotRequiredNameRule(string Message) : IPrimaryActionPlanSlotRule
{
    public List<string> Check(PrimaryActionPlanSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryActionPlanSlotRequiredLocaleRule(string Message) : IPrimaryActionPlanSlotRule
{
    public List<string> Check(PrimaryActionPlanSlot item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryActionPlanSlotRequiredCodeRule(string Message) : IPrimaryActionPlanSlotRule
{
    public List<string> Check(PrimaryActionPlanSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryActionPlanSlotCodeLengthRule(int MaxLength) : IPrimaryActionPlanSlotRule
{
    public List<string> Check(PrimaryActionPlanSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryActionPlanSlotCodePrefixRule(string Prefix) : IPrimaryActionPlanSlotRule
{
    public List<string> Check(PrimaryActionPlanSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary action plan slot.</summary>
public sealed record PrimaryActionPlanSlotValidator(List<IPrimaryActionPlanSlotRule> Rules)
{
    public List<string> Validate(PrimaryActionPlanSlot item)
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

    public void RequireValid(PrimaryActionPlanSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryActionPlanSlot> items)
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

/// <summary>Functions of primary action plan slot rules.</summary>
public static class PrimaryActionPlanSlotRules
{
    public static PrimaryActionPlanSlotValidator DefaultPrimaryActionPlanSlotValidator()
    {
        return new PrimaryActionPlanSlotValidator(
            new List<IPrimaryActionPlanSlotRule>
            {
                new PrimaryActionPlanSlotRequiredNameRule("primary action plan slot needs a value"),
                new PrimaryActionPlanSlotRequiredLocaleRule("primary action plan slot needs an entry"),
                new PrimaryActionPlanSlotRequiredCodeRule("primary action plan slot needs an entry"),
                new PrimaryActionPlanSlotCodeLengthRule(1000),
                new PrimaryActionPlanSlotCodePrefixRule("Q"),
            });
    }
}
