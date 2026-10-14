namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>A rule a weighted reorg claim must pass. Check returns its problems, or none.</summary>
public interface IWeightedReorgClaimRule
{
    List<string> Check(WeightedReorgClaim item);
}

public sealed record WeightedReorgClaimSequenceRangeRule(int Min, int Max) : IWeightedReorgClaimRule
{
    public List<string> Check(WeightedReorgClaim item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedReorgClaimCodeLengthRule(int MaxLength) : IWeightedReorgClaimRule
{
    public List<string> Check(WeightedReorgClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedReorgClaimNameLengthRule(int MaxLength) : IWeightedReorgClaimRule
{
    public List<string> Check(WeightedReorgClaim item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted reorg claim.</summary>
public sealed record WeightedReorgClaimValidator(List<IWeightedReorgClaimRule> Rules)
{
    public List<string> Validate(WeightedReorgClaim item)
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

    public void RequireValid(WeightedReorgClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedReorgClaim> items)
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

/// <summary>Functions of weighted reorg claim rules.</summary>
public static class WeightedReorgClaimRules
{
    public static WeightedReorgClaimValidator DefaultWeightedReorgClaimValidator()
    {
        return new WeightedReorgClaimValidator(
            new List<IWeightedReorgClaimRule>
            {
                new WeightedReorgClaimSequenceRangeRule(16, 120),
                new WeightedReorgClaimCodeLengthRule(1000),
                new WeightedReorgClaimNameLengthRule(80),
            });
    }
}
