namespace Acme.Workforce.Talent.Nomination.Variance;

/// <summary>A rule a regional nomination variance must pass. Check returns its problems, or none.</summary>
public interface IRegionalNominationVarianceRule
{
    List<string> Check(RegionalNominationVariance item);
}

public sealed record RegionalNominationVarianceRequiredManagerIdRule(string Message) : IRegionalNominationVarianceRule
{
    public List<string> Check(RegionalNominationVariance item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalNominationVarianceManagerIdLengthRule(int MaxLength) : IRegionalNominationVarianceRule
{
    public List<string> Check(RegionalNominationVariance item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalNominationVariancePriorityRangeRule(int Min, int Max) : IRegionalNominationVarianceRule
{
    public List<string> Check(RegionalNominationVariance item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional nomination variance.</summary>
public sealed record RegionalNominationVarianceValidator(List<IRegionalNominationVarianceRule> Rules)
{
    public List<string> Validate(RegionalNominationVariance item)
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

    public void RequireValid(RegionalNominationVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalNominationVariance> items)
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

/// <summary>Functions of regional nomination variance rules.</summary>
public static class RegionalNominationVarianceRules
{
    public static RegionalNominationVarianceValidator DefaultRegionalNominationVarianceValidator()
    {
        return new RegionalNominationVarianceValidator(
            new List<IRegionalNominationVarianceRule>
            {
                new RegionalNominationVarianceRequiredManagerIdRule("regional nomination variance needs a value"),
                new RegionalNominationVarianceManagerIdLengthRule(80),
                new RegionalNominationVariancePriorityRangeRule(16, 86400),
            });
    }
}
