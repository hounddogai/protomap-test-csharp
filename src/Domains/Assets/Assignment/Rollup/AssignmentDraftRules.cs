namespace Acme.Workforce.Assets.Assignment.Rollup;

/// <summary>A rule a assignment draft must pass. Check returns its problems, or none.</summary>
public interface IAssignmentDraftRule
{
    List<string> Check(AssignmentDraft item);
}

public sealed record AssignmentDraftExpiresAtRangeRule(int Min, int Max) : IAssignmentDraftRule
{
    public List<string> Check(AssignmentDraft item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDraftRequiredLabelRule(string Message) : IAssignmentDraftRule
{
    public List<string> Check(AssignmentDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDraftCodePrefixRule(string Prefix) : IAssignmentDraftRule
{
    public List<string> Check(AssignmentDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDraftLabelLengthRule(int MaxLength) : IAssignmentDraftRule
{
    public List<string> Check(AssignmentDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentDraftRatingRangeRule(double Min, double Max) : IAssignmentDraftRule
{
    public List<string> Check(AssignmentDraft item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a assignment draft.</summary>
public sealed record AssignmentDraftValidator(List<IAssignmentDraftRule> Rules)
{
    public List<string> Validate(AssignmentDraft item)
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

    public void RequireValid(AssignmentDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentDraft> items)
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

/// <summary>Functions of assignment draft rules.</summary>
public static class AssignmentDraftRules
{
    public static AssignmentDraftValidator DefaultAssignmentDraftValidator()
    {
        return new AssignmentDraftValidator(
            new List<IAssignmentDraftRule>
            {
                new AssignmentDraftExpiresAtRangeRule(32, 64),
                new AssignmentDraftRequiredLabelRule("assignment draft needs an entry"),
                new AssignmentDraftCodePrefixRule("Q"),
                new AssignmentDraftLabelLengthRule(16),
                new AssignmentDraftRatingRangeRule(3.9, 3401.2),
            });
    }
}
