namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>A rule a regional succession plan draft must pass. Check returns its problems, or none.</summary>
public interface IRegionalSuccessionPlanDraftRule
{
    List<string> Check(RegionalSuccessionPlanDraft item);
}

public sealed record RegionalSuccessionPlanDraftAmountRangeRule(double Min, double Max) : IRegionalSuccessionPlanDraftRule
{
    public List<string> Check(RegionalSuccessionPlanDraft item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalSuccessionPlanDraftPriorityRangeRule(int Min, int Max) : IRegionalSuccessionPlanDraftRule
{
    public List<string> Check(RegionalSuccessionPlanDraft item)
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

public sealed record RegionalSuccessionPlanDraftLocaleLengthRule(int MaxLength) : IRegionalSuccessionPlanDraftRule
{
    public List<string> Check(RegionalSuccessionPlanDraft item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanDraftReviewerLengthRule(int MaxLength) : IRegionalSuccessionPlanDraftRule
{
    public List<string> Check(RegionalSuccessionPlanDraft item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanDraftCodeLengthRule(int MaxLength) : IRegionalSuccessionPlanDraftRule
{
    public List<string> Check(RegionalSuccessionPlanDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalSuccessionPlanDraftTitleLengthRule(int MaxLength) : IRegionalSuccessionPlanDraftRule
{
    public List<string> Check(RegionalSuccessionPlanDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional succession plan draft.</summary>
public sealed record RegionalSuccessionPlanDraftValidator(List<IRegionalSuccessionPlanDraftRule> Rules)
{
    public List<string> Validate(RegionalSuccessionPlanDraft item)
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

    public void RequireValid(RegionalSuccessionPlanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalSuccessionPlanDraft> items)
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

/// <summary>Functions of regional succession plan draft rules.</summary>
public static class RegionalSuccessionPlanDraftRules
{
    public static RegionalSuccessionPlanDraftValidator DefaultRegionalSuccessionPlanDraftValidator()
    {
        return new RegionalSuccessionPlanDraftValidator(
            new List<IRegionalSuccessionPlanDraftRule>
            {
                new RegionalSuccessionPlanDraftAmountRangeRule(9.6, 3427.4),
                new RegionalSuccessionPlanDraftPriorityRangeRule(64, 1000),
                new RegionalSuccessionPlanDraftLocaleLengthRule(16),
                new RegionalSuccessionPlanDraftReviewerLengthRule(16),
                new RegionalSuccessionPlanDraftCodeLengthRule(16),
                new RegionalSuccessionPlanDraftTitleLengthRule(32),
            });
    }
}
