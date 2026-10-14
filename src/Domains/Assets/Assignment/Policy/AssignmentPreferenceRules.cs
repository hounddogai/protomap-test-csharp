namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment preference must pass. Check returns its problems, or none.</summary>
public interface IAssignmentPreferenceRule
{
    List<string> Check(AssignmentPreference item);
}

public sealed record AssignmentPreferenceAllowedCategoryRule(List<string> Allowed) : IAssignmentPreferenceRule
{
    public List<string> Check(AssignmentPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is stale" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPreferenceQuantityRangeRule(int Min, int Max) : IAssignmentPreferenceRule
{
    public List<string> Check(AssignmentPreference item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentPreferenceCodeLengthRule(int MaxLength) : IAssignmentPreferenceRule
{
    public List<string> Check(AssignmentPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPreferenceTitleLengthRule(int MaxLength) : IAssignmentPreferenceRule
{
    public List<string> Check(AssignmentPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment preference.</summary>
public sealed record AssignmentPreferenceValidator(List<IAssignmentPreferenceRule> Rules)
{
    public List<string> Validate(AssignmentPreference item)
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

    public void RequireValid(AssignmentPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentPreference> items)
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

/// <summary>Functions of assignment preference rules.</summary>
public static class AssignmentPreferenceRules
{
    public static AssignmentPreferenceValidator DefaultAssignmentPreferenceValidator()
    {
        return new AssignmentPreferenceValidator(
            new List<IAssignmentPreferenceRule>
            {
                new AssignmentPreferenceAllowedCategoryRule(new List<string>()),
                new AssignmentPreferenceQuantityRangeRule(64, 80),
                new AssignmentPreferenceCodeLengthRule(255),
                new AssignmentPreferenceTitleLengthRule(80),
            });
    }
}
