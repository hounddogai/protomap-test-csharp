namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered position policy must pass. Check returns its problems, or none.</summary>
public interface ITieredPositionPolicyRule
{
    List<string> Check(TieredPositionPolicy item);
}

public sealed record TieredPositionPolicyTitleLengthRule(int MaxLength) : ITieredPositionPolicyRule
{
    public List<string> Check(TieredPositionPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionPolicyAllowedDescriptionRule(List<string> Allowed) : ITieredPositionPolicyRule
{
    public List<string> Check(TieredPositionPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is too long" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionPolicyRequiredDescriptionRule(string Message) : ITieredPositionPolicyRule
{
    public List<string> Check(TieredPositionPolicy item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered position policy.</summary>
public sealed record TieredPositionPolicyValidator(List<ITieredPositionPolicyRule> Rules)
{
    public List<string> Validate(TieredPositionPolicy item)
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

    public void RequireValid(TieredPositionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPositionPolicy> items)
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

/// <summary>Functions of tiered position policy rules.</summary>
public static class TieredPositionPolicyRules
{
    public static TieredPositionPolicyValidator DefaultTieredPositionPolicyValidator()
    {
        return new TieredPositionPolicyValidator(
            new List<ITieredPositionPolicyRule>
            {
                new TieredPositionPolicyTitleLengthRule(16),
                new TieredPositionPolicyAllowedDescriptionRule(new List<string>()),
                new TieredPositionPolicyRequiredDescriptionRule("tiered position policy needs a value"),
            });
    }
}
