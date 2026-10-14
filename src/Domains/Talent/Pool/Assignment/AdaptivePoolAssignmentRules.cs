namespace Acme.Workforce.Talent.Pool.Assignment;

/// <summary>A rule a adaptive pool assignment must pass. Check returns its problems, or none.</summary>
public interface IAdaptivePoolAssignmentRule
{
    List<string> Check(AdaptivePoolAssignment item);
}

public sealed record AdaptivePoolAssignmentRequiredLabelRule(string Message) : IAdaptivePoolAssignmentRule
{
    public List<string> Check(AdaptivePoolAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolAssignmentRequiredRegionRule(string Message) : IAdaptivePoolAssignmentRule
{
    public List<string> Check(AdaptivePoolAssignment item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolAssignmentLabelLengthRule(int MaxLength) : IAdaptivePoolAssignmentRule
{
    public List<string> Check(AdaptivePoolAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptivePoolAssignmentCodeLengthRule(int MaxLength) : IAdaptivePoolAssignmentRule
{
    public List<string> Check(AdaptivePoolAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive pool assignment.</summary>
public sealed record AdaptivePoolAssignmentValidator(List<IAdaptivePoolAssignmentRule> Rules)
{
    public List<string> Validate(AdaptivePoolAssignment item)
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

    public void RequireValid(AdaptivePoolAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptivePoolAssignment> items)
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

/// <summary>Functions of adaptive pool assignment rules.</summary>
public static class AdaptivePoolAssignmentRules
{
    public static AdaptivePoolAssignmentValidator DefaultAdaptivePoolAssignmentValidator()
    {
        return new AdaptivePoolAssignmentValidator(
            new List<IAdaptivePoolAssignmentRule>
            {
                new AdaptivePoolAssignmentRequiredLabelRule("adaptive pool assignment needs this field"),
                new AdaptivePoolAssignmentRequiredRegionRule("adaptive pool assignment needs an entry"),
                new AdaptivePoolAssignmentLabelLengthRule(255),
                new AdaptivePoolAssignmentCodeLengthRule(3600),
            });
    }
}
