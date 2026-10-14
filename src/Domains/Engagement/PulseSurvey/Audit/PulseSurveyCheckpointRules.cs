namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>A rule a pulse survey checkpoint must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyCheckpointRule
{
    List<string> Check(PulseSurveyCheckpoint item);
}

public sealed record PulseSurveyCheckpointGradeLengthRule(int MaxLength) : IPulseSurveyCheckpointRule
{
    public List<string> Check(PulseSurveyCheckpoint item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyCheckpointEndsAtRangeRule(int Min, int Max) : IPulseSurveyCheckpointRule
{
    public List<string> Check(PulseSurveyCheckpoint item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyCheckpointRequiredCodeRule(string Message) : IPulseSurveyCheckpointRule
{
    public List<string> Check(PulseSurveyCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyCheckpointJobCodeLengthRule(int MaxLength) : IPulseSurveyCheckpointRule
{
    public List<string> Check(PulseSurveyCheckpoint item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyCheckpointRequiredGradeRule(string Message) : IPulseSurveyCheckpointRule
{
    public List<string> Check(PulseSurveyCheckpoint item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey checkpoint.</summary>
public sealed record PulseSurveyCheckpointValidator(List<IPulseSurveyCheckpointRule> Rules)
{
    public List<string> Validate(PulseSurveyCheckpoint item)
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

    public void RequireValid(PulseSurveyCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyCheckpoint> items)
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

/// <summary>Functions of pulse survey checkpoint rules.</summary>
public static class PulseSurveyCheckpointRules
{
    public static PulseSurveyCheckpointValidator DefaultPulseSurveyCheckpointValidator()
    {
        return new PulseSurveyCheckpointValidator(
            new List<IPulseSurveyCheckpointRule>
            {
                new PulseSurveyCheckpointGradeLengthRule(32),
                new PulseSurveyCheckpointEndsAtRangeRule(64, 120),
                new PulseSurveyCheckpointRequiredCodeRule("pulse survey checkpoint needs a value"),
                new PulseSurveyCheckpointJobCodeLengthRule(3600),
                new PulseSurveyCheckpointRequiredGradeRule("pulse survey checkpoint needs an entry"),
            });
    }
}
