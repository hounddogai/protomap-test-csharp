namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>A rule a pulse survey plan must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyPlanRule
{
    List<string> Check(PulseSurveyPlan item);
}

public sealed record PulseSurveyPlanRatingRangeRule(double Min, double Max) : IPulseSurveyPlanRule
{
    public List<string> Check(PulseSurveyPlan item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyPlanLabelLengthRule(int MaxLength) : IPulseSurveyPlanRule
{
    public List<string> Check(PulseSurveyPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyPlanUpdatedAtRangeRule(int Min, int Max) : IPulseSurveyPlanRule
{
    public List<string> Check(PulseSurveyPlan item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyPlanQuantityRangeRule(int Min, int Max) : IPulseSurveyPlanRule
{
    public List<string> Check(PulseSurveyPlan item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyPlanCostRangeRule(double Min, double Max) : IPulseSurveyPlanRule
{
    public List<string> Check(PulseSurveyPlan item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyPlanRequiredCodeRule(string Message) : IPulseSurveyPlanRule
{
    public List<string> Check(PulseSurveyPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey plan.</summary>
public sealed record PulseSurveyPlanValidator(List<IPulseSurveyPlanRule> Rules)
{
    public List<string> Validate(PulseSurveyPlan item)
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

    public void RequireValid(PulseSurveyPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyPlan> items)
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

/// <summary>Functions of pulse survey plan rules.</summary>
public static class PulseSurveyPlanRules
{
    public static PulseSurveyPlanValidator DefaultPulseSurveyPlanValidator()
    {
        return new PulseSurveyPlanValidator(
            new List<IPulseSurveyPlanRule>
            {
                new PulseSurveyPlanRatingRangeRule(5.3, 4530.9),
                new PulseSurveyPlanLabelLengthRule(32),
                new PulseSurveyPlanUpdatedAtRangeRule(32, 86400),
                new PulseSurveyPlanQuantityRangeRule(255, 3600),
                new PulseSurveyPlanCostRangeRule(3.7, 4729.2),
                new PulseSurveyPlanRequiredCodeRule("pulse survey plan needs an entry"),
            });
    }
}
