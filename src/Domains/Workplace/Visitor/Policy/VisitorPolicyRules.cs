namespace Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>A rule a visitor policy must pass. Check returns its problems, or none.</summary>
public interface IVisitorPolicyRule
{
    List<string> Check(VisitorPolicy item);
}

public sealed record VisitorPolicyUpdatedAtRangeRule(int Min, int Max) : IVisitorPolicyRule
{
    public List<string> Check(VisitorPolicy item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorPolicyCodePrefixRule(string Prefix) : IVisitorPolicyRule
{
    public List<string> Check(VisitorPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorPolicyCeilingRangeRule(double Min, double Max) : IVisitorPolicyRule
{
    public List<string> Check(VisitorPolicy item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorPolicyRequiredCodeRule(string Message) : IVisitorPolicyRule
{
    public List<string> Check(VisitorPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor policy.</summary>
public sealed record VisitorPolicyValidator(List<IVisitorPolicyRule> Rules)
{
    public List<string> Validate(VisitorPolicy item)
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

    public void RequireValid(VisitorPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorPolicy> items)
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

/// <summary>Functions of visitor policy rules.</summary>
public static class VisitorPolicyRules
{
    public static VisitorPolicyValidator DefaultVisitorPolicyValidator()
    {
        return new VisitorPolicyValidator(
            new List<IVisitorPolicyRule>
            {
                new VisitorPolicyUpdatedAtRangeRule(80, 80),
                new VisitorPolicyCodePrefixRule("X"),
                new VisitorPolicyCeilingRangeRule(0.4, 3415.6),
                new VisitorPolicyRequiredCodeRule("visitor policy needs this field"),
            });
    }
}
