namespace Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>A rule a weighted bonus pool checkpoint must pass. Check returns its problems, or none.</summary>
public interface IWeightedBonusPoolCheckpointRule
{
    List<string> Check(WeightedBonusPoolCheckpoint item);
}

public sealed record WeightedBonusPoolCheckpointLabelLengthRule(int MaxLength) : IWeightedBonusPoolCheckpointRule
{
    public List<string> Check(WeightedBonusPoolCheckpoint item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolCheckpointRequiredLabelRule(string Message) : IWeightedBonusPoolCheckpointRule
{
    public List<string> Check(WeightedBonusPoolCheckpoint item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolCheckpointExternalRefLengthRule(int MaxLength) : IWeightedBonusPoolCheckpointRule
{
    public List<string> Check(WeightedBonusPoolCheckpoint item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolCheckpointAmountRangeRule(double Min, double Max) : IWeightedBonusPoolCheckpointRule
{
    public List<string> Check(WeightedBonusPoolCheckpoint item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolCheckpointCodeLengthRule(int MaxLength) : IWeightedBonusPoolCheckpointRule
{
    public List<string> Check(WeightedBonusPoolCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted bonus pool checkpoint.</summary>
public sealed record WeightedBonusPoolCheckpointValidator(List<IWeightedBonusPoolCheckpointRule> Rules)
{
    public List<string> Validate(WeightedBonusPoolCheckpoint item)
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

    public void RequireValid(WeightedBonusPoolCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBonusPoolCheckpoint> items)
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

/// <summary>Functions of weighted bonus pool checkpoint rules.</summary>
public static class WeightedBonusPoolCheckpointRules
{
    public static WeightedBonusPoolCheckpointValidator DefaultWeightedBonusPoolCheckpointValidator()
    {
        return new WeightedBonusPoolCheckpointValidator(
            new List<IWeightedBonusPoolCheckpointRule>
            {
                new WeightedBonusPoolCheckpointLabelLengthRule(80),
                new WeightedBonusPoolCheckpointRequiredLabelRule("weighted bonus pool checkpoint needs a value"),
                new WeightedBonusPoolCheckpointExternalRefLengthRule(64),
                new WeightedBonusPoolCheckpointAmountRangeRule(7.9, 2954.5),
                new WeightedBonusPoolCheckpointCodeLengthRule(80),
            });
    }
}
