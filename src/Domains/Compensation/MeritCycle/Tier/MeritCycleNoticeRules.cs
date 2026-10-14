namespace Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>A rule a merit cycle notice must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleNoticeRule
{
    List<string> Check(MeritCycleNotice item);
}

public sealed record MeritCycleNoticeRequiredTitleRule(string Message) : IMeritCycleNoticeRule
{
    public List<string> Check(MeritCycleNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleNoticeRequiredCodeRule(string Message) : IMeritCycleNoticeRule
{
    public List<string> Check(MeritCycleNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleNoticeLabelLengthRule(int MaxLength) : IMeritCycleNoticeRule
{
    public List<string> Check(MeritCycleNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleNoticeRequiredSalaryBandRule(string Message) : IMeritCycleNoticeRule
{
    public List<string> Check(MeritCycleNotice item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleNoticeSalaryBandLengthRule(int MaxLength) : IMeritCycleNoticeRule
{
    public List<string> Check(MeritCycleNotice item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleNoticeTitleLengthRule(int MaxLength) : IMeritCycleNoticeRule
{
    public List<string> Check(MeritCycleNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle notice.</summary>
public sealed record MeritCycleNoticeValidator(List<IMeritCycleNoticeRule> Rules)
{
    public List<string> Validate(MeritCycleNotice item)
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

    public void RequireValid(MeritCycleNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleNotice> items)
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

/// <summary>Functions of merit cycle notice rules.</summary>
public static class MeritCycleNoticeRules
{
    public static MeritCycleNoticeValidator DefaultMeritCycleNoticeValidator()
    {
        return new MeritCycleNoticeValidator(
            new List<IMeritCycleNoticeRule>
            {
                new MeritCycleNoticeRequiredTitleRule("merit cycle notice needs this field"),
                new MeritCycleNoticeRequiredCodeRule("merit cycle notice needs a value"),
                new MeritCycleNoticeLabelLengthRule(120),
                new MeritCycleNoticeRequiredSalaryBandRule("merit cycle notice needs an entry"),
                new MeritCycleNoticeSalaryBandLengthRule(80),
                new MeritCycleNoticeTitleLengthRule(255),
            });
    }
}
