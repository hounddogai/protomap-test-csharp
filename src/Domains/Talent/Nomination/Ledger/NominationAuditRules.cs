namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination audit must pass. Check returns its problems, or none.</summary>
public interface INominationAuditRule
{
    List<string> Check(NominationAudit item);
}

public sealed record NominationAuditNotesLengthRule(int MaxLength) : INominationAuditRule
{
    public List<string> Check(NominationAudit item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationAuditCodeLengthRule(int MaxLength) : INominationAuditRule
{
    public List<string> Check(NominationAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationAuditAllowedNotesRule(List<string> Allowed) : INominationAuditRule
{
    public List<string> Check(NominationAudit item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record NominationAuditGradeLengthRule(int MaxLength) : INominationAuditRule
{
    public List<string> Check(NominationAudit item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record NominationAuditMinimumRangeRule(double Min, double Max) : INominationAuditRule
{
    public List<string> Check(NominationAudit item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination audit.</summary>
public sealed record NominationAuditValidator(List<INominationAuditRule> Rules)
{
    public List<string> Validate(NominationAudit item)
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

    public void RequireValid(NominationAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationAudit> items)
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

/// <summary>Functions of nomination audit rules.</summary>
public static class NominationAuditRules
{
    public static NominationAuditValidator DefaultNominationAuditValidator()
    {
        return new NominationAuditValidator(
            new List<INominationAuditRule>
            {
                new NominationAuditNotesLengthRule(80),
                new NominationAuditCodeLengthRule(3600),
                new NominationAuditAllowedNotesRule(new List<string> { "tiered", "capped", "legacy" }),
                new NominationAuditGradeLengthRule(255),
                new NominationAuditMinimumRangeRule(2.1, 3984.8),
            });
    }
}
