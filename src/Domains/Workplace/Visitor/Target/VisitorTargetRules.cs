namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor target must pass. Check returns its problems, or none.</summary>
public interface IVisitorTargetRule
{
    List<string> Check(VisitorTarget item);
}

public sealed record VisitorTargetAllowedLabelRule(List<string> Allowed) : IVisitorTargetRule
{
    public List<string> Check(VisitorTarget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record VisitorTargetCategoryLengthRule(int MaxLength) : IVisitorTargetRule
{
    public List<string> Check(VisitorTarget item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorTargetBadgeLengthRule(int MaxLength) : IVisitorTargetRule
{
    public List<string> Check(VisitorTarget item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor target.</summary>
public sealed record VisitorTargetValidator(List<IVisitorTargetRule> Rules)
{
    public List<string> Validate(VisitorTarget item)
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

    public void RequireValid(VisitorTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorTarget> items)
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

/// <summary>Functions of visitor target rules.</summary>
public static class VisitorTargetRules
{
    public static VisitorTargetValidator DefaultVisitorTargetValidator()
    {
        return new VisitorTargetValidator(
            new List<IVisitorTargetRule>
            {
                new VisitorTargetAllowedLabelRule(new List<string>()),
                new VisitorTargetCategoryLengthRule(3600),
                new VisitorTargetBadgeLengthRule(120),
            });
    }
}
