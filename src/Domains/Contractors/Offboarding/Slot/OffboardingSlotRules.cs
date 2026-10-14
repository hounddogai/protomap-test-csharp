namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding slot must pass. Check returns its problems, or none.</summary>
public interface IOffboardingSlotRule
{
    List<string> Check(OffboardingSlot item);
}

public sealed record OffboardingSlotAllowedNameRule(List<string> Allowed) : IOffboardingSlotRule
{
    public List<string> Check(OffboardingSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingSlotFteRangeRule(double Min, double Max) : IOffboardingSlotRule
{
    public List<string> Check(OffboardingSlot item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingSlotRequiredCodeRule(string Message) : IOffboardingSlotRule
{
    public List<string> Check(OffboardingSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding slot.</summary>
public sealed record OffboardingSlotValidator(List<IOffboardingSlotRule> Rules)
{
    public List<string> Validate(OffboardingSlot item)
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

    public void RequireValid(OffboardingSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingSlot> items)
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

/// <summary>Functions of offboarding slot rules.</summary>
public static class OffboardingSlotRules
{
    public static OffboardingSlotValidator DefaultOffboardingSlotValidator()
    {
        return new OffboardingSlotValidator(
            new List<IOffboardingSlotRule>
            {
                new OffboardingSlotAllowedNameRule(new List<string>()),
                new OffboardingSlotFteRangeRule(2.0, 949.1),
                new OffboardingSlotRequiredCodeRule("offboarding slot needs an entry"),
            });
    }
}
