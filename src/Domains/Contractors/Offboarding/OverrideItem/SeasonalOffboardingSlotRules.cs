namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal offboarding slot must pass. Check returns its problems, or none.</summary>
public interface ISeasonalOffboardingSlotRule
{
    List<string> Check(SeasonalOffboardingSlot item);
}

public sealed record SeasonalOffboardingSlotAllowedCodeRule(List<string> Allowed) : ISeasonalOffboardingSlotRule
{
    public List<string> Check(SeasonalOffboardingSlot item)
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

public sealed record SeasonalOffboardingSlotRequiredContactEmailRule(string Message) : ISeasonalOffboardingSlotRule
{
    public List<string> Check(SeasonalOffboardingSlot item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingSlotSequenceRangeRule(int Min, int Max) : ISeasonalOffboardingSlotRule
{
    public List<string> Check(SeasonalOffboardingSlot item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalOffboardingSlotMaximumRangeRule(double Min, double Max) : ISeasonalOffboardingSlotRule
{
    public List<string> Check(SeasonalOffboardingSlot item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalOffboardingSlotRequiredNameRule(string Message) : ISeasonalOffboardingSlotRule
{
    public List<string> Check(SeasonalOffboardingSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal offboarding slot.</summary>
public sealed record SeasonalOffboardingSlotValidator(List<ISeasonalOffboardingSlotRule> Rules)
{
    public List<string> Validate(SeasonalOffboardingSlot item)
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

    public void RequireValid(SeasonalOffboardingSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalOffboardingSlot> items)
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

/// <summary>Functions of seasonal offboarding slot rules.</summary>
public static class SeasonalOffboardingSlotRules
{
    public static SeasonalOffboardingSlotValidator DefaultSeasonalOffboardingSlotValidator()
    {
        return new SeasonalOffboardingSlotValidator(
            new List<ISeasonalOffboardingSlotRule>
            {
                new SeasonalOffboardingSlotAllowedCodeRule(new List<string> { "regional", "adaptive", "seasonal" }),
                new SeasonalOffboardingSlotRequiredContactEmailRule("seasonal offboarding slot needs a value"),
                new SeasonalOffboardingSlotSequenceRangeRule(120, 86400),
                new SeasonalOffboardingSlotMaximumRangeRule(8.9, 2380.8),
                new SeasonalOffboardingSlotRequiredNameRule("seasonal offboarding slot needs this field"),
            });
    }
}
