namespace Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>A rule a batched skill profile forecast must pass. Check returns its problems, or none.</summary>
public interface IBatchedSkillProfileForecastRule
{
    List<string> Check(BatchedSkillProfileForecast item);
}

public sealed record BatchedSkillProfileForecastAmountRangeRule(double Min, double Max) : IBatchedSkillProfileForecastRule
{
    public List<string> Check(BatchedSkillProfileForecast item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileForecastRequiredSummaryRule(string Message) : IBatchedSkillProfileForecastRule
{
    public List<string> Check(BatchedSkillProfileForecast item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedSkillProfileForecastCodeLengthRule(int MaxLength) : IBatchedSkillProfileForecastRule
{
    public List<string> Check(BatchedSkillProfileForecast item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched skill profile forecast.</summary>
public sealed record BatchedSkillProfileForecastValidator(List<IBatchedSkillProfileForecastRule> Rules)
{
    public List<string> Validate(BatchedSkillProfileForecast item)
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

    public void RequireValid(BatchedSkillProfileForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedSkillProfileForecast> items)
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

/// <summary>Functions of batched skill profile forecast rules.</summary>
public static class BatchedSkillProfileForecastRules
{
    public static BatchedSkillProfileForecastValidator DefaultBatchedSkillProfileForecastValidator()
    {
        return new BatchedSkillProfileForecastValidator(
            new List<IBatchedSkillProfileForecastRule>
            {
                new BatchedSkillProfileForecastAmountRangeRule(8.4, 2770.9),
                new BatchedSkillProfileForecastRequiredSummaryRule("batched skill profile forecast needs a value"),
                new BatchedSkillProfileForecastCodeLengthRule(80),
            });
    }
}
