namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

/// <summary>A rule a license seat notice must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatNoticeRule
{
    List<string> Check(LicenseSeatNotice item);
}

public sealed record LicenseSeatNoticeTitleLengthRule(int MaxLength) : ILicenseSeatNoticeRule
{
    public List<string> Check(LicenseSeatNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatNoticeRequiredCodeRule(string Message) : ILicenseSeatNoticeRule
{
    public List<string> Check(LicenseSeatNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatNoticeCodeLengthRule(int MaxLength) : ILicenseSeatNoticeRule
{
    public List<string> Check(LicenseSeatNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat notice.</summary>
public sealed record LicenseSeatNoticeValidator(List<ILicenseSeatNoticeRule> Rules)
{
    public List<string> Validate(LicenseSeatNotice item)
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

    public void RequireValid(LicenseSeatNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatNotice> items)
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

/// <summary>Functions of license seat notice rules.</summary>
public static class LicenseSeatNoticeRules
{
    public static LicenseSeatNoticeValidator DefaultLicenseSeatNoticeValidator()
    {
        return new LicenseSeatNoticeValidator(
            new List<ILicenseSeatNoticeRule>
            {
                new LicenseSeatNoticeTitleLengthRule(1000),
                new LicenseSeatNoticeRequiredCodeRule("license seat notice needs a value"),
                new LicenseSeatNoticeCodeLengthRule(3600),
            });
    }
}
