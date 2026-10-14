namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy training due batch must pass. Check returns its problems, or none.</summary>
public interface ILegacyTrainingDueBatchRule
{
    List<string> Check(LegacyTrainingDueBatch item);
}

public sealed record LegacyTrainingDueBatchRequiredNameRule(string Message) : ILegacyTrainingDueBatchRule
{
    public List<string> Check(LegacyTrainingDueBatch item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueBatchAllowedRegionRule(List<string> Allowed) : ILegacyTrainingDueBatchRule
{
    public List<string> Check(LegacyTrainingDueBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueBatchRetriesRangeRule(int Min, int Max) : ILegacyTrainingDueBatchRule
{
    public List<string> Check(LegacyTrainingDueBatch item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueBatchNameLengthRule(int MaxLength) : ILegacyTrainingDueBatchRule
{
    public List<string> Check(LegacyTrainingDueBatch item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueBatchRequiredSalaryBandRule(string Message) : ILegacyTrainingDueBatchRule
{
    public List<string> Check(LegacyTrainingDueBatch item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy training due batch.</summary>
public sealed record LegacyTrainingDueBatchValidator(List<ILegacyTrainingDueBatchRule> Rules)
{
    public List<string> Validate(LegacyTrainingDueBatch item)
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

    public void RequireValid(LegacyTrainingDueBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTrainingDueBatch> items)
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

/// <summary>Functions of legacy training due batch rules.</summary>
public static class LegacyTrainingDueBatchRules
{
    public static LegacyTrainingDueBatchValidator DefaultLegacyTrainingDueBatchValidator()
    {
        return new LegacyTrainingDueBatchValidator(
            new List<ILegacyTrainingDueBatchRule>
            {
                new LegacyTrainingDueBatchRequiredNameRule("legacy training due batch needs this field"),
                new LegacyTrainingDueBatchAllowedRegionRule(new List<string> { "adaptive", "strict", "manual" }),
                new LegacyTrainingDueBatchRetriesRangeRule(16, 255),
                new LegacyTrainingDueBatchNameLengthRule(64),
                new LegacyTrainingDueBatchRequiredSalaryBandRule("legacy training due batch needs an entry"),
            });
    }
}
