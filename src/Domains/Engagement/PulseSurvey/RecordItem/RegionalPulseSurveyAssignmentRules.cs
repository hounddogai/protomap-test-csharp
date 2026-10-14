namespace Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>A rule a regional pulse survey assignment must pass. Check returns its problems, or none.</summary>
public interface IRegionalPulseSurveyAssignmentRule
{
    List<string> Check(RegionalPulseSurveyAssignment item);
}

public sealed record RegionalPulseSurveyAssignmentPinnedGuardRule(double Limit) : IRegionalPulseSurveyAssignmentRule
{
    public List<string> Check(RegionalPulseSurveyAssignment item)
    {
        if (item.Pinned && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPulseSurveyAssignmentMaximumRangeRule(double Min, double Max) : IRegionalPulseSurveyAssignmentRule
{
    public List<string> Check(RegionalPulseSurveyAssignment item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalPulseSurveyAssignmentCodeLengthRule(int MaxLength) : IRegionalPulseSurveyAssignmentRule
{
    public List<string> Check(RegionalPulseSurveyAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional pulse survey assignment.</summary>
public sealed record RegionalPulseSurveyAssignmentValidator(List<IRegionalPulseSurveyAssignmentRule> Rules)
{
    public List<string> Validate(RegionalPulseSurveyAssignment item)
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

    public void RequireValid(RegionalPulseSurveyAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalPulseSurveyAssignment> items)
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

/// <summary>Functions of regional pulse survey assignment rules.</summary>
public static class RegionalPulseSurveyAssignmentRules
{
    public static RegionalPulseSurveyAssignmentValidator DefaultRegionalPulseSurveyAssignmentValidator()
    {
        return new RegionalPulseSurveyAssignmentValidator(
            new List<IRegionalPulseSurveyAssignmentRule>
            {
                new RegionalPulseSurveyAssignmentPinnedGuardRule(7.1),
                new RegionalPulseSurveyAssignmentMaximumRangeRule(9.6, 1331.6),
                new RegionalPulseSurveyAssignmentCodeLengthRule(1000),
            });
    }
}
