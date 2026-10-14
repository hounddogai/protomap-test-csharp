namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>A rule a global pulse survey record must pass. Check returns its problems, or none.</summary>
public interface IGlobalPulseSurveyRecordRule
{
    List<string> Check(GlobalPulseSurveyRecord item);
}

public sealed record GlobalPulseSurveyRecordEndsAtRangeRule(int Min, int Max) : IGlobalPulseSurveyRecordRule
{
    public List<string> Check(GlobalPulseSurveyRecord item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyRecordRequiredNameRule(string Message) : IGlobalPulseSurveyRecordRule
{
    public List<string> Check(GlobalPulseSurveyRecord item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalPulseSurveyRecordWeightRangeRule(double Min, double Max) : IGlobalPulseSurveyRecordRule
{
    public List<string> Check(GlobalPulseSurveyRecord item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a global pulse survey record.</summary>
public sealed record GlobalPulseSurveyRecordValidator(List<IGlobalPulseSurveyRecordRule> Rules)
{
    public List<string> Validate(GlobalPulseSurveyRecord item)
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

    public void RequireValid(GlobalPulseSurveyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalPulseSurveyRecord> items)
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

/// <summary>Functions of global pulse survey record rules.</summary>
public static class GlobalPulseSurveyRecordRules
{
    public static GlobalPulseSurveyRecordValidator DefaultGlobalPulseSurveyRecordValidator()
    {
        return new GlobalPulseSurveyRecordValidator(
            new List<IGlobalPulseSurveyRecordRule>
            {
                new GlobalPulseSurveyRecordEndsAtRangeRule(64, 86400),
                new GlobalPulseSurveyRecordRequiredNameRule("global pulse survey record needs a value"),
                new GlobalPulseSurveyRecordWeightRangeRule(2.3, 2896.0),
            });
    }
}
