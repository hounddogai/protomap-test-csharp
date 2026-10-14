namespace Acme.Workforce.Assets.Assignment.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary assignment tier must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAssignmentTierRule
{
    List<string> Check(PrimaryAssignmentTier item);
}

public sealed record PrimaryAssignmentTierAllowedLabelRule(List<string> Allowed) : IPrimaryAssignmentTierRule
{
    public List<string> Check(PrimaryAssignmentTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAssignmentTierScoreRangeRule(double Min, double Max) : IPrimaryAssignmentTierRule
{
    public List<string> Check(PrimaryAssignmentTier item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryAssignmentTierAmountRangeRule(double Min, double Max) : IPrimaryAssignmentTierRule
{
    public List<string> Check(PrimaryAssignmentTier item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryAssignmentTierLabelLengthRule(int MaxLength) : IPrimaryAssignmentTierRule
{
    public List<string> Check(PrimaryAssignmentTier item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary assignment tier.</summary>
public sealed record PrimaryAssignmentTierValidator(List<IPrimaryAssignmentTierRule> Rules)
{
    public List<string> Validate(PrimaryAssignmentTier item)
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

    public void RequireValid(PrimaryAssignmentTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAssignmentTier> items)
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

/// <summary>Functions of primary assignment tier rules.</summary>
public static class PrimaryAssignmentTierRules
{
    public static PrimaryAssignmentTierValidator DefaultPrimaryAssignmentTierValidator()
    {
        return new PrimaryAssignmentTierValidator(
            new List<IPrimaryAssignmentTierRule>
            {
                new PrimaryAssignmentTierAllowedLabelRule(new List<string> { "default", "primary" }),
                new PrimaryAssignmentTierScoreRangeRule(7.9, 1020.5),
                new PrimaryAssignmentTierAmountRangeRule(1.3, 2504.3),
                new PrimaryAssignmentTierLabelLengthRule(80),
            });
    }
}
