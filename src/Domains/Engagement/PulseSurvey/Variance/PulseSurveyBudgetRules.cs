namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey budget must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyBudgetRule
{
    List<string> Check(PulseSurveyBudget item);
}

public sealed record PulseSurveyBudgetCodeLengthRule(int MaxLength) : IPulseSurveyBudgetRule
{
    public List<string> Check(PulseSurveyBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyBudgetAllowedChecksumRule(List<string> Allowed) : IPulseSurveyBudgetRule
{
    public List<string> Check(PulseSurveyBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyBudgetRequiredCodeRule(string Message) : IPulseSurveyBudgetRule
{
    public List<string> Check(PulseSurveyBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyBudgetRatioRangeRule(double Min, double Max) : IPulseSurveyBudgetRule
{
    public List<string> Check(PulseSurveyBudget item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyBudgetRequiredNotesRule(string Message) : IPulseSurveyBudgetRule
{
    public List<string> Check(PulseSurveyBudget item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey budget.</summary>
public sealed record PulseSurveyBudgetValidator(List<IPulseSurveyBudgetRule> Rules)
{
    public List<string> Validate(PulseSurveyBudget item)
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

    public void RequireValid(PulseSurveyBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyBudget> items)
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

/// <summary>Functions of pulse survey budget rules.</summary>
public static class PulseSurveyBudgetRules
{
    public static PulseSurveyBudgetValidator DefaultPulseSurveyBudgetValidator()
    {
        return new PulseSurveyBudgetValidator(
            new List<IPulseSurveyBudgetRule>
            {
                new PulseSurveyBudgetCodeLengthRule(86400),
                new PulseSurveyBudgetAllowedChecksumRule(new List<string> { "default", "seasonal", "rolling" }),
                new PulseSurveyBudgetRequiredCodeRule("pulse survey budget needs this field"),
                new PulseSurveyBudgetRatioRangeRule(2.6, 1786.7),
                new PulseSurveyBudgetRequiredNotesRule("pulse survey budget needs this field"),
            });
    }
}
