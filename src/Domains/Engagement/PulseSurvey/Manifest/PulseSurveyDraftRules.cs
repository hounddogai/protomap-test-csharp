namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey draft must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyDraftRule
{
    List<string> Check(PulseSurveyDraft item);
}

public sealed record PulseSurveyDraftRequiredCodeRule(string Message) : IPulseSurveyDraftRule
{
    public List<string> Check(PulseSurveyDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyDraftManagerIdLengthRule(int MaxLength) : IPulseSurveyDraftRule
{
    public List<string> Check(PulseSurveyDraft item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyDraftActiveGuardRule(double Limit) : IPulseSurveyDraftRule
{
    public List<string> Check(PulseSurveyDraft item)
    {
        if (item.Active && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyDraftAllowedNameRule(List<string> Allowed) : IPulseSurveyDraftRule
{
    public List<string> Check(PulseSurveyDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyDraftRequiredSalaryBandRule(string Message) : IPulseSurveyDraftRule
{
    public List<string> Check(PulseSurveyDraft item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey draft.</summary>
public sealed record PulseSurveyDraftValidator(List<IPulseSurveyDraftRule> Rules)
{
    public List<string> Validate(PulseSurveyDraft item)
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

    public void RequireValid(PulseSurveyDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyDraft> items)
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

/// <summary>Functions of pulse survey draft rules.</summary>
public static class PulseSurveyDraftRules
{
    public static PulseSurveyDraftValidator DefaultPulseSurveyDraftValidator()
    {
        return new PulseSurveyDraftValidator(
            new List<IPulseSurveyDraftRule>
            {
                new PulseSurveyDraftRequiredCodeRule("pulse survey draft needs an entry"),
                new PulseSurveyDraftManagerIdLengthRule(86400),
                new PulseSurveyDraftActiveGuardRule(4.8),
                new PulseSurveyDraftAllowedNameRule(new List<string> { "manual", "lenient" }),
                new PulseSurveyDraftRequiredSalaryBandRule("pulse survey draft needs an entry"),
            });
    }
}
