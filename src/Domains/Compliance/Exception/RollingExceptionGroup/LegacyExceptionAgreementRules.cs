namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>A rule a legacy exception agreement must pass. Check returns its problems, or none.</summary>
public interface ILegacyExceptionAgreementRule
{
    List<string> Check(LegacyExceptionAgreement item);
}

public sealed record LegacyExceptionAgreementGraceDaysRangeRule(int Min, int Max) : ILegacyExceptionAgreementRule
{
    public List<string> Check(LegacyExceptionAgreement item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyExceptionAgreementCodeLengthRule(int MaxLength) : ILegacyExceptionAgreementRule
{
    public List<string> Check(LegacyExceptionAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyExceptionAgreementRequiredNameRule(string Message) : ILegacyExceptionAgreementRule
{
    public List<string> Check(LegacyExceptionAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyExceptionAgreementDurationMinutesRangeRule(int Min, int Max) : ILegacyExceptionAgreementRule
{
    public List<string> Check(LegacyExceptionAgreement item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LegacyExceptionAgreementSkillLengthRule(int MaxLength) : ILegacyExceptionAgreementRule
{
    public List<string> Check(LegacyExceptionAgreement item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy exception agreement.</summary>
public sealed record LegacyExceptionAgreementValidator(List<ILegacyExceptionAgreementRule> Rules)
{
    public List<string> Validate(LegacyExceptionAgreement item)
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

    public void RequireValid(LegacyExceptionAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyExceptionAgreement> items)
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

/// <summary>Functions of legacy exception agreement rules.</summary>
public static class LegacyExceptionAgreementRules
{
    public static LegacyExceptionAgreementValidator DefaultLegacyExceptionAgreementValidator()
    {
        return new LegacyExceptionAgreementValidator(
            new List<ILegacyExceptionAgreementRule>
            {
                new LegacyExceptionAgreementGraceDaysRangeRule(16, 120),
                new LegacyExceptionAgreementCodeLengthRule(3600),
                new LegacyExceptionAgreementRequiredNameRule("legacy exception agreement needs an entry"),
                new LegacyExceptionAgreementDurationMinutesRangeRule(64, 1000),
                new LegacyExceptionAgreementSkillLengthRule(86400),
            });
    }
}
