namespace Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>A rule a retro pay notice must pass. Check returns its problems, or none.</summary>
public interface IRetroPayNoticeRule
{
    List<string> Check(RetroPayNotice item);
}

public sealed record RetroPayNoticeLabelLengthRule(int MaxLength) : IRetroPayNoticeRule
{
    public List<string> Check(RetroPayNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayNoticePinnedGuardRule(double Limit) : IRetroPayNoticeRule
{
    public List<string> Check(RetroPayNotice item)
    {
        if (item.Pinned && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayNoticeAmountRangeRule(double Min, double Max) : IRetroPayNoticeRule
{
    public List<string> Check(RetroPayNotice item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay notice.</summary>
public sealed record RetroPayNoticeValidator(List<IRetroPayNoticeRule> Rules)
{
    public List<string> Validate(RetroPayNotice item)
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

    public void RequireValid(RetroPayNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayNotice> items)
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

/// <summary>Functions of retro pay notice rules.</summary>
public static class RetroPayNoticeRules
{
    public static RetroPayNoticeValidator DefaultRetroPayNoticeValidator()
    {
        return new RetroPayNoticeValidator(
            new List<IRetroPayNoticeRule>
            {
                new RetroPayNoticeLabelLengthRule(86400),
                new RetroPayNoticePinnedGuardRule(2.6),
                new RetroPayNoticeAmountRangeRule(1.3, 4929.0),
            });
    }
}
