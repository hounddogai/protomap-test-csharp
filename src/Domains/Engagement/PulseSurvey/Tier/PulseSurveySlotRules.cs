namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>A rule a pulse survey slot must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveySlotRule
{
    List<string> Check(PulseSurveySlot item);
}

public sealed record PulseSurveySlotCodeLengthRule(int MaxLength) : IPulseSurveySlotRule
{
    public List<string> Check(PulseSurveySlot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveySlotRequiredSummaryRule(string Message) : IPulseSurveySlotRule
{
    public List<string> Check(PulseSurveySlot item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveySlotRequiredNameRule(string Message) : IPulseSurveySlotRule
{
    public List<string> Check(PulseSurveySlot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveySlotAttemptsRangeRule(int Min, int Max) : IPulseSurveySlotRule
{
    public List<string> Check(PulseSurveySlot item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveySlotSummaryLengthRule(int MaxLength) : IPulseSurveySlotRule
{
    public List<string> Check(PulseSurveySlot item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveySlotNameLengthRule(int MaxLength) : IPulseSurveySlotRule
{
    public List<string> Check(PulseSurveySlot item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey slot.</summary>
public sealed record PulseSurveySlotValidator(List<IPulseSurveySlotRule> Rules)
{
    public List<string> Validate(PulseSurveySlot item)
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

    public void RequireValid(PulseSurveySlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveySlot> items)
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

/// <summary>Functions of pulse survey slot rules.</summary>
public static class PulseSurveySlotRules
{
    public static PulseSurveySlotValidator DefaultPulseSurveySlotValidator()
    {
        return new PulseSurveySlotValidator(
            new List<IPulseSurveySlotRule>
            {
                new PulseSurveySlotCodeLengthRule(64),
                new PulseSurveySlotRequiredSummaryRule("pulse survey slot needs an entry"),
                new PulseSurveySlotRequiredNameRule("pulse survey slot needs an entry"),
                new PulseSurveySlotAttemptsRangeRule(16, 255),
                new PulseSurveySlotSummaryLengthRule(1000),
                new PulseSurveySlotNameLengthRule(32),
            });
    }
}
