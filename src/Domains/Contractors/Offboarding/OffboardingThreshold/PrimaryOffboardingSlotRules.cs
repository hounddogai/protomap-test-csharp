namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>A rule a primary offboarding slot must pass. Check returns its problems, or none.</summary>
public interface IPrimaryOffboardingSlotRule
{
    List<string> Check(PrimaryOffboardingSlot item);
}

public sealed record PrimaryOffboardingSlotNameLengthRule(int MaxLength) : IPrimaryOffboardingSlotRule
{
    public List<string> Check(PrimaryOffboardingSlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOffboardingSlotDiscountRangeRule(double Min, double Max) : IPrimaryOffboardingSlotRule
{
    public List<string> Check(PrimaryOffboardingSlot item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOffboardingSlotRequiredCodeRule(string Message) : IPrimaryOffboardingSlotRule
{
    public List<string> Check(PrimaryOffboardingSlot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary offboarding slot.</summary>
public sealed record PrimaryOffboardingSlotValidator(List<IPrimaryOffboardingSlotRule> Rules)
{
    public List<string> Validate(PrimaryOffboardingSlot item)
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

    public void RequireValid(PrimaryOffboardingSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryOffboardingSlot> items)
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

/// <summary>Functions of primary offboarding slot rules.</summary>
public static class PrimaryOffboardingSlotRules
{
    public static PrimaryOffboardingSlotValidator DefaultPrimaryOffboardingSlotValidator()
    {
        return new PrimaryOffboardingSlotValidator(
            new List<IPrimaryOffboardingSlotRule>
            {
                new PrimaryOffboardingSlotNameLengthRule(3600),
                new PrimaryOffboardingSlotDiscountRangeRule(7.8, 3163.3),
                new PrimaryOffboardingSlotRequiredCodeRule("primary offboarding slot needs an entry"),
            });
    }
}
