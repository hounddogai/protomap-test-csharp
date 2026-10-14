namespace Acme.Workforce.Assets.Assignment.Entry;

/// <summary>A rule a assignment baseline must pass. Check returns its problems, or none.</summary>
public interface IAssignmentBaselineRule
{
    List<string> Check(AssignmentBaseline item);
}

public sealed record AssignmentBaselineCodeLengthRule(int MaxLength) : IAssignmentBaselineRule
{
    public List<string> Check(AssignmentBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentBaselineWeightRangeRule(double Min, double Max) : IAssignmentBaselineRule
{
    public List<string> Check(AssignmentBaseline item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentBaselineRatioRangeRule(double Min, double Max) : IAssignmentBaselineRule
{
    public List<string> Check(AssignmentBaseline item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentBaselineChecksumLengthRule(int MaxLength) : IAssignmentBaselineRule
{
    public List<string> Check(AssignmentBaseline item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentBaselineAmountRangeRule(double Min, double Max) : IAssignmentBaselineRule
{
    public List<string> Check(AssignmentBaseline item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentBaselineContactEmailLengthRule(int MaxLength) : IAssignmentBaselineRule
{
    public List<string> Check(AssignmentBaseline item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment baseline.</summary>
public sealed record AssignmentBaselineValidator(List<IAssignmentBaselineRule> Rules)
{
    public List<string> Validate(AssignmentBaseline item)
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

    public void RequireValid(AssignmentBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentBaseline> items)
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

/// <summary>Functions of assignment baseline rules.</summary>
public static class AssignmentBaselineRules
{
    public static AssignmentBaselineValidator DefaultAssignmentBaselineValidator()
    {
        return new AssignmentBaselineValidator(
            new List<IAssignmentBaselineRule>
            {
                new AssignmentBaselineCodeLengthRule(3600),
                new AssignmentBaselineWeightRangeRule(6.7, 1585.2),
                new AssignmentBaselineRatioRangeRule(9.6, 1621.4),
                new AssignmentBaselineChecksumLengthRule(16),
                new AssignmentBaselineAmountRangeRule(6.8, 516.8),
                new AssignmentBaselineContactEmailLengthRule(120),
            });
    }
}
