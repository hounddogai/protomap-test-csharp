namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual assignment draft must pass. Check returns its problems, or none.</summary>
public interface IManualAssignmentDraftRule
{
    List<string> Check(ManualAssignmentDraft item);
}

public sealed record ManualAssignmentDraftTitleLengthRule(int MaxLength) : IManualAssignmentDraftRule
{
    public List<string> Check(ManualAssignmentDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssignmentDraftApprovedGuardRule(int Limit) : IManualAssignmentDraftRule
{
    public List<string> Check(ManualAssignmentDraft item)
    {
        if (item.Approved && item.ExpiresAt > Limit)
        {
            return new List<string> { $"expires at {item.ExpiresAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssignmentDraftAllowedCodeRule(List<string> Allowed) : IManualAssignmentDraftRule
{
    public List<string> Check(ManualAssignmentDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ManualAssignmentDraftChannelLengthRule(int MaxLength) : IManualAssignmentDraftRule
{
    public List<string> Check(ManualAssignmentDraft item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual assignment draft.</summary>
public sealed record ManualAssignmentDraftValidator(List<IManualAssignmentDraftRule> Rules)
{
    public List<string> Validate(ManualAssignmentDraft item)
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

    public void RequireValid(ManualAssignmentDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAssignmentDraft> items)
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

/// <summary>Functions of manual assignment draft rules.</summary>
public static class ManualAssignmentDraftRules
{
    public static ManualAssignmentDraftValidator DefaultManualAssignmentDraftValidator()
    {
        return new ManualAssignmentDraftValidator(
            new List<IManualAssignmentDraftRule>
            {
                new ManualAssignmentDraftTitleLengthRule(120),
                new ManualAssignmentDraftApprovedGuardRule(120),
                new ManualAssignmentDraftAllowedCodeRule(new List<string> { "pending", "fallback", "seasonal" }),
                new ManualAssignmentDraftChannelLengthRule(1000),
            });
    }
}
