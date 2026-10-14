namespace Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>A rule a seasonal training due baseline must pass. Check returns its problems, or none.</summary>
public interface ISeasonalTrainingDueBaselineRule
{
    List<string> Check(SeasonalTrainingDueBaseline item);
}

public sealed record SeasonalTrainingDueBaselineBadgeLengthRule(int MaxLength) : ISeasonalTrainingDueBaselineRule
{
    public List<string> Check(SeasonalTrainingDueBaseline item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTrainingDueBaselinePhoneLengthRule(int MaxLength) : ISeasonalTrainingDueBaselineRule
{
    public List<string> Check(SeasonalTrainingDueBaseline item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTrainingDueBaselineRequiredCodeRule(string Message) : ISeasonalTrainingDueBaselineRule
{
    public List<string> Check(SeasonalTrainingDueBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal training due baseline.</summary>
public sealed record SeasonalTrainingDueBaselineValidator(List<ISeasonalTrainingDueBaselineRule> Rules)
{
    public List<string> Validate(SeasonalTrainingDueBaseline item)
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

    public void RequireValid(SeasonalTrainingDueBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalTrainingDueBaseline> items)
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

/// <summary>Functions of seasonal training due baseline rules.</summary>
public static class SeasonalTrainingDueBaselineRules
{
    public static SeasonalTrainingDueBaselineValidator DefaultSeasonalTrainingDueBaselineValidator()
    {
        return new SeasonalTrainingDueBaselineValidator(
            new List<ISeasonalTrainingDueBaselineRule>
            {
                new SeasonalTrainingDueBaselineBadgeLengthRule(86400),
                new SeasonalTrainingDueBaselinePhoneLengthRule(32),
                new SeasonalTrainingDueBaselineRequiredCodeRule("seasonal training due baseline needs an entry"),
            });
    }
}
