namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>A rule a merit cycle override must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleOverrideRule
{
    List<string> Check(MeritCycleOverride item);
}

public sealed record MeritCycleOverrideRequiredLocaleRule(string Message) : IMeritCycleOverrideRule
{
    public List<string> Check(MeritCycleOverride item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleOverrideRequiredLabelRule(string Message) : IMeritCycleOverrideRule
{
    public List<string> Check(MeritCycleOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleOverrideCategoryLengthRule(int MaxLength) : IMeritCycleOverrideRule
{
    public List<string> Check(MeritCycleOverride item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleOverrideLocaleLengthRule(int MaxLength) : IMeritCycleOverrideRule
{
    public List<string> Check(MeritCycleOverride item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleOverrideAmountRangeRule(double Min, double Max) : IMeritCycleOverrideRule
{
    public List<string> Check(MeritCycleOverride item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a merit cycle override.</summary>
public sealed record MeritCycleOverrideValidator(List<IMeritCycleOverrideRule> Rules)
{
    public List<string> Validate(MeritCycleOverride item)
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

    public void RequireValid(MeritCycleOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleOverride> items)
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

/// <summary>Functions of merit cycle override rules.</summary>
public static class MeritCycleOverrideRules
{
    public static MeritCycleOverrideValidator DefaultMeritCycleOverrideValidator()
    {
        return new MeritCycleOverrideValidator(
            new List<IMeritCycleOverrideRule>
            {
                new MeritCycleOverrideRequiredLocaleRule("merit cycle override needs this field"),
                new MeritCycleOverrideRequiredLabelRule("merit cycle override needs a value"),
                new MeritCycleOverrideCategoryLengthRule(86400),
                new MeritCycleOverrideLocaleLengthRule(120),
                new MeritCycleOverrideAmountRangeRule(9.9, 1665.7),
            });
    }
}
