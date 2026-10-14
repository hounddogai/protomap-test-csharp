namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted training due audit must pass. Check returns its problems, or none.</summary>
public interface IWeightedTrainingDueAuditRule
{
    List<string> Check(WeightedTrainingDueAudit item);
}

public sealed record WeightedTrainingDueAuditCodePrefixRule(string Prefix) : IWeightedTrainingDueAuditRule
{
    public List<string> Check(WeightedTrainingDueAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAuditRequiredTitleRule(string Message) : IWeightedTrainingDueAuditRule
{
    public List<string> Check(WeightedTrainingDueAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAuditAllowedCostCenterRule(List<string> Allowed) : IWeightedTrainingDueAuditRule
{
    public List<string> Check(WeightedTrainingDueAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAuditRequiredCostCenterRule(string Message) : IWeightedTrainingDueAuditRule
{
    public List<string> Check(WeightedTrainingDueAudit item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAuditRequiredCodeRule(string Message) : IWeightedTrainingDueAuditRule
{
    public List<string> Check(WeightedTrainingDueAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedTrainingDueAuditCostCenterLengthRule(int MaxLength) : IWeightedTrainingDueAuditRule
{
    public List<string> Check(WeightedTrainingDueAudit item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted training due audit.</summary>
public sealed record WeightedTrainingDueAuditValidator(List<IWeightedTrainingDueAuditRule> Rules)
{
    public List<string> Validate(WeightedTrainingDueAudit item)
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

    public void RequireValid(WeightedTrainingDueAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedTrainingDueAudit> items)
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

/// <summary>Functions of weighted training due audit rules.</summary>
public static class WeightedTrainingDueAuditRules
{
    public static WeightedTrainingDueAuditValidator DefaultWeightedTrainingDueAuditValidator()
    {
        return new WeightedTrainingDueAuditValidator(
            new List<IWeightedTrainingDueAuditRule>
            {
                new WeightedTrainingDueAuditCodePrefixRule("OPS"),
                new WeightedTrainingDueAuditRequiredTitleRule("weighted training due audit needs an entry"),
                new WeightedTrainingDueAuditAllowedCostCenterRule(new List<string>()),
                new WeightedTrainingDueAuditRequiredCostCenterRule("weighted training due audit needs a value"),
                new WeightedTrainingDueAuditRequiredCodeRule("weighted training due audit needs a value"),
                new WeightedTrainingDueAuditCostCenterLengthRule(86400),
            });
    }
}
