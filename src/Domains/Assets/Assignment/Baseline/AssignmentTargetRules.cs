namespace Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>A rule a assignment target must pass. Check returns its problems, or none.</summary>
public interface IAssignmentTargetRule
{
    List<string> Check(AssignmentTarget item);
}

public sealed record AssignmentTargetBudgetRangeRule(double Min, double Max) : IAssignmentTargetRule
{
    public List<string> Check(AssignmentTarget item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentTargetCodePrefixRule(string Prefix) : IAssignmentTargetRule
{
    public List<string> Check(AssignmentTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentTargetJobCodeLengthRule(int MaxLength) : IAssignmentTargetRule
{
    public List<string> Check(AssignmentTarget item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentTargetRequiredOwnerIdRule(string Message) : IAssignmentTargetRule
{
    public List<string> Check(AssignmentTarget item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentTargetRequiredCodeRule(string Message) : IAssignmentTargetRule
{
    public List<string> Check(AssignmentTarget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment target.</summary>
public sealed record AssignmentTargetValidator(List<IAssignmentTargetRule> Rules)
{
    public List<string> Validate(AssignmentTarget item)
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

    public void RequireValid(AssignmentTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentTarget> items)
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

/// <summary>Functions of assignment target rules.</summary>
public static class AssignmentTargetRules
{
    public static AssignmentTargetValidator DefaultAssignmentTargetValidator()
    {
        return new AssignmentTargetValidator(
            new List<IAssignmentTargetRule>
            {
                new AssignmentTargetBudgetRangeRule(1.9, 2433.0),
                new AssignmentTargetCodePrefixRule("Q"),
                new AssignmentTargetJobCodeLengthRule(255),
                new AssignmentTargetRequiredOwnerIdRule("assignment target needs this field"),
                new AssignmentTargetRequiredCodeRule("assignment target needs an entry"),
            });
    }
}
