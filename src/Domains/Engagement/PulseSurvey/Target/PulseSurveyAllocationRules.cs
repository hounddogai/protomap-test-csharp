namespace Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>A rule a pulse survey allocation must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyAllocationRule
{
    List<string> Check(PulseSurveyAllocation item);
}

public sealed record PulseSurveyAllocationDurationMinutesRangeRule(int Min, int Max) : IPulseSurveyAllocationRule
{
    public List<string> Check(PulseSurveyAllocation item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAllocationCodeLengthRule(int MaxLength) : IPulseSurveyAllocationRule
{
    public List<string> Check(PulseSurveyAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAllocationRequiredExternalRefRule(string Message) : IPulseSurveyAllocationRule
{
    public List<string> Check(PulseSurveyAllocation item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAllocationRequiredCodeRule(string Message) : IPulseSurveyAllocationRule
{
    public List<string> Check(PulseSurveyAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey allocation.</summary>
public sealed record PulseSurveyAllocationValidator(List<IPulseSurveyAllocationRule> Rules)
{
    public List<string> Validate(PulseSurveyAllocation item)
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

    public void RequireValid(PulseSurveyAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyAllocation> items)
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

/// <summary>Functions of pulse survey allocation rules.</summary>
public static class PulseSurveyAllocationRules
{
    public static PulseSurveyAllocationValidator DefaultPulseSurveyAllocationValidator()
    {
        return new PulseSurveyAllocationValidator(
            new List<IPulseSurveyAllocationRule>
            {
                new PulseSurveyAllocationDurationMinutesRangeRule(32, 120),
                new PulseSurveyAllocationCodeLengthRule(16),
                new PulseSurveyAllocationRequiredExternalRefRule("pulse survey allocation needs a value"),
                new PulseSurveyAllocationRequiredCodeRule("pulse survey allocation needs an entry"),
            });
    }
}
