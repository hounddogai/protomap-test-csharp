namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey record must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyRecordRule
{
    List<string> Check(PulseSurveyRecord item);
}

public sealed record PulseSurveyRecordRequiredLocaleRule(string Message) : IPulseSurveyRecordRule
{
    public List<string> Check(PulseSurveyRecord item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRecordCodeLengthRule(int MaxLength) : IPulseSurveyRecordRule
{
    public List<string> Check(PulseSurveyRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRecordAllowedNameRule(List<string> Allowed) : IPulseSurveyRecordRule
{
    public List<string> Check(PulseSurveyRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey record.</summary>
public sealed record PulseSurveyRecordValidator(List<IPulseSurveyRecordRule> Rules)
{
    public List<string> Validate(PulseSurveyRecord item)
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

    public void RequireValid(PulseSurveyRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyRecord> items)
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

/// <summary>Functions of pulse survey record rules.</summary>
public static class PulseSurveyRecordRules
{
    public static PulseSurveyRecordValidator DefaultPulseSurveyRecordValidator()
    {
        return new PulseSurveyRecordValidator(
            new List<IPulseSurveyRecordRule>
            {
                new PulseSurveyRecordRequiredLocaleRule("pulse survey record needs an entry"),
                new PulseSurveyRecordCodeLengthRule(120),
                new PulseSurveyRecordAllowedNameRule(new List<string>()),
            });
    }
}
