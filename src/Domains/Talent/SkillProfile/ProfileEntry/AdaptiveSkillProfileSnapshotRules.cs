namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

/// <summary>A rule a adaptive skill profile snapshot must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveSkillProfileSnapshotRule
{
    List<string> Check(AdaptiveSkillProfileSnapshot item);
}

public sealed record AdaptiveSkillProfileSnapshotRequiredCodeRule(string Message) : IAdaptiveSkillProfileSnapshotRule
{
    public List<string> Check(AdaptiveSkillProfileSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfileSnapshotWindowDaysRangeRule(int Min, int Max) : IAdaptiveSkillProfileSnapshotRule
{
    public List<string> Check(AdaptiveSkillProfileSnapshot item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfileSnapshotRequiredLabelRule(string Message) : IAdaptiveSkillProfileSnapshotRule
{
    public List<string> Check(AdaptiveSkillProfileSnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfileSnapshotCodePrefixRule(string Prefix) : IAdaptiveSkillProfileSnapshotRule
{
    public List<string> Check(AdaptiveSkillProfileSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfileSnapshotEmployeeIdLengthRule(int MaxLength) : IAdaptiveSkillProfileSnapshotRule
{
    public List<string> Check(AdaptiveSkillProfileSnapshot item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveSkillProfileSnapshotRequiredEmployeeIdRule(string Message) : IAdaptiveSkillProfileSnapshotRule
{
    public List<string> Check(AdaptiveSkillProfileSnapshot item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive skill profile snapshot.</summary>
public sealed record AdaptiveSkillProfileSnapshotValidator(List<IAdaptiveSkillProfileSnapshotRule> Rules)
{
    public List<string> Validate(AdaptiveSkillProfileSnapshot item)
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

    public void RequireValid(AdaptiveSkillProfileSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveSkillProfileSnapshot> items)
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

/// <summary>Functions of adaptive skill profile snapshot rules.</summary>
public static class AdaptiveSkillProfileSnapshotRules
{
    public static AdaptiveSkillProfileSnapshotValidator DefaultAdaptiveSkillProfileSnapshotValidator()
    {
        return new AdaptiveSkillProfileSnapshotValidator(
            new List<IAdaptiveSkillProfileSnapshotRule>
            {
                new AdaptiveSkillProfileSnapshotRequiredCodeRule("adaptive skill profile snapshot needs a value"),
                new AdaptiveSkillProfileSnapshotWindowDaysRangeRule(64, 64),
                new AdaptiveSkillProfileSnapshotRequiredLabelRule("adaptive skill profile snapshot needs a value"),
                new AdaptiveSkillProfileSnapshotCodePrefixRule("REF"),
                new AdaptiveSkillProfileSnapshotEmployeeIdLengthRule(3600),
                new AdaptiveSkillProfileSnapshotRequiredEmployeeIdRule(
                    "adaptive skill profile snapshot needs an entry"),
            });
    }
}
