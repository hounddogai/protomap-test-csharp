namespace Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>A rule a rolling mobility notice must pass. Check returns its problems, or none.</summary>
public interface IRollingMobilityNoticeRule
{
    List<string> Check(RollingMobilityNotice item);
}

public sealed record RollingMobilityNoticeCodeLengthRule(int MaxLength) : IRollingMobilityNoticeRule
{
    public List<string> Check(RollingMobilityNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityNoticeRequiredCodeRule(string Message) : IRollingMobilityNoticeRule
{
    public List<string> Check(RollingMobilityNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingMobilityNoticeTenureMonthsRangeRule(int Min, int Max) : IRollingMobilityNoticeRule
{
    public List<string> Check(RollingMobilityNotice item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a rolling mobility notice.</summary>
public sealed record RollingMobilityNoticeValidator(List<IRollingMobilityNoticeRule> Rules)
{
    public List<string> Validate(RollingMobilityNotice item)
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

    public void RequireValid(RollingMobilityNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingMobilityNotice> items)
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

/// <summary>Functions of rolling mobility notice rules.</summary>
public static class RollingMobilityNoticeRules
{
    public static RollingMobilityNoticeValidator DefaultRollingMobilityNoticeValidator()
    {
        return new RollingMobilityNoticeValidator(
            new List<IRollingMobilityNoticeRule>
            {
                new RollingMobilityNoticeCodeLengthRule(32),
                new RollingMobilityNoticeRequiredCodeRule("rolling mobility notice needs a value"),
                new RollingMobilityNoticeTenureMonthsRangeRule(32, 32),
            });
    }
}
