namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional return plan item must pass. Check returns its problems, or none.</summary>
public interface IRegionalReturnPlanItemRule
{
    List<string> Check(RegionalReturnPlanItem item);
}

public sealed record RegionalReturnPlanItemAllowedReviewerRule(List<string> Allowed) : IRegionalReturnPlanItemRule
{
    public List<string> Check(RegionalReturnPlanItem item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record RegionalReturnPlanItemCodePrefixRule(string Prefix) : IRegionalReturnPlanItemRule
{
    public List<string> Check(RegionalReturnPlanItem item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalReturnPlanItemScoreRangeRule(double Min, double Max) : IRegionalReturnPlanItemRule
{
    public List<string> Check(RegionalReturnPlanItem item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional return plan item.</summary>
public sealed record RegionalReturnPlanItemValidator(List<IRegionalReturnPlanItemRule> Rules)
{
    public List<string> Validate(RegionalReturnPlanItem item)
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

    public void RequireValid(RegionalReturnPlanItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalReturnPlanItem> items)
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

/// <summary>Functions of regional return plan item rules.</summary>
public static class RegionalReturnPlanItemRules
{
    public static RegionalReturnPlanItemValidator DefaultRegionalReturnPlanItemValidator()
    {
        return new RegionalReturnPlanItemValidator(
            new List<IRegionalReturnPlanItemRule>
            {
                new RegionalReturnPlanItemAllowedReviewerRule(new List<string> { "adaptive" }),
                new RegionalReturnPlanItemCodePrefixRule("REF"),
                new RegionalReturnPlanItemScoreRangeRule(8.9, 2388.8),
            });
    }
}
