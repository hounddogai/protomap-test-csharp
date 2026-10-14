namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped offboarding slot must pass. Check returns its problems, or none.</summary>
public interface ICappedOffboardingSlotRule
{
    List<string> Check(CappedOffboardingSlot item);
}

public sealed record CappedOffboardingSlotAllowedNameRule(List<string> Allowed) : ICappedOffboardingSlotRule
{
    public List<string> Check(CappedOffboardingSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingSlotTitleLengthRule(int MaxLength) : ICappedOffboardingSlotRule
{
    public List<string> Check(CappedOffboardingSlot item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingSlotPhoneLengthRule(int MaxLength) : ICappedOffboardingSlotRule
{
    public List<string> Check(CappedOffboardingSlot item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped offboarding slot.</summary>
public sealed record CappedOffboardingSlotValidator(List<ICappedOffboardingSlotRule> Rules)
{
    public List<string> Validate(CappedOffboardingSlot item)
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

    public void RequireValid(CappedOffboardingSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedOffboardingSlot> items)
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

/// <summary>Functions of capped offboarding slot rules.</summary>
public static class CappedOffboardingSlotRules
{
    public static CappedOffboardingSlotValidator DefaultCappedOffboardingSlotValidator()
    {
        return new CappedOffboardingSlotValidator(
            new List<ICappedOffboardingSlotRule>
            {
                new CappedOffboardingSlotAllowedNameRule(new List<string> { "legacy", "batched" }),
                new CappedOffboardingSlotTitleLengthRule(255),
                new CappedOffboardingSlotPhoneLengthRule(1000),
            });
    }
}
