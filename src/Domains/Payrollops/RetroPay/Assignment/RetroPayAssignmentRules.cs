namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

/// <summary>A rule a retro pay assignment must pass. Check returns its problems, or none.</summary>
public interface IRetroPayAssignmentRule
{
    List<string> Check(RetroPayAssignment item);
}

public sealed record RetroPayAssignmentCodePrefixRule(string Prefix) : IRetroPayAssignmentRule
{
    public List<string> Check(RetroPayAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAssignmentReviewerLengthRule(int MaxLength) : IRetroPayAssignmentRule
{
    public List<string> Check(RetroPayAssignment item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAssignmentRequiredReviewerRule(string Message) : IRetroPayAssignmentRule
{
    public List<string> Check(RetroPayAssignment item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayAssignmentRequiredLabelRule(string Message) : IRetroPayAssignmentRule
{
    public List<string> Check(RetroPayAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay assignment.</summary>
public sealed record RetroPayAssignmentValidator(List<IRetroPayAssignmentRule> Rules)
{
    public List<string> Validate(RetroPayAssignment item)
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

    public void RequireValid(RetroPayAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayAssignment> items)
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

/// <summary>Functions of retro pay assignment rules.</summary>
public static class RetroPayAssignmentRules
{
    public static RetroPayAssignmentValidator DefaultRetroPayAssignmentValidator()
    {
        return new RetroPayAssignmentValidator(
            new List<IRetroPayAssignmentRule>
            {
                new RetroPayAssignmentCodePrefixRule("ACM"),
                new RetroPayAssignmentReviewerLengthRule(64),
                new RetroPayAssignmentRequiredReviewerRule("retro pay assignment needs an entry"),
                new RetroPayAssignmentRequiredLabelRule("retro pay assignment needs an entry"),
            });
    }
}
