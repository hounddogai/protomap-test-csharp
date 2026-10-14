namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>A rule a garnishment notice must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentNoticeRule
{
    List<string> Check(GarnishmentNotice item);
}

public sealed record GarnishmentNoticeTenureMonthsRangeRule(int Min, int Max) : IGarnishmentNoticeRule
{
    public List<string> Check(GarnishmentNotice item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentNoticeRequiredNameRule(string Message) : IGarnishmentNoticeRule
{
    public List<string> Check(GarnishmentNotice item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentNoticeDiscountRangeRule(double Min, double Max) : IGarnishmentNoticeRule
{
    public List<string> Check(GarnishmentNotice item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentNoticeRequiredGradeRule(string Message) : IGarnishmentNoticeRule
{
    public List<string> Check(GarnishmentNotice item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentNoticeGradeLengthRule(int MaxLength) : IGarnishmentNoticeRule
{
    public List<string> Check(GarnishmentNotice item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment notice.</summary>
public sealed record GarnishmentNoticeValidator(List<IGarnishmentNoticeRule> Rules)
{
    public List<string> Validate(GarnishmentNotice item)
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

    public void RequireValid(GarnishmentNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentNotice> items)
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

/// <summary>Functions of garnishment notice rules.</summary>
public static class GarnishmentNoticeRules
{
    public static GarnishmentNoticeValidator DefaultGarnishmentNoticeValidator()
    {
        return new GarnishmentNoticeValidator(
            new List<IGarnishmentNoticeRule>
            {
                new GarnishmentNoticeTenureMonthsRangeRule(32, 64),
                new GarnishmentNoticeRequiredNameRule("garnishment notice needs a value"),
                new GarnishmentNoticeDiscountRangeRule(2.2, 4965.8),
                new GarnishmentNoticeRequiredGradeRule("garnishment notice needs an entry"),
                new GarnishmentNoticeGradeLengthRule(255),
            });
    }
}
