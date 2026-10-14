namespace Acme.Workforce.Assets.Assignment.Claim;

/// <summary>A rule a assignment agreement must pass. Check returns its problems, or none.</summary>
public interface IAssignmentAgreementRule
{
    List<string> Check(AssignmentAgreement item);
}

public sealed record AssignmentAgreementRateRangeRule(double Min, double Max) : IAssignmentAgreementRule
{
    public List<string> Check(AssignmentAgreement item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AssignmentAgreementRequiredEmployeeIdRule(string Message) : IAssignmentAgreementRule
{
    public List<string> Check(AssignmentAgreement item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAgreementCodeLengthRule(int MaxLength) : IAssignmentAgreementRule
{
    public List<string> Check(AssignmentAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAgreementRequiredTitleRule(string Message) : IAssignmentAgreementRule
{
    public List<string> Check(AssignmentAgreement item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssignmentAgreementRatingRangeRule(double Min, double Max) : IAssignmentAgreementRule
{
    public List<string> Check(AssignmentAgreement item)
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

public sealed record AssignmentAgreementPhoneLengthRule(int MaxLength) : IAssignmentAgreementRule
{
    public List<string> Check(AssignmentAgreement item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment agreement.</summary>
public sealed record AssignmentAgreementValidator(List<IAssignmentAgreementRule> Rules)
{
    public List<string> Validate(AssignmentAgreement item)
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

    public void RequireValid(AssignmentAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentAgreement> items)
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

/// <summary>Functions of assignment agreement rules.</summary>
public static class AssignmentAgreementRules
{
    public static AssignmentAgreementValidator DefaultAssignmentAgreementValidator()
    {
        return new AssignmentAgreementValidator(
            new List<IAssignmentAgreementRule>
            {
                new AssignmentAgreementRateRangeRule(8.7, 2565.3),
                new AssignmentAgreementRequiredEmployeeIdRule("assignment agreement needs this field"),
                new AssignmentAgreementCodeLengthRule(1000),
                new AssignmentAgreementRequiredTitleRule("assignment agreement needs a value"),
                new AssignmentAgreementRatingRangeRule(5.2, 2188.6),
                new AssignmentAgreementPhoneLengthRule(86400),
            });
    }
}
