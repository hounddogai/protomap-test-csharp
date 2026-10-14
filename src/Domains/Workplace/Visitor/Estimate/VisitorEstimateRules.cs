namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a visitor estimate must pass. Check returns its problems, or none.</summary>
public interface IVisitorEstimateRule
{
    List<string> Check(VisitorEstimate item);
}

public sealed record VisitorEstimateRequiredCodeRule(string Message) : IVisitorEstimateRule
{
    public List<string> Check(VisitorEstimate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorEstimateRequiredLabelRule(string Message) : IVisitorEstimateRule
{
    public List<string> Check(VisitorEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorEstimateAllowedJobCodeRule(List<string> Allowed) : IVisitorEstimateRule
{
    public List<string> Check(VisitorEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor estimate.</summary>
public sealed record VisitorEstimateValidator(List<IVisitorEstimateRule> Rules)
{
    public List<string> Validate(VisitorEstimate item)
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

    public void RequireValid(VisitorEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorEstimate> items)
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

/// <summary>Functions of visitor estimate rules.</summary>
public static class VisitorEstimateRules
{
    public static VisitorEstimateValidator DefaultVisitorEstimateValidator()
    {
        return new VisitorEstimateValidator(
            new List<IVisitorEstimateRule>
            {
                new VisitorEstimateRequiredCodeRule("visitor estimate needs a value"),
                new VisitorEstimateRequiredLabelRule("visitor estimate needs an entry"),
                new VisitorEstimateAllowedJobCodeRule(new List<string>()),
            });
    }
}
