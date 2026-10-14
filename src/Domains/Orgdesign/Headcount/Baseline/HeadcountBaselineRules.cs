namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount baseline must pass. Check returns its problems, or none.</summary>
public interface IHeadcountBaselineRule
{
    List<string> Check(HeadcountBaseline item);
}

public sealed record HeadcountBaselineCodePrefixRule(string Prefix) : IHeadcountBaselineRule
{
    public List<string> Check(HeadcountBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBaselineAllowedReviewerRule(List<string> Allowed) : IHeadcountBaselineRule
{
    public List<string> Check(HeadcountBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is expired" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountBaselineRequiredNameRule(string Message) : IHeadcountBaselineRule
{
    public List<string> Check(HeadcountBaseline item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount baseline.</summary>
public sealed record HeadcountBaselineValidator(List<IHeadcountBaselineRule> Rules)
{
    public List<string> Validate(HeadcountBaseline item)
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

    public void RequireValid(HeadcountBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountBaseline> items)
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

/// <summary>Functions of headcount baseline rules.</summary>
public static class HeadcountBaselineRules
{
    public static HeadcountBaselineValidator DefaultHeadcountBaselineValidator()
    {
        return new HeadcountBaselineValidator(
            new List<IHeadcountBaselineRule>
            {
                new HeadcountBaselineCodePrefixRule("OPS"),
                new HeadcountBaselineAllowedReviewerRule(new List<string> { "legacy", "rolling", "strict" }),
                new HeadcountBaselineRequiredNameRule("headcount baseline needs a value"),
            });
    }
}
