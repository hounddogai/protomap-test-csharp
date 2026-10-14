namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped desk adjustment must pass. Check returns its problems, or none.</summary>
public interface ICappedDeskAdjustmentRule
{
    List<string> Check(CappedDeskAdjustment item);
}

public sealed record CappedDeskAdjustmentRatioRangeRule(double Min, double Max) : ICappedDeskAdjustmentRule
{
    public List<string> Check(CappedDeskAdjustment item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskAdjustmentRequiredDescriptionRule(string Message) : ICappedDeskAdjustmentRule
{
    public List<string> Check(CappedDeskAdjustment item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskAdjustmentRequiredBadgeRule(string Message) : ICappedDeskAdjustmentRule
{
    public List<string> Check(CappedDeskAdjustment item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskAdjustmentAllowedCategoryRule(List<string> Allowed) : ICappedDeskAdjustmentRule
{
    public List<string> Check(CappedDeskAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeskAdjustmentBadgeLengthRule(int MaxLength) : ICappedDeskAdjustmentRule
{
    public List<string> Check(CappedDeskAdjustment item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped desk adjustment.</summary>
public sealed record CappedDeskAdjustmentValidator(List<ICappedDeskAdjustmentRule> Rules)
{
    public List<string> Validate(CappedDeskAdjustment item)
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

    public void RequireValid(CappedDeskAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedDeskAdjustment> items)
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

/// <summary>Functions of capped desk adjustment rules.</summary>
public static class CappedDeskAdjustmentRules
{
    public static CappedDeskAdjustmentValidator DefaultCappedDeskAdjustmentValidator()
    {
        return new CappedDeskAdjustmentValidator(
            new List<ICappedDeskAdjustmentRule>
            {
                new CappedDeskAdjustmentRatioRangeRule(0.4, 943.6),
                new CappedDeskAdjustmentRequiredDescriptionRule("capped desk adjustment needs a value"),
                new CappedDeskAdjustmentRequiredBadgeRule("capped desk adjustment needs an entry"),
                new CappedDeskAdjustmentAllowedCategoryRule(new List<string> { "capped" }),
                new CappedDeskAdjustmentBadgeLengthRule(64),
            });
    }
}
