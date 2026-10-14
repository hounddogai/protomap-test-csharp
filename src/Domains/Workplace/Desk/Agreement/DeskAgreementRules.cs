namespace Acme.Workforce.Workplace.Desk.Agreement;

/// <summary>A rule a desk agreement must pass. Check returns its problems, or none.</summary>
public interface IDeskAgreementRule
{
    List<string> Check(DeskAgreement item);
}

public sealed record DeskAgreementRequiredCodeRule(string Message) : IDeskAgreementRule
{
    public List<string> Check(DeskAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskAgreementBadgeLengthRule(int MaxLength) : IDeskAgreementRule
{
    public List<string> Check(DeskAgreement item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskAgreementRequiredJobCodeRule(string Message) : IDeskAgreementRule
{
    public List<string> Check(DeskAgreement item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk agreement.</summary>
public sealed record DeskAgreementValidator(List<IDeskAgreementRule> Rules)
{
    public List<string> Validate(DeskAgreement item)
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

    public void RequireValid(DeskAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskAgreement> items)
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

/// <summary>Functions of desk agreement rules.</summary>
public static class DeskAgreementRules
{
    public static DeskAgreementValidator DefaultDeskAgreementValidator()
    {
        return new DeskAgreementValidator(
            new List<IDeskAgreementRule>
            {
                new DeskAgreementRequiredCodeRule("desk agreement needs an entry"),
                new DeskAgreementBadgeLengthRule(80),
                new DeskAgreementRequiredJobCodeRule("desk agreement needs an entry"),
            });
    }
}
