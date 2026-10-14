namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan profile must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanProfileRule
{
    List<string> Check(SuccessionPlanProfile item);
}

public sealed record SuccessionPlanProfileEndsAtRangeRule(int Min, int Max) : ISuccessionPlanProfileRule
{
    public List<string> Check(SuccessionPlanProfile item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SuccessionPlanProfileRequiredSegmentRule(string Message) : ISuccessionPlanProfileRule
{
    public List<string> Check(SuccessionPlanProfile item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanProfileCodePrefixRule(string Prefix) : ISuccessionPlanProfileRule
{
    public List<string> Check(SuccessionPlanProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanProfileAllowedSegmentRule(List<string> Allowed) : ISuccessionPlanProfileRule
{
    public List<string> Check(SuccessionPlanProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan profile.</summary>
public sealed record SuccessionPlanProfileValidator(List<ISuccessionPlanProfileRule> Rules)
{
    public List<string> Validate(SuccessionPlanProfile item)
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

    public void RequireValid(SuccessionPlanProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanProfile> items)
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

/// <summary>Functions of succession plan profile rules.</summary>
public static class SuccessionPlanProfileRules
{
    public static SuccessionPlanProfileValidator DefaultSuccessionPlanProfileValidator()
    {
        return new SuccessionPlanProfileValidator(
            new List<ISuccessionPlanProfileRule>
            {
                new SuccessionPlanProfileEndsAtRangeRule(80, 3600),
                new SuccessionPlanProfileRequiredSegmentRule("succession plan profile needs this field"),
                new SuccessionPlanProfileCodePrefixRule("ACM"),
                new SuccessionPlanProfileAllowedSegmentRule(new List<string> { "regional" }),
            });
    }
}
