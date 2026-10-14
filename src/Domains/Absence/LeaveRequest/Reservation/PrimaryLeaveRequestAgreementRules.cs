namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>A rule a primary leave request agreement must pass. Check returns its problems, or none.</summary>
public interface IPrimaryLeaveRequestAgreementRule
{
    List<string> Check(PrimaryLeaveRequestAgreement item);
}

public sealed record PrimaryLeaveRequestAgreementRequiredEmployeeIdRule(string Message) : IPrimaryLeaveRequestAgreementRule
{
    public List<string> Check(PrimaryLeaveRequestAgreement item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestAgreementEndsAtRangeRule(int Min, int Max) : IPrimaryLeaveRequestAgreementRule
{
    public List<string> Check(PrimaryLeaveRequestAgreement item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryLeaveRequestAgreementAttemptsRangeRule(int Min, int Max) : IPrimaryLeaveRequestAgreementRule
{
    public List<string> Check(PrimaryLeaveRequestAgreement item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryLeaveRequestAgreementEmployeeIdLengthRule(int MaxLength) : IPrimaryLeaveRequestAgreementRule
{
    public List<string> Check(PrimaryLeaveRequestAgreement item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryLeaveRequestAgreementRequiredNameRule(string Message) : IPrimaryLeaveRequestAgreementRule
{
    public List<string> Check(PrimaryLeaveRequestAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary leave request agreement.</summary>
public sealed record PrimaryLeaveRequestAgreementValidator(List<IPrimaryLeaveRequestAgreementRule> Rules)
{
    public List<string> Validate(PrimaryLeaveRequestAgreement item)
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

    public void RequireValid(PrimaryLeaveRequestAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryLeaveRequestAgreement> items)
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

/// <summary>Functions of primary leave request agreement rules.</summary>
public static class PrimaryLeaveRequestAgreementRules
{
    public static PrimaryLeaveRequestAgreementValidator DefaultPrimaryLeaveRequestAgreementValidator()
    {
        return new PrimaryLeaveRequestAgreementValidator(
            new List<IPrimaryLeaveRequestAgreementRule>
            {
                new PrimaryLeaveRequestAgreementRequiredEmployeeIdRule(
                    "primary leave request agreement needs a value"),
                new PrimaryLeaveRequestAgreementEndsAtRangeRule(80, 120),
                new PrimaryLeaveRequestAgreementAttemptsRangeRule(32, 86400),
                new PrimaryLeaveRequestAgreementEmployeeIdLengthRule(255),
                new PrimaryLeaveRequestAgreementRequiredNameRule("primary leave request agreement needs an entry"),
            });
    }
}
