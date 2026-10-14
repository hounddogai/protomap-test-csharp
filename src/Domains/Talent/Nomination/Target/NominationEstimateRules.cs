namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination estimate must pass. Check returns its problems, or none.</summary>
public interface INominationEstimateRule
{
    List<string> Check(NominationEstimate item);
}

public sealed record NominationEstimateRequiredDepartmentRule(string Message) : INominationEstimateRule
{
    public List<string> Check(NominationEstimate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record NominationEstimateDepartmentLengthRule(int MaxLength) : INominationEstimateRule
{
    public List<string> Check(NominationEstimate item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationEstimateAllowedCodeRule(List<string> Allowed) : INominationEstimateRule
{
    public List<string> Check(NominationEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination estimate.</summary>
public sealed record NominationEstimateValidator(List<INominationEstimateRule> Rules)
{
    public List<string> Validate(NominationEstimate item)
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

    public void RequireValid(NominationEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationEstimate> items)
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

/// <summary>Functions of nomination estimate rules.</summary>
public static class NominationEstimateRules
{
    public static NominationEstimateValidator DefaultNominationEstimateValidator()
    {
        return new NominationEstimateValidator(
            new List<INominationEstimateRule>
            {
                new NominationEstimateRequiredDepartmentRule("nomination estimate needs this field"),
                new NominationEstimateDepartmentLengthRule(64),
                new NominationEstimateAllowedCodeRule(new List<string> { "regional" }),
            });
    }
}
