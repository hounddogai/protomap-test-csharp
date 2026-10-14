namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

/// <summary>A rule a headcount estimate must pass. Check returns its problems, or none.</summary>
public interface IHeadcountEstimateRule
{
    List<string> Check(HeadcountEstimate item);
}

public sealed record HeadcountEstimateRequiredLabelRule(string Message) : IHeadcountEstimateRule
{
    public List<string> Check(HeadcountEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountEstimateCodePrefixRule(string Prefix) : IHeadcountEstimateRule
{
    public List<string> Check(HeadcountEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountEstimateRequiredCodeRule(string Message) : IHeadcountEstimateRule
{
    public List<string> Check(HeadcountEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HeadcountEstimateApprovedGuardRule(int Limit) : IHeadcountEstimateRule
{
    public List<string> Check(HeadcountEstimate item)
    {
        if (item.Approved && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount estimate.</summary>
public sealed record HeadcountEstimateValidator(List<IHeadcountEstimateRule> Rules)
{
    public List<string> Validate(HeadcountEstimate item)
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

    public void RequireValid(HeadcountEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountEstimate> items)
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

/// <summary>Functions of headcount estimate rules.</summary>
public static class HeadcountEstimateRules
{
    public static HeadcountEstimateValidator DefaultHeadcountEstimateValidator()
    {
        return new HeadcountEstimateValidator(
            new List<IHeadcountEstimateRule>
            {
                new HeadcountEstimateRequiredLabelRule("headcount estimate needs an entry"),
                new HeadcountEstimateCodePrefixRule("Q"),
                new HeadcountEstimateRequiredCodeRule("headcount estimate needs an entry"),
                new HeadcountEstimateApprovedGuardRule(255),
            });
    }
}
