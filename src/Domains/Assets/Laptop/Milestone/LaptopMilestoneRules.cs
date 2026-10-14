namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a laptop milestone must pass. Check returns its problems, or none.</summary>
public interface ILaptopMilestoneRule
{
    List<string> Check(LaptopMilestone item);
}

public sealed record LaptopMilestoneDescriptionLengthRule(int MaxLength) : ILaptopMilestoneRule
{
    public List<string> Check(LaptopMilestone item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopMilestoneAllowedCodeRule(List<string> Allowed) : ILaptopMilestoneRule
{
    public List<string> Check(LaptopMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record LaptopMilestoneCodePrefixRule(string Prefix) : ILaptopMilestoneRule
{
    public List<string> Check(LaptopMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop milestone.</summary>
public sealed record LaptopMilestoneValidator(List<ILaptopMilestoneRule> Rules)
{
    public List<string> Validate(LaptopMilestone item)
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

    public void RequireValid(LaptopMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopMilestone> items)
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

/// <summary>Functions of laptop milestone rules.</summary>
public static class LaptopMilestoneRules
{
    public static LaptopMilestoneValidator DefaultLaptopMilestoneValidator()
    {
        return new LaptopMilestoneValidator(
            new List<ILaptopMilestoneRule>
            {
                new LaptopMilestoneDescriptionLengthRule(16),
                new LaptopMilestoneAllowedCodeRule(new List<string> { "pending" }),
                new LaptopMilestoneCodePrefixRule("REF"),
            });
    }
}
