namespace Acme.Workforce.Assets.Assignment.Quota;

/// <summary>A rule a assignment quota must pass. Check returns its problems, or none.</summary>
public interface IAssignmentQuotaRule
{
    List<string> Check(AssignmentQuota item);
}

public sealed record AssignmentQuotaNotesLengthRule(int MaxLength) : IAssignmentQuotaRule
{
    public List<string> Check(AssignmentQuota item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentQuotaCodeLengthRule(int MaxLength) : IAssignmentQuotaRule
{
    public List<string> Check(AssignmentQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentQuotaRetriesRangeRule(int Min, int Max) : IAssignmentQuotaRule
{
    public List<string> Check(AssignmentQuota item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentQuotaRequiredLabelRule(string Message) : IAssignmentQuotaRule
{
    public List<string> Check(AssignmentQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentQuotaAmountRangeRule(double Min, double Max) : IAssignmentQuotaRule
{
    public List<string> Check(AssignmentQuota item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentQuotaRequiredNotesRule(string Message) : IAssignmentQuotaRule
{
    public List<string> Check(AssignmentQuota item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment quota.</summary>
public sealed record AssignmentQuotaValidator(List<IAssignmentQuotaRule> Rules)
{
    public List<string> Validate(AssignmentQuota item)
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

    public void RequireValid(AssignmentQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentQuota> items)
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

/// <summary>Functions of assignment quota rules.</summary>
public static class AssignmentQuotaRules
{
    public static AssignmentQuotaValidator DefaultAssignmentQuotaValidator()
    {
        return new AssignmentQuotaValidator(
            new List<IAssignmentQuotaRule>
            {
                new AssignmentQuotaNotesLengthRule(255),
                new AssignmentQuotaCodeLengthRule(64),
                new AssignmentQuotaRetriesRangeRule(32, 32),
                new AssignmentQuotaRequiredLabelRule("assignment quota needs a value"),
                new AssignmentQuotaAmountRangeRule(9.2, 3198.2),
                new AssignmentQuotaRequiredNotesRule("assignment quota needs an entry"),
            });
    }
}
