namespace Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>A rule a strict tax form tier must pass. Check returns its problems, or none.</summary>
public interface IStrictTaxFormTierRule
{
    List<string> Check(StrictTaxFormTier item);
}

public sealed record StrictTaxFormTierRequiredTimezoneRule(string Message) : IStrictTaxFormTierRule
{
    public List<string> Check(StrictTaxFormTier item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictTaxFormTierLabelLengthRule(int MaxLength) : IStrictTaxFormTierRule
{
    public List<string> Check(StrictTaxFormTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTaxFormTierTimezoneLengthRule(int MaxLength) : IStrictTaxFormTierRule
{
    public List<string> Check(StrictTaxFormTier item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTaxFormTierRequiredSkillRule(string Message) : IStrictTaxFormTierRule
{
    public List<string> Check(StrictTaxFormTier item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict tax form tier.</summary>
public sealed record StrictTaxFormTierValidator(List<IStrictTaxFormTierRule> Rules)
{
    public List<string> Validate(StrictTaxFormTier item)
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

    public void RequireValid(StrictTaxFormTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictTaxFormTier> items)
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

/// <summary>Functions of strict tax form tier rules.</summary>
public static class StrictTaxFormTierRules
{
    public static StrictTaxFormTierValidator DefaultStrictTaxFormTierValidator()
    {
        return new StrictTaxFormTierValidator(
            new List<IStrictTaxFormTierRule>
            {
                new StrictTaxFormTierRequiredTimezoneRule("strict tax form tier needs this field"),
                new StrictTaxFormTierLabelLengthRule(255),
                new StrictTaxFormTierTimezoneLengthRule(86400),
                new StrictTaxFormTierRequiredSkillRule("strict tax form tier needs a value"),
            });
    }
}
