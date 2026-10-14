namespace Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>A rule a pending visitor policy must pass. Check returns its problems, or none.</summary>
public interface IPendingVisitorPolicyRule
{
    List<string> Check(PendingVisitorPolicy item);
}

public sealed record PendingVisitorPolicyCodeLengthRule(int MaxLength) : IPendingVisitorPolicyRule
{
    public List<string> Check(PendingVisitorPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorPolicyLabelLengthRule(int MaxLength) : IPendingVisitorPolicyRule
{
    public List<string> Check(PendingVisitorPolicy item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingVisitorPolicyRequiredReviewerRule(string Message) : IPendingVisitorPolicyRule
{
    public List<string> Check(PendingVisitorPolicy item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending visitor policy.</summary>
public sealed record PendingVisitorPolicyValidator(List<IPendingVisitorPolicyRule> Rules)
{
    public List<string> Validate(PendingVisitorPolicy item)
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

    public void RequireValid(PendingVisitorPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingVisitorPolicy> items)
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

/// <summary>Functions of pending visitor policy rules.</summary>
public static class PendingVisitorPolicyRules
{
    public static PendingVisitorPolicyValidator DefaultPendingVisitorPolicyValidator()
    {
        return new PendingVisitorPolicyValidator(
            new List<IPendingVisitorPolicyRule>
            {
                new PendingVisitorPolicyCodeLengthRule(64),
                new PendingVisitorPolicyLabelLengthRule(32),
                new PendingVisitorPolicyRequiredReviewerRule("pending visitor policy needs an entry"),
            });
    }
}
