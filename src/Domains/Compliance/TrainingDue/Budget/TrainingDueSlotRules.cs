namespace Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>A rule a training due slot must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueSlotRule
{
    List<string> Check(TrainingDueSlot item);
}

public sealed record TrainingDueSlotLocationLengthRule(int MaxLength) : ITrainingDueSlotRule
{
    public List<string> Check(TrainingDueSlot item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueSlotRequiredLabelRule(string Message) : ITrainingDueSlotRule
{
    public List<string> Check(TrainingDueSlot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueSlotCodePrefixRule(string Prefix) : ITrainingDueSlotRule
{
    public List<string> Check(TrainingDueSlot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueSlotAttemptsRangeRule(int Min, int Max) : ITrainingDueSlotRule
{
    public List<string> Check(TrainingDueSlot item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueSlotCodeLengthRule(int MaxLength) : ITrainingDueSlotRule
{
    public List<string> Check(TrainingDueSlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due slot.</summary>
public sealed record TrainingDueSlotValidator(List<ITrainingDueSlotRule> Rules)
{
    public List<string> Validate(TrainingDueSlot item)
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

    public void RequireValid(TrainingDueSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueSlot> items)
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

/// <summary>Functions of training due slot rules.</summary>
public static class TrainingDueSlotRules
{
    public static TrainingDueSlotValidator DefaultTrainingDueSlotValidator()
    {
        return new TrainingDueSlotValidator(
            new List<ITrainingDueSlotRule>
            {
                new TrainingDueSlotLocationLengthRule(3600),
                new TrainingDueSlotRequiredLabelRule("training due slot needs this field"),
                new TrainingDueSlotCodePrefixRule("Q"),
                new TrainingDueSlotAttemptsRangeRule(255, 3600),
                new TrainingDueSlotCodeLengthRule(32),
            });
    }
}
