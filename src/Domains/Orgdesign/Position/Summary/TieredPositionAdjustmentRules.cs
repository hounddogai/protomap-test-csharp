namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered position adjustment must pass. Check returns its problems, or none.</summary>
public interface ITieredPositionAdjustmentRule
{
    List<string> Check(TieredPositionAdjustment item);
}

public sealed record TieredPositionAdjustmentRequiredContactEmailRule(string Message) : ITieredPositionAdjustmentRule
{
    public List<string> Check(TieredPositionAdjustment item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionAdjustmentAllowedCategoryRule(List<string> Allowed) : ITieredPositionAdjustmentRule
{
    public List<string> Check(TieredPositionAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionAdjustmentCodeLengthRule(int MaxLength) : ITieredPositionAdjustmentRule
{
    public List<string> Check(TieredPositionAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered position adjustment.</summary>
public sealed record TieredPositionAdjustmentValidator(List<ITieredPositionAdjustmentRule> Rules)
{
    public List<string> Validate(TieredPositionAdjustment item)
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

    public void RequireValid(TieredPositionAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPositionAdjustment> items)
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

/// <summary>Functions of tiered position adjustment rules.</summary>
public static class TieredPositionAdjustmentRules
{
    public static TieredPositionAdjustmentValidator DefaultTieredPositionAdjustmentValidator()
    {
        return new TieredPositionAdjustmentValidator(
            new List<ITieredPositionAdjustmentRule>
            {
                new TieredPositionAdjustmentRequiredContactEmailRule("tiered position adjustment needs an entry"),
                new TieredPositionAdjustmentAllowedCategoryRule(new List<string> { "manual", "lenient" }),
                new TieredPositionAdjustmentCodeLengthRule(80),
            });
    }
}
