namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient certification plan must pass. Check returns its problems, or none.</summary>
public interface ILenientCertificationPlanRule
{
    List<string> Check(LenientCertificationPlan item);
}

public sealed record LenientCertificationPlanMinimumRangeRule(double Min, double Max) : ILenientCertificationPlanRule
{
    public List<string> Check(LenientCertificationPlan item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientCertificationPlanAllowedLabelRule(List<string> Allowed) : ILenientCertificationPlanRule
{
    public List<string> Check(LenientCertificationPlan item)
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

public sealed record LenientCertificationPlanCodePrefixRule(string Prefix) : ILenientCertificationPlanRule
{
    public List<string> Check(LenientCertificationPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient certification plan.</summary>
public sealed record LenientCertificationPlanValidator(List<ILenientCertificationPlanRule> Rules)
{
    public List<string> Validate(LenientCertificationPlan item)
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

    public void RequireValid(LenientCertificationPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientCertificationPlan> items)
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

/// <summary>Functions of lenient certification plan rules.</summary>
public static class LenientCertificationPlanRules
{
    public static LenientCertificationPlanValidator DefaultLenientCertificationPlanValidator()
    {
        return new LenientCertificationPlanValidator(
            new List<ILenientCertificationPlanRule>
            {
                new LenientCertificationPlanMinimumRangeRule(1.9, 2140.4),
                new LenientCertificationPlanAllowedLabelRule(new List<string> { "primary", "weighted" }),
                new LenientCertificationPlanCodePrefixRule("OPS"),
            });
    }
}
