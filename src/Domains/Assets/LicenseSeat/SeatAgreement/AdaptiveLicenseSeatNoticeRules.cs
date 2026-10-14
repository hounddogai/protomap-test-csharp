namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>A rule a adaptive license seat notice must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveLicenseSeatNoticeRule
{
    List<string> Check(AdaptiveLicenseSeatNotice item);
}

public sealed record AdaptiveLicenseSeatNoticeCodePrefixRule(string Prefix) : IAdaptiveLicenseSeatNoticeRule
{
    public List<string> Check(AdaptiveLicenseSeatNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatNoticeTitleLengthRule(int MaxLength) : IAdaptiveLicenseSeatNoticeRule
{
    public List<string> Check(AdaptiveLicenseSeatNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatNoticeFteRangeRule(double Min, double Max) : IAdaptiveLicenseSeatNoticeRule
{
    public List<string> Check(AdaptiveLicenseSeatNotice item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveLicenseSeatNoticeRatioRangeRule(double Min, double Max) : IAdaptiveLicenseSeatNoticeRule
{
    public List<string> Check(AdaptiveLicenseSeatNotice item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveLicenseSeatNoticeTenureMonthsRangeRule(int Min, int Max) : IAdaptiveLicenseSeatNoticeRule
{
    public List<string> Check(AdaptiveLicenseSeatNotice item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive license seat notice.</summary>
public sealed record AdaptiveLicenseSeatNoticeValidator(List<IAdaptiveLicenseSeatNoticeRule> Rules)
{
    public List<string> Validate(AdaptiveLicenseSeatNotice item)
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

    public void RequireValid(AdaptiveLicenseSeatNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveLicenseSeatNotice> items)
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

/// <summary>Functions of adaptive license seat notice rules.</summary>
public static class AdaptiveLicenseSeatNoticeRules
{
    public static AdaptiveLicenseSeatNoticeValidator DefaultAdaptiveLicenseSeatNoticeValidator()
    {
        return new AdaptiveLicenseSeatNoticeValidator(
            new List<IAdaptiveLicenseSeatNoticeRule>
            {
                new AdaptiveLicenseSeatNoticeCodePrefixRule("ACM"),
                new AdaptiveLicenseSeatNoticeTitleLengthRule(1000),
                new AdaptiveLicenseSeatNoticeFteRangeRule(0.8, 3364.7),
                new AdaptiveLicenseSeatNoticeRatioRangeRule(3.1, 797.4),
                new AdaptiveLicenseSeatNoticeTenureMonthsRangeRule(3600, 3600),
            });
    }
}
