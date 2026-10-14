namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>A rule a pending training due ledger must pass. Check returns its problems, or none.</summary>
public interface IPendingTrainingDueLedgerRule
{
    List<string> Check(PendingTrainingDueLedger item);
}

public sealed record PendingTrainingDueLedgerDescriptionLengthRule(int MaxLength) : IPendingTrainingDueLedgerRule
{
    public List<string> Check(PendingTrainingDueLedger item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueLedgerCodePrefixRule(string Prefix) : IPendingTrainingDueLedgerRule
{
    public List<string> Check(PendingTrainingDueLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueLedgerRatingRangeRule(double Min, double Max) : IPendingTrainingDueLedgerRule
{
    public List<string> Check(PendingTrainingDueLedger item)
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

public sealed record PendingTrainingDueLedgerRequiredNameRule(string Message) : IPendingTrainingDueLedgerRule
{
    public List<string> Check(PendingTrainingDueLedger item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingTrainingDueLedgerSegmentLengthRule(int MaxLength) : IPendingTrainingDueLedgerRule
{
    public List<string> Check(PendingTrainingDueLedger item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending training due ledger.</summary>
public sealed record PendingTrainingDueLedgerValidator(List<IPendingTrainingDueLedgerRule> Rules)
{
    public List<string> Validate(PendingTrainingDueLedger item)
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

    public void RequireValid(PendingTrainingDueLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTrainingDueLedger> items)
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

/// <summary>Functions of pending training due ledger rules.</summary>
public static class PendingTrainingDueLedgerRules
{
    public static PendingTrainingDueLedgerValidator DefaultPendingTrainingDueLedgerValidator()
    {
        return new PendingTrainingDueLedgerValidator(
            new List<IPendingTrainingDueLedgerRule>
            {
                new PendingTrainingDueLedgerDescriptionLengthRule(32),
                new PendingTrainingDueLedgerCodePrefixRule("Q"),
                new PendingTrainingDueLedgerRatingRangeRule(0.0, 3777.3),
                new PendingTrainingDueLedgerRequiredNameRule("pending training due ledger needs this field"),
                new PendingTrainingDueLedgerSegmentLengthRule(80),
            });
    }
}
