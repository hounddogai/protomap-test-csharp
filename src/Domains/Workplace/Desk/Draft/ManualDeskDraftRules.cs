namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual desk draft must pass. Check returns its problems, or none.</summary>
public interface IManualDeskDraftRule
{
    List<string> Check(ManualDeskDraft item);
}

public sealed record ManualDeskDraftScoreRangeRule(double Min, double Max) : IManualDeskDraftRule
{
    public List<string> Check(ManualDeskDraft item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualDeskDraftAllowedChecksumRule(List<string> Allowed) : IManualDeskDraftRule
{
    public List<string> Check(ManualDeskDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ManualDeskDraftCodeLengthRule(int MaxLength) : IManualDeskDraftRule
{
    public List<string> Check(ManualDeskDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualDeskDraftArchivedGuardRule(double Limit) : IManualDeskDraftRule
{
    public List<string> Check(ManualDeskDraft item)
    {
        if (item.Archived && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ManualDeskDraftRequiredCodeRule(string Message) : IManualDeskDraftRule
{
    public List<string> Check(ManualDeskDraft item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualDeskDraftChecksumLengthRule(int MaxLength) : IManualDeskDraftRule
{
    public List<string> Check(ManualDeskDraft item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual desk draft.</summary>
public sealed record ManualDeskDraftValidator(List<IManualDeskDraftRule> Rules)
{
    public List<string> Validate(ManualDeskDraft item)
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

    public void RequireValid(ManualDeskDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualDeskDraft> items)
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

/// <summary>Functions of manual desk draft rules.</summary>
public static class ManualDeskDraftRules
{
    public static ManualDeskDraftValidator DefaultManualDeskDraftValidator()
    {
        return new ManualDeskDraftValidator(
            new List<IManualDeskDraftRule>
            {
                new ManualDeskDraftScoreRangeRule(1.8, 4888.7),
                new ManualDeskDraftAllowedChecksumRule(new List<string> { "fallback", "legacy" }),
                new ManualDeskDraftCodeLengthRule(3600),
                new ManualDeskDraftArchivedGuardRule(5.5),
                new ManualDeskDraftRequiredCodeRule("manual desk draft needs an entry"),
                new ManualDeskDraftChecksumLengthRule(16),
            });
    }
}
