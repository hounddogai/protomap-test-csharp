namespace Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>A rule a adaptive career path slot must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCareerPathSlotRule
{
    List<string> Check(AdaptiveCareerPathSlot item);
}

public sealed record AdaptiveCareerPathSlotRequiredNameRule(string Message) : IAdaptiveCareerPathSlotRule
{
    public List<string> Check(AdaptiveCareerPathSlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathSlotRequiredJobCodeRule(string Message) : IAdaptiveCareerPathSlotRule
{
    public List<string> Check(AdaptiveCareerPathSlot item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathSlotNameLengthRule(int MaxLength) : IAdaptiveCareerPathSlotRule
{
    public List<string> Check(AdaptiveCareerPathSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathSlotCodeLengthRule(int MaxLength) : IAdaptiveCareerPathSlotRule
{
    public List<string> Check(AdaptiveCareerPathSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCareerPathSlotCodePrefixRule(string Prefix) : IAdaptiveCareerPathSlotRule
{
    public List<string> Check(AdaptiveCareerPathSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive career path slot.</summary>
public sealed record AdaptiveCareerPathSlotValidator(List<IAdaptiveCareerPathSlotRule> Rules)
{
    public List<string> Validate(AdaptiveCareerPathSlot item)
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

    public void RequireValid(AdaptiveCareerPathSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCareerPathSlot> items)
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

/// <summary>Functions of adaptive career path slot rules.</summary>
public static class AdaptiveCareerPathSlotRules
{
    public static AdaptiveCareerPathSlotValidator DefaultAdaptiveCareerPathSlotValidator()
    {
        return new AdaptiveCareerPathSlotValidator(
            new List<IAdaptiveCareerPathSlotRule>
            {
                new AdaptiveCareerPathSlotRequiredNameRule("adaptive career path slot needs a value"),
                new AdaptiveCareerPathSlotRequiredJobCodeRule("adaptive career path slot needs this field"),
                new AdaptiveCareerPathSlotNameLengthRule(120),
                new AdaptiveCareerPathSlotCodeLengthRule(32),
                new AdaptiveCareerPathSlotCodePrefixRule("X"),
            });
    }
}
