namespace Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>A rule a training due allocation must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueAllocationRule
{
    List<string> Check(TrainingDueAllocation item);
}

public sealed record TrainingDueAllocationRequiredNotesRule(string Message) : ITrainingDueAllocationRule
{
    public List<string> Check(TrainingDueAllocation item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueAllocationCodeLengthRule(int MaxLength) : ITrainingDueAllocationRule
{
    public List<string> Check(TrainingDueAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueAllocationNotesLengthRule(int MaxLength) : ITrainingDueAllocationRule
{
    public List<string> Check(TrainingDueAllocation item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueAllocationFteRangeRule(double Min, double Max) : ITrainingDueAllocationRule
{
    public List<string> Check(TrainingDueAllocation item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueAllocationRequiredChecksumRule(string Message) : ITrainingDueAllocationRule
{
    public List<string> Check(TrainingDueAllocation item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due allocation.</summary>
public sealed record TrainingDueAllocationValidator(List<ITrainingDueAllocationRule> Rules)
{
    public List<string> Validate(TrainingDueAllocation item)
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

    public void RequireValid(TrainingDueAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueAllocation> items)
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

/// <summary>Functions of training due allocation rules.</summary>
public static class TrainingDueAllocationRules
{
    public static TrainingDueAllocationValidator DefaultTrainingDueAllocationValidator()
    {
        return new TrainingDueAllocationValidator(
            new List<ITrainingDueAllocationRule>
            {
                new TrainingDueAllocationRequiredNotesRule("training due allocation needs an entry"),
                new TrainingDueAllocationCodeLengthRule(86400),
                new TrainingDueAllocationNotesLengthRule(120),
                new TrainingDueAllocationFteRangeRule(3.9, 3564.9),
                new TrainingDueAllocationRequiredChecksumRule("training due allocation needs this field"),
            });
    }
}
