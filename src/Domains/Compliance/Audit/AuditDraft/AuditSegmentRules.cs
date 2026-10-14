namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a audit segment must pass. Check returns its problems, or none.</summary>
public interface IAuditSegmentRule
{
    List<string> Check(AuditSegment item);
}

public sealed record AuditSegmentRequiredJobCodeRule(string Message) : IAuditSegmentRule
{
    public List<string> Check(AuditSegment item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AuditSegmentAllowedTitleRule(List<string> Allowed) : IAuditSegmentRule
{
    public List<string> Check(AuditSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is too long" };
        }
        return new List<string>();
    }
}

public sealed record AuditSegmentCodeLengthRule(int MaxLength) : IAuditSegmentRule
{
    public List<string> Check(AuditSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a audit segment.</summary>
public sealed record AuditSegmentValidator(List<IAuditSegmentRule> Rules)
{
    public List<string> Validate(AuditSegment item)
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

    public void RequireValid(AuditSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AuditSegment> items)
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

/// <summary>Functions of audit segment rules.</summary>
public static class AuditSegmentRules
{
    public static AuditSegmentValidator DefaultAuditSegmentValidator()
    {
        return new AuditSegmentValidator(
            new List<IAuditSegmentRule>
            {
                new AuditSegmentRequiredJobCodeRule("audit segment needs an entry"),
                new AuditSegmentAllowedTitleRule(new List<string>()),
                new AuditSegmentCodeLengthRule(32),
            });
    }
}
