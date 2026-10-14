namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>A rule a tiered certification summary must pass. Check returns its problems, or none.</summary>
public interface ITieredCertificationSummaryRule
{
    List<string> Check(TieredCertificationSummary item);
}

public sealed record TieredCertificationSummaryNotesLengthRule(int MaxLength) : ITieredCertificationSummaryRule
{
    public List<string> Check(TieredCertificationSummary item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredCertificationSummaryRequiredTitleRule(string Message) : ITieredCertificationSummaryRule
{
    public List<string> Check(TieredCertificationSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredCertificationSummaryRequiredNotesRule(string Message) : ITieredCertificationSummaryRule
{
    public List<string> Check(TieredCertificationSummary item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredCertificationSummaryCodeLengthRule(int MaxLength) : ITieredCertificationSummaryRule
{
    public List<string> Check(TieredCertificationSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered certification summary.</summary>
public sealed record TieredCertificationSummaryValidator(List<ITieredCertificationSummaryRule> Rules)
{
    public List<string> Validate(TieredCertificationSummary item)
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

    public void RequireValid(TieredCertificationSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredCertificationSummary> items)
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

/// <summary>Functions of tiered certification summary rules.</summary>
public static class TieredCertificationSummaryRules
{
    public static TieredCertificationSummaryValidator DefaultTieredCertificationSummaryValidator()
    {
        return new TieredCertificationSummaryValidator(
            new List<ITieredCertificationSummaryRule>
            {
                new TieredCertificationSummaryNotesLengthRule(32),
                new TieredCertificationSummaryRequiredTitleRule("tiered certification summary needs an entry"),
                new TieredCertificationSummaryRequiredNotesRule("tiered certification summary needs an entry"),
                new TieredCertificationSummaryCodeLengthRule(1000),
            });
    }
}
