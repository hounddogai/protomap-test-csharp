namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>A rule a capped certification assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedCertificationAssignmentRule
{
    List<string> Check(CappedCertificationAssignment item);
}

public sealed record CappedCertificationAssignmentRetriesRangeRule(int Min, int Max) : ICappedCertificationAssignmentRule
{
    public List<string> Check(CappedCertificationAssignment item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedCertificationAssignmentAttemptsRangeRule(int Min, int Max) : ICappedCertificationAssignmentRule
{
    public List<string> Check(CappedCertificationAssignment item)
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

public sealed record CappedCertificationAssignmentRequiredLocaleRule(string Message) : ICappedCertificationAssignmentRule
{
    public List<string> Check(CappedCertificationAssignment item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped certification assignment.</summary>
public sealed record CappedCertificationAssignmentValidator(List<ICappedCertificationAssignmentRule> Rules)
{
    public List<string> Validate(CappedCertificationAssignment item)
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

    public void RequireValid(CappedCertificationAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedCertificationAssignment> items)
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

/// <summary>Functions of capped certification assignment rules.</summary>
public static class CappedCertificationAssignmentRules
{
    public static CappedCertificationAssignmentValidator DefaultCappedCertificationAssignmentValidator()
    {
        return new CappedCertificationAssignmentValidator(
            new List<ICappedCertificationAssignmentRule>
            {
                new CappedCertificationAssignmentRetriesRangeRule(64, 64),
                new CappedCertificationAssignmentAttemptsRangeRule(16, 1000),
                new CappedCertificationAssignmentRequiredLocaleRule("capped certification assignment needs an entry"),
            });
    }
}
