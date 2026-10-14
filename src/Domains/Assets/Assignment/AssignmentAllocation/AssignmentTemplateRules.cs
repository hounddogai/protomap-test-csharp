namespace Acme.Workforce.Assets.Assignment.AssignmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a assignment template must pass. Check returns its problems, or none.</summary>
public interface IAssignmentTemplateRule
{
    List<string> Check(AssignmentTemplate item);
}

public sealed record AssignmentTemplateRequiredLabelRule(string Message) : IAssignmentTemplateRule
{
    public List<string> Check(AssignmentTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentTemplateNameLengthRule(int MaxLength) : IAssignmentTemplateRule
{
    public List<string> Check(AssignmentTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentTemplateRateRangeRule(double Min, double Max) : IAssignmentTemplateRule
{
    public List<string> Check(AssignmentTemplate item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentTemplateCodePrefixRule(string Prefix) : IAssignmentTemplateRule
{
    public List<string> Check(AssignmentTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentTemplateAllowedLabelRule(List<string> Allowed) : IAssignmentTemplateRule
{
    public List<string> Check(AssignmentTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment template.</summary>
public sealed record AssignmentTemplateValidator(List<IAssignmentTemplateRule> Rules)
{
    public List<string> Validate(AssignmentTemplate item)
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

    public void RequireValid(AssignmentTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentTemplate> items)
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

/// <summary>Functions of assignment template rules.</summary>
public static class AssignmentTemplateRules
{
    public static AssignmentTemplateValidator DefaultAssignmentTemplateValidator()
    {
        return new AssignmentTemplateValidator(
            new List<IAssignmentTemplateRule>
            {
                new AssignmentTemplateRequiredLabelRule("assignment template needs an entry"),
                new AssignmentTemplateNameLengthRule(3600),
                new AssignmentTemplateRateRangeRule(6.3, 4002.0),
                new AssignmentTemplateCodePrefixRule("REF"),
                new AssignmentTemplateAllowedLabelRule(new List<string> { "adaptive" }),
            });
    }
}
