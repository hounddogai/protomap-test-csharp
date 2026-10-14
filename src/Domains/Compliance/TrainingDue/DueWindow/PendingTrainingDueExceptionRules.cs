namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>A rule a pending training due exception must pass. Check returns its problems, or none.</summary>
public interface IPendingTrainingDueExceptionRule
{
    List<string> Check(PendingTrainingDueException item);
}

public sealed record PendingTrainingDueExceptionRequiredCountryRule(string Message) : IPendingTrainingDueExceptionRule
{
    public List<string> Check(PendingTrainingDueException item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueExceptionLabelLengthRule(int MaxLength) : IPendingTrainingDueExceptionRule
{
    public List<string> Check(PendingTrainingDueException item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueExceptionRateRangeRule(double Min, double Max) : IPendingTrainingDueExceptionRule
{
    public List<string> Check(PendingTrainingDueException item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pending training due exception.</summary>
public sealed record PendingTrainingDueExceptionValidator(List<IPendingTrainingDueExceptionRule> Rules)
{
    public List<string> Validate(PendingTrainingDueException item)
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

    public void RequireValid(PendingTrainingDueException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTrainingDueException> items)
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

/// <summary>Functions of pending training due exception rules.</summary>
public static class PendingTrainingDueExceptionRules
{
    public static PendingTrainingDueExceptionValidator DefaultPendingTrainingDueExceptionValidator()
    {
        return new PendingTrainingDueExceptionValidator(
            new List<IPendingTrainingDueExceptionRule>
            {
                new PendingTrainingDueExceptionRequiredCountryRule("pending training due exception needs an entry"),
                new PendingTrainingDueExceptionLabelLengthRule(1000),
                new PendingTrainingDueExceptionRateRangeRule(1.7, 1206.0),
            });
    }
}
