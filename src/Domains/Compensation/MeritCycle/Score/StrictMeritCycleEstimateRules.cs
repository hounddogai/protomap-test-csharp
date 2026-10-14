namespace Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>A rule a strict merit cycle estimate must pass. Check returns its problems, or none.</summary>
public interface IStrictMeritCycleEstimateRule
{
    List<string> Check(StrictMeritCycleEstimate item);
}

public sealed record StrictMeritCycleEstimateLabelLengthRule(int MaxLength) : IStrictMeritCycleEstimateRule
{
    public List<string> Check(StrictMeritCycleEstimate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictMeritCycleEstimateAmountRangeRule(double Min, double Max) : IStrictMeritCycleEstimateRule
{
    public List<string> Check(StrictMeritCycleEstimate item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictMeritCycleEstimateRequiredLabelRule(string Message) : IStrictMeritCycleEstimateRule
{
    public List<string> Check(StrictMeritCycleEstimate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictMeritCycleEstimateNotesLengthRule(int MaxLength) : IStrictMeritCycleEstimateRule
{
    public List<string> Check(StrictMeritCycleEstimate item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict merit cycle estimate.</summary>
public sealed record StrictMeritCycleEstimateValidator(List<IStrictMeritCycleEstimateRule> Rules)
{
    public List<string> Validate(StrictMeritCycleEstimate item)
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

    public void RequireValid(StrictMeritCycleEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictMeritCycleEstimate> items)
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

/// <summary>Functions of strict merit cycle estimate rules.</summary>
public static class StrictMeritCycleEstimateRules
{
    public static StrictMeritCycleEstimateValidator DefaultStrictMeritCycleEstimateValidator()
    {
        return new StrictMeritCycleEstimateValidator(
            new List<IStrictMeritCycleEstimateRule>
            {
                new StrictMeritCycleEstimateLabelLengthRule(64),
                new StrictMeritCycleEstimateAmountRangeRule(3.8, 752.8),
                new StrictMeritCycleEstimateRequiredLabelRule("strict merit cycle estimate needs an entry"),
                new StrictMeritCycleEstimateNotesLengthRule(32),
            });
    }
}
