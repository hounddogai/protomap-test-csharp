namespace Acme.Workforce.Assets.Assignment.Milestone;

/// <summary>A rule a assignment slot must pass. Check returns its problems, or none.</summary>
public interface IAssignmentSlotRule
{
    List<string> Check(AssignmentSlot item);
}

public sealed record AssignmentSlotBatchSizeRangeRule(int Min, int Max) : IAssignmentSlotRule
{
    public List<string> Check(AssignmentSlot item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentSlotRequiredLabelRule(string Message) : IAssignmentSlotRule
{
    public List<string> Check(AssignmentSlot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentSlotOwnerIdLengthRule(int MaxLength) : IAssignmentSlotRule
{
    public List<string> Check(AssignmentSlot item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentSlotCodePrefixRule(string Prefix) : IAssignmentSlotRule
{
    public List<string> Check(AssignmentSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentSlotPriorityRangeRule(int Min, int Max) : IAssignmentSlotRule
{
    public List<string> Check(AssignmentSlot item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment slot.</summary>
public sealed record AssignmentSlotValidator(List<IAssignmentSlotRule> Rules)
{
    public List<string> Validate(AssignmentSlot item)
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

    public void RequireValid(AssignmentSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentSlot> items)
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

/// <summary>Functions of assignment slot rules.</summary>
public static class AssignmentSlotRules
{
    public static AssignmentSlotValidator DefaultAssignmentSlotValidator()
    {
        return new AssignmentSlotValidator(
            new List<IAssignmentSlotRule>
            {
                new AssignmentSlotBatchSizeRangeRule(80, 3600),
                new AssignmentSlotRequiredLabelRule("assignment slot needs a value"),
                new AssignmentSlotOwnerIdLengthRule(32),
                new AssignmentSlotCodePrefixRule("ACM"),
                new AssignmentSlotPriorityRangeRule(80, 80),
            });
    }
}
