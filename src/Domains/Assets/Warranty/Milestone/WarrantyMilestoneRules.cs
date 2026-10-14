namespace Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>A rule a warranty milestone must pass. Check returns its problems, or none.</summary>
public interface IWarrantyMilestoneRule
{
    List<string> Check(WarrantyMilestone item);
}

public sealed record WarrantyMilestoneSalaryBandLengthRule(int MaxLength) : IWarrantyMilestoneRule
{
    public List<string> Check(WarrantyMilestone item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyMilestoneRequiredNameRule(string Message) : IWarrantyMilestoneRule
{
    public List<string> Check(WarrantyMilestone item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyMilestoneRequiredTeamRule(string Message) : IWarrantyMilestoneRule
{
    public List<string> Check(WarrantyMilestone item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty milestone.</summary>
public sealed record WarrantyMilestoneValidator(List<IWarrantyMilestoneRule> Rules)
{
    public List<string> Validate(WarrantyMilestone item)
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

    public void RequireValid(WarrantyMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyMilestone> items)
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

/// <summary>Functions of warranty milestone rules.</summary>
public static class WarrantyMilestoneRules
{
    public static WarrantyMilestoneValidator DefaultWarrantyMilestoneValidator()
    {
        return new WarrantyMilestoneValidator(
            new List<IWarrantyMilestoneRule>
            {
                new WarrantyMilestoneSalaryBandLengthRule(1000),
                new WarrantyMilestoneRequiredNameRule("warranty milestone needs this field"),
                new WarrantyMilestoneRequiredTeamRule("warranty milestone needs this field"),
            });
    }
}
