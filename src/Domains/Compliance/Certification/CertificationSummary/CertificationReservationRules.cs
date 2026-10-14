namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

/// <summary>A rule a certification reservation must pass. Check returns its problems, or none.</summary>
public interface ICertificationReservationRule
{
    List<string> Check(CertificationReservation item);
}

public sealed record CertificationReservationPriorityRangeRule(int Min, int Max) : ICertificationReservationRule
{
    public List<string> Check(CertificationReservation item)
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

public sealed record CertificationReservationRequiredExternalRefRule(string Message) : ICertificationReservationRule
{
    public List<string> Check(CertificationReservation item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationReservationCodeLengthRule(int MaxLength) : ICertificationReservationRule
{
    public List<string> Check(CertificationReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CertificationReservationRequiredEmployeeIdRule(string Message) : ICertificationReservationRule
{
    public List<string> Check(CertificationReservation item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CertificationReservationLeaveDaysRangeRule(int Min, int Max) : ICertificationReservationRule
{
    public List<string> Check(CertificationReservation item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a certification reservation.</summary>
public sealed record CertificationReservationValidator(List<ICertificationReservationRule> Rules)
{
    public List<string> Validate(CertificationReservation item)
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

    public void RequireValid(CertificationReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CertificationReservation> items)
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

/// <summary>Functions of certification reservation rules.</summary>
public static class CertificationReservationRules
{
    public static CertificationReservationValidator DefaultCertificationReservationValidator()
    {
        return new CertificationReservationValidator(
            new List<ICertificationReservationRule>
            {
                new CertificationReservationPriorityRangeRule(16, 32),
                new CertificationReservationRequiredExternalRefRule("certification reservation needs this field"),
                new CertificationReservationCodeLengthRule(80),
                new CertificationReservationRequiredEmployeeIdRule("certification reservation needs an entry"),
                new CertificationReservationLeaveDaysRangeRule(120, 255),
            });
    }
}
