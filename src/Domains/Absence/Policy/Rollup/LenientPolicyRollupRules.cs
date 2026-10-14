namespace Acme.Workforce.Absence.Policy.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient policy rollup must pass. Check returns its problems, or none.</summary>
public interface ILenientPolicyRollupRule
{
    List<string> Check(LenientPolicyRollup item);
}

public sealed record LenientPolicyRollupRequiredNameRule(string Message) : ILenientPolicyRollupRule
{
    public List<string> Check(LenientPolicyRollup item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyRollupRequiredLabelRule(string Message) : ILenientPolicyRollupRule
{
    public List<string> Check(LenientPolicyRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPolicyRollupAllowedDepartmentRule(List<string> Allowed) : ILenientPolicyRollupRule
{
    public List<string> Check(LenientPolicyRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient policy rollup.</summary>
public sealed record LenientPolicyRollupValidator(List<ILenientPolicyRollupRule> Rules)
{
    public List<string> Validate(LenientPolicyRollup item)
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

    public void RequireValid(LenientPolicyRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPolicyRollup> items)
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

/// <summary>Functions of lenient policy rollup rules.</summary>
public static class LenientPolicyRollupRules
{
    public static LenientPolicyRollupValidator DefaultLenientPolicyRollupValidator()
    {
        return new LenientPolicyRollupValidator(
            new List<ILenientPolicyRollupRule>
            {
                new LenientPolicyRollupRequiredNameRule("lenient policy rollup needs this field"),
                new LenientPolicyRollupRequiredLabelRule("lenient policy rollup needs an entry"),
                new LenientPolicyRollupAllowedDepartmentRule(new List<string> { "scheduled", "default" }),
            });
    }
}
