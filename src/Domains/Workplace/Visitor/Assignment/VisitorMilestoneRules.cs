namespace Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>A rule a visitor milestone must pass. Check returns its problems, or none.</summary>
public interface IVisitorMilestoneRule
{
    List<string> Check(VisitorMilestone item);
}

public sealed record VisitorMilestoneCostRangeRule(double Min, double Max) : IVisitorMilestoneRule
{
    public List<string> Check(VisitorMilestone item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorMilestoneMaximumRangeRule(double Min, double Max) : IVisitorMilestoneRule
{
    public List<string> Check(VisitorMilestone item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorMilestoneRequiredCodeRule(string Message) : IVisitorMilestoneRule
{
    public List<string> Check(VisitorMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorMilestoneRequiredSummaryRule(string Message) : IVisitorMilestoneRule
{
    public List<string> Check(VisitorMilestone item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorMilestoneCodePrefixRule(string Prefix) : IVisitorMilestoneRule
{
    public List<string> Check(VisitorMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor milestone.</summary>
public sealed record VisitorMilestoneValidator(List<IVisitorMilestoneRule> Rules)
{
    public List<string> Validate(VisitorMilestone item)
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

    public void RequireValid(VisitorMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorMilestone> items)
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

/// <summary>Functions of visitor milestone rules.</summary>
public static class VisitorMilestoneRules
{
    public static VisitorMilestoneValidator DefaultVisitorMilestoneValidator()
    {
        return new VisitorMilestoneValidator(
            new List<IVisitorMilestoneRule>
            {
                new VisitorMilestoneCostRangeRule(7.9, 2994.8),
                new VisitorMilestoneMaximumRangeRule(9.2, 2661.0),
                new VisitorMilestoneRequiredCodeRule("visitor milestone needs an entry"),
                new VisitorMilestoneRequiredSummaryRule("visitor milestone needs an entry"),
                new VisitorMilestoneCodePrefixRule("OPS"),
            });
    }
}
