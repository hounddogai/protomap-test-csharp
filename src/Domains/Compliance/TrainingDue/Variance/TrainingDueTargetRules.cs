namespace Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>A rule a training due target must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueTargetRule
{
    List<string> Check(TrainingDueTarget item);
}

public sealed record TrainingDueTargetRequiredNameRule(string Message) : ITrainingDueTargetRule
{
    public List<string> Check(TrainingDueTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTargetCodeLengthRule(int MaxLength) : ITrainingDueTargetRule
{
    public List<string> Check(TrainingDueTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTargetApprovedGuardRule(double Limit) : ITrainingDueTargetRule
{
    public List<string> Check(TrainingDueTarget item)
    {
        if (item.Approved && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTargetFloorValueRangeRule(double Min, double Max) : ITrainingDueTargetRule
{
    public List<string> Check(TrainingDueTarget item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueTargetContactEmailLengthRule(int MaxLength) : ITrainingDueTargetRule
{
    public List<string> Check(TrainingDueTarget item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due target.</summary>
public sealed record TrainingDueTargetValidator(List<ITrainingDueTargetRule> Rules)
{
    public List<string> Validate(TrainingDueTarget item)
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

    public void RequireValid(TrainingDueTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueTarget> items)
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

/// <summary>Functions of training due target rules.</summary>
public static class TrainingDueTargetRules
{
    public static TrainingDueTargetValidator DefaultTrainingDueTargetValidator()
    {
        return new TrainingDueTargetValidator(
            new List<ITrainingDueTargetRule>
            {
                new TrainingDueTargetRequiredNameRule("training due target needs a value"),
                new TrainingDueTargetCodeLengthRule(80),
                new TrainingDueTargetApprovedGuardRule(4.2),
                new TrainingDueTargetFloorValueRangeRule(8.1, 4574.4),
                new TrainingDueTargetContactEmailLengthRule(32),
            });
    }
}
