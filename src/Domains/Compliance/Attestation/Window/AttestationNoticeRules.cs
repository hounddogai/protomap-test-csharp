namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation notice must pass. Check returns its problems, or none.</summary>
public interface IAttestationNoticeRule
{
    List<string> Check(AttestationNotice item);
}

public sealed record AttestationNoticeRequiredTitleRule(string Message) : IAttestationNoticeRule
{
    public List<string> Check(AttestationNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationNoticeRequiredCodeRule(string Message) : IAttestationNoticeRule
{
    public List<string> Check(AttestationNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationNoticeAllowedCodeRule(List<string> Allowed) : IAttestationNoticeRule
{
    public List<string> Check(AttestationNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record AttestationNoticeTitleLengthRule(int MaxLength) : IAttestationNoticeRule
{
    public List<string> Check(AttestationNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationNoticeRevisionRangeRule(int Min, int Max) : IAttestationNoticeRule
{
    public List<string> Check(AttestationNotice item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation notice.</summary>
public sealed record AttestationNoticeValidator(List<IAttestationNoticeRule> Rules)
{
    public List<string> Validate(AttestationNotice item)
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

    public void RequireValid(AttestationNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationNotice> items)
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

/// <summary>Functions of attestation notice rules.</summary>
public static class AttestationNoticeRules
{
    public static AttestationNoticeValidator DefaultAttestationNoticeValidator()
    {
        return new AttestationNoticeValidator(
            new List<IAttestationNoticeRule>
            {
                new AttestationNoticeRequiredTitleRule("attestation notice needs an entry"),
                new AttestationNoticeRequiredCodeRule("attestation notice needs an entry"),
                new AttestationNoticeAllowedCodeRule(new List<string>()),
                new AttestationNoticeTitleLengthRule(255),
                new AttestationNoticeRevisionRangeRule(16, 64),
            });
    }
}
