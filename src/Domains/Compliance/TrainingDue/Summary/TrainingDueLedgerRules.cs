namespace Acme.Workforce.Compliance.TrainingDue.Summary;

/// <summary>A rule a training due ledger must pass. Check returns its problems, or none.</summary>
public interface ITrainingDueLedgerRule
{
    List<string> Check(TrainingDueLedger item);
}

public sealed record TrainingDueLedgerRatingRangeRule(double Min, double Max) : ITrainingDueLedgerRule
{
    public List<string> Check(TrainingDueLedger item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TrainingDueLedgerRequiredContactEmailRule(string Message) : ITrainingDueLedgerRule
{
    public List<string> Check(TrainingDueLedger item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TrainingDueLedgerMaximumRangeRule(double Min, double Max) : ITrainingDueLedgerRule
{
    public List<string> Check(TrainingDueLedger item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a training due ledger.</summary>
public sealed record TrainingDueLedgerValidator(List<ITrainingDueLedgerRule> Rules)
{
    public List<string> Validate(TrainingDueLedger item)
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

    public void RequireValid(TrainingDueLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TrainingDueLedger> items)
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

/// <summary>Functions of training due ledger rules.</summary>
public static class TrainingDueLedgerRules
{
    public static TrainingDueLedgerValidator DefaultTrainingDueLedgerValidator()
    {
        return new TrainingDueLedgerValidator(
            new List<ITrainingDueLedgerRule>
            {
                new TrainingDueLedgerRatingRangeRule(2.7, 2710.7),
                new TrainingDueLedgerRequiredContactEmailRule("training due ledger needs a value"),
                new TrainingDueLedgerMaximumRangeRule(4.6, 3317.7),
            });
    }
}
