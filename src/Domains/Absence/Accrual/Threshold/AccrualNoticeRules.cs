namespace Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>A rule a accrual notice must pass. Check returns its problems, or none.</summary>
public interface IAccrualNoticeRule
{
    List<string> Check(AccrualNotice item);
}

public sealed record AccrualNoticeWindowDaysRangeRule(int Min, int Max) : IAccrualNoticeRule
{
    public List<string> Check(AccrualNotice item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualNoticeRequiredCodeRule(string Message) : IAccrualNoticeRule
{
    public List<string> Check(AccrualNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualNoticeRequiredLabelRule(string Message) : IAccrualNoticeRule
{
    public List<string> Check(AccrualNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual notice.</summary>
public sealed record AccrualNoticeValidator(List<IAccrualNoticeRule> Rules)
{
    public List<string> Validate(AccrualNotice item)
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

    public void RequireValid(AccrualNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualNotice> items)
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

/// <summary>Functions of accrual notice rules.</summary>
public static class AccrualNoticeRules
{
    public static AccrualNoticeValidator DefaultAccrualNoticeValidator()
    {
        return new AccrualNoticeValidator(
            new List<IAccrualNoticeRule>
            {
                new AccrualNoticeWindowDaysRangeRule(80, 120),
                new AccrualNoticeRequiredCodeRule("accrual notice needs a value"),
                new AccrualNoticeRequiredLabelRule("accrual notice needs this field"),
            });
    }
}
