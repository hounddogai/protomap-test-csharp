namespace Acme.Workforce.Talent.Mobility.Target;

/// <summary>A rule a mobility notice must pass. Check returns its problems, or none.</summary>
public interface IMobilityNoticeRule
{
    List<string> Check(MobilityNotice item);
}

public sealed record MobilityNoticeRequiredUnitRule(string Message) : IMobilityNoticeRule
{
    public List<string> Check(MobilityNotice item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityNoticeUnitLengthRule(int MaxLength) : IMobilityNoticeRule
{
    public List<string> Check(MobilityNotice item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityNoticeCostRangeRule(double Min, double Max) : IMobilityNoticeRule
{
    public List<string> Check(MobilityNotice item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a mobility notice.</summary>
public sealed record MobilityNoticeValidator(List<IMobilityNoticeRule> Rules)
{
    public List<string> Validate(MobilityNotice item)
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

    public void RequireValid(MobilityNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityNotice> items)
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

/// <summary>Functions of mobility notice rules.</summary>
public static class MobilityNoticeRules
{
    public static MobilityNoticeValidator DefaultMobilityNoticeValidator()
    {
        return new MobilityNoticeValidator(
            new List<IMobilityNoticeRule>
            {
                new MobilityNoticeRequiredUnitRule("mobility notice needs a value"),
                new MobilityNoticeUnitLengthRule(86400),
                new MobilityNoticeCostRangeRule(1.2, 4517.2),
            });
    }
}
