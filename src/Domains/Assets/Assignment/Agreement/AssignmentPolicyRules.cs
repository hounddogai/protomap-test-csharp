namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment policy must pass. Check returns its problems, or none.</summary>
public interface IAssignmentPolicyRule
{
    List<string> Check(AssignmentPolicy item);
}

public sealed record AssignmentPolicyTitleLengthRule(int MaxLength) : IAssignmentPolicyRule
{
    public List<string> Check(AssignmentPolicy item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPolicyRequiredTitleRule(string Message) : IAssignmentPolicyRule
{
    public List<string> Check(AssignmentPolicy item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPolicyAllowedTitleRule(List<string> Allowed) : IAssignmentPolicyRule
{
    public List<string> Check(AssignmentPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentPolicyDiscountRangeRule(double Min, double Max) : IAssignmentPolicyRule
{
    public List<string> Check(AssignmentPolicy item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment policy.</summary>
public sealed record AssignmentPolicyValidator(List<IAssignmentPolicyRule> Rules)
{
    public List<string> Validate(AssignmentPolicy item)
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

    public void RequireValid(AssignmentPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentPolicy> items)
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

/// <summary>Functions of assignment policy rules.</summary>
public static class AssignmentPolicyRules
{
    public static AssignmentPolicyValidator DefaultAssignmentPolicyValidator()
    {
        return new AssignmentPolicyValidator(
            new List<IAssignmentPolicyRule>
            {
                new AssignmentPolicyTitleLengthRule(80),
                new AssignmentPolicyRequiredTitleRule("assignment policy needs a value"),
                new AssignmentPolicyAllowedTitleRule(new List<string> { "global", "manual", "batched" }),
                new AssignmentPolicyDiscountRangeRule(6.8, 254.5),
            });
    }
}
