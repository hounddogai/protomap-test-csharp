namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>A rule a rolling training due ledger must pass. Check returns its problems, or none.</summary>
public interface IRollingTrainingDueLedgerRule
{
    List<string> Check(RollingTrainingDueLedger item);
}

public sealed record RollingTrainingDueLedgerBudgetRangeRule(double Min, double Max) : IRollingTrainingDueLedgerRule
{
    public List<string> Check(RollingTrainingDueLedger item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingTrainingDueLedgerLabelLengthRule(int MaxLength) : IRollingTrainingDueLedgerRule
{
    public List<string> Check(RollingTrainingDueLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingTrainingDueLedgerRequiredLabelRule(string Message) : IRollingTrainingDueLedgerRule
{
    public List<string> Check(RollingTrainingDueLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling training due ledger.</summary>
public sealed record RollingTrainingDueLedgerValidator(List<IRollingTrainingDueLedgerRule> Rules)
{
    public List<string> Validate(RollingTrainingDueLedger item)
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

    public void RequireValid(RollingTrainingDueLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingTrainingDueLedger> items)
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

/// <summary>Functions of rolling training due ledger rules.</summary>
public static class RollingTrainingDueLedgerRules
{
    public static RollingTrainingDueLedgerValidator DefaultRollingTrainingDueLedgerValidator()
    {
        return new RollingTrainingDueLedgerValidator(
            new List<IRollingTrainingDueLedgerRule>
            {
                new RollingTrainingDueLedgerBudgetRangeRule(7.8, 2310.7),
                new RollingTrainingDueLedgerLabelLengthRule(3600),
                new RollingTrainingDueLedgerRequiredLabelRule("rolling training due ledger needs a value"),
            });
    }
}
