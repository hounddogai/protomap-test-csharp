namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

/// <summary>A rule a pulse survey assignment must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyAssignmentRule
{
    List<string> Check(PulseSurveyAssignment item);
}

public sealed record PulseSurveyAssignmentRequiredNameRule(string Message) : IPulseSurveyAssignmentRule
{
    public List<string> Check(PulseSurveyAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAssignmentNameLengthRule(int MaxLength) : IPulseSurveyAssignmentRule
{
    public List<string> Check(PulseSurveyAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAssignmentCountryLengthRule(int MaxLength) : IPulseSurveyAssignmentRule
{
    public List<string> Check(PulseSurveyAssignment item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAssignmentRequiredPhoneRule(string Message) : IPulseSurveyAssignmentRule
{
    public List<string> Check(PulseSurveyAssignment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAssignmentRequiredCodeRule(string Message) : IPulseSurveyAssignmentRule
{
    public List<string> Check(PulseSurveyAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey assignment.</summary>
public sealed record PulseSurveyAssignmentValidator(List<IPulseSurveyAssignmentRule> Rules)
{
    public List<string> Validate(PulseSurveyAssignment item)
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

    public void RequireValid(PulseSurveyAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyAssignment> items)
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

/// <summary>Functions of pulse survey assignment rules.</summary>
public static class PulseSurveyAssignmentRules
{
    public static PulseSurveyAssignmentValidator DefaultPulseSurveyAssignmentValidator()
    {
        return new PulseSurveyAssignmentValidator(
            new List<IPulseSurveyAssignmentRule>
            {
                new PulseSurveyAssignmentRequiredNameRule("pulse survey assignment needs this field"),
                new PulseSurveyAssignmentNameLengthRule(1000),
                new PulseSurveyAssignmentCountryLengthRule(1000),
                new PulseSurveyAssignmentRequiredPhoneRule("pulse survey assignment needs an entry"),
                new PulseSurveyAssignmentRequiredCodeRule("pulse survey assignment needs a value"),
            });
    }
}
