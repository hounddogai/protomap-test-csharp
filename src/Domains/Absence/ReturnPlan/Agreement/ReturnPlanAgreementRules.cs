namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>A rule a return plan agreement must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanAgreementRule
{
    List<string> Check(ReturnPlanAgreement item);
}

public sealed record ReturnPlanAgreementRatingRangeRule(double Min, double Max) : IReturnPlanAgreementRule
{
    public List<string> Check(ReturnPlanAgreement item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanAgreementLabelLengthRule(int MaxLength) : IReturnPlanAgreementRule
{
    public List<string> Check(ReturnPlanAgreement item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanAgreementCeilingRangeRule(double Min, double Max) : IReturnPlanAgreementRule
{
    public List<string> Check(ReturnPlanAgreement item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan agreement.</summary>
public sealed record ReturnPlanAgreementValidator(List<IReturnPlanAgreementRule> Rules)
{
    public List<string> Validate(ReturnPlanAgreement item)
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

    public void RequireValid(ReturnPlanAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanAgreement> items)
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

/// <summary>Functions of return plan agreement rules.</summary>
public static class ReturnPlanAgreementRules
{
    public static ReturnPlanAgreementValidator DefaultReturnPlanAgreementValidator()
    {
        return new ReturnPlanAgreementValidator(
            new List<IReturnPlanAgreementRule>
            {
                new ReturnPlanAgreementRatingRangeRule(0.9, 3666.3),
                new ReturnPlanAgreementLabelLengthRule(120),
                new ReturnPlanAgreementCeilingRangeRule(8.4, 4667.9),
            });
    }
}
