namespace Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>A rule a adaptive warranty milestone must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveWarrantyMilestoneRule
{
    List<string> Check(AdaptiveWarrantyMilestone item);
}

public sealed record AdaptiveWarrantyMilestoneChecksumLengthRule(int MaxLength) : IAdaptiveWarrantyMilestoneRule
{
    public List<string> Check(AdaptiveWarrantyMilestone item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyMilestoneRequiredUnitRule(string Message) : IAdaptiveWarrantyMilestoneRule
{
    public List<string> Check(AdaptiveWarrantyMilestone item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveWarrantyMilestoneCodeLengthRule(int MaxLength) : IAdaptiveWarrantyMilestoneRule
{
    public List<string> Check(AdaptiveWarrantyMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive warranty milestone.</summary>
public sealed record AdaptiveWarrantyMilestoneValidator(List<IAdaptiveWarrantyMilestoneRule> Rules)
{
    public List<string> Validate(AdaptiveWarrantyMilestone item)
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

    public void RequireValid(AdaptiveWarrantyMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveWarrantyMilestone> items)
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

/// <summary>Functions of adaptive warranty milestone rules.</summary>
public static class AdaptiveWarrantyMilestoneRules
{
    public static AdaptiveWarrantyMilestoneValidator DefaultAdaptiveWarrantyMilestoneValidator()
    {
        return new AdaptiveWarrantyMilestoneValidator(
            new List<IAdaptiveWarrantyMilestoneRule>
            {
                new AdaptiveWarrantyMilestoneChecksumLengthRule(16),
                new AdaptiveWarrantyMilestoneRequiredUnitRule("adaptive warranty milestone needs a value"),
                new AdaptiveWarrantyMilestoneCodeLengthRule(86400),
            });
    }
}
