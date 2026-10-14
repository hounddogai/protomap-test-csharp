namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted adjustment draft must pass. Check returns its problems, or none.</summary>
public interface IWeightedAdjustmentDraftRule
{
    List<string> Check(WeightedAdjustmentDraft item);
}

public sealed record WeightedAdjustmentDraftAllowedSummaryRule(List<string> Allowed) : IWeightedAdjustmentDraftRule
{
    public List<string> Check(WeightedAdjustmentDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is expired" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAdjustmentDraftNotesLengthRule(int MaxLength) : IWeightedAdjustmentDraftRule
{
    public List<string> Check(WeightedAdjustmentDraft item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAdjustmentDraftApprovedGuardRule(double Limit) : IWeightedAdjustmentDraftRule
{
    public List<string> Check(WeightedAdjustmentDraft item)
    {
        if (item.Approved && item.Ceiling > Limit)
        {
            return new List<string> { $"ceiling {item.Ceiling} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted adjustment draft.</summary>
public sealed record WeightedAdjustmentDraftValidator(List<IWeightedAdjustmentDraftRule> Rules)
{
    public List<string> Validate(WeightedAdjustmentDraft item)
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

    public void RequireValid(WeightedAdjustmentDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAdjustmentDraft> items)
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

/// <summary>Functions of weighted adjustment draft rules.</summary>
public static class WeightedAdjustmentDraftRules
{
    public static WeightedAdjustmentDraftValidator DefaultWeightedAdjustmentDraftValidator()
    {
        return new WeightedAdjustmentDraftValidator(
            new List<IWeightedAdjustmentDraftRule>
            {
                new WeightedAdjustmentDraftAllowedSummaryRule(new List<string>()),
                new WeightedAdjustmentDraftNotesLengthRule(32),
                new WeightedAdjustmentDraftApprovedGuardRule(2.3),
            });
    }
}
