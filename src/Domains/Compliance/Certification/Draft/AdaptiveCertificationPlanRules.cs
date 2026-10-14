namespace Acme.Workforce.Compliance.Certification.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive certification plan must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveCertificationPlanRule
{
    List<string> Check(AdaptiveCertificationPlan item);
}

public sealed record AdaptiveCertificationPlanRevisionRangeRule(int Min, int Max) : IAdaptiveCertificationPlanRule
{
    public List<string> Check(AdaptiveCertificationPlan item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveCertificationPlanCodeLengthRule(int MaxLength) : IAdaptiveCertificationPlanRule
{
    public List<string> Check(AdaptiveCertificationPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCertificationPlanAllowedCodeRule(List<string> Allowed) : IAdaptiveCertificationPlanRule
{
    public List<string> Check(AdaptiveCertificationPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveCertificationPlanCapacityRangeRule(int Min, int Max) : IAdaptiveCertificationPlanRule
{
    public List<string> Check(AdaptiveCertificationPlan item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive certification plan.</summary>
public sealed record AdaptiveCertificationPlanValidator(List<IAdaptiveCertificationPlanRule> Rules)
{
    public List<string> Validate(AdaptiveCertificationPlan item)
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

    public void RequireValid(AdaptiveCertificationPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveCertificationPlan> items)
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

/// <summary>Functions of adaptive certification plan rules.</summary>
public static class AdaptiveCertificationPlanRules
{
    public static AdaptiveCertificationPlanValidator DefaultAdaptiveCertificationPlanValidator()
    {
        return new AdaptiveCertificationPlanValidator(
            new List<IAdaptiveCertificationPlanRule>
            {
                new AdaptiveCertificationPlanRevisionRangeRule(80, 3600),
                new AdaptiveCertificationPlanCodeLengthRule(86400),
                new AdaptiveCertificationPlanAllowedCodeRule(new List<string> { "capped" }),
                new AdaptiveCertificationPlanCapacityRangeRule(1000, 1000),
            });
    }
}
