namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a bonus pool draft must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolDraftRule
{
    List<string> Check(BonusPoolDraft item);
}

public sealed record BonusPoolDraftLabelLengthRule(int MaxLength) : IBonusPoolDraftRule
{
    public List<string> Check(BonusPoolDraft item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolDraftBatchSizeRangeRule(int Min, int Max) : IBonusPoolDraftRule
{
    public List<string> Check(BonusPoolDraft item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolDraftAllowedLabelRule(List<string> Allowed) : IBonusPoolDraftRule
{
    public List<string> Check(BonusPoolDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is expired" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolDraftTenureMonthsRangeRule(int Min, int Max) : IBonusPoolDraftRule
{
    public List<string> Check(BonusPoolDraft item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a bonus pool draft.</summary>
public sealed record BonusPoolDraftValidator(List<IBonusPoolDraftRule> Rules)
{
    public List<string> Validate(BonusPoolDraft item)
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

    public void RequireValid(BonusPoolDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolDraft> items)
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

/// <summary>Functions of bonus pool draft rules.</summary>
public static class BonusPoolDraftRules
{
    public static BonusPoolDraftValidator DefaultBonusPoolDraftValidator()
    {
        return new BonusPoolDraftValidator(
            new List<IBonusPoolDraftRule>
            {
                new BonusPoolDraftLabelLengthRule(16),
                new BonusPoolDraftBatchSizeRangeRule(64, 1000),
                new BonusPoolDraftAllowedLabelRule(new List<string> { "weighted", "seasonal" }),
                new BonusPoolDraftTenureMonthsRangeRule(16, 64),
            });
    }
}
