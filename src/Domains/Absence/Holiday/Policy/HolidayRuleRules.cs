namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday rule must pass. Check returns its problems, or none.</summary>
public interface IHolidayRuleRule
{
    List<string> Check(HolidayRule item);
}

public sealed record HolidayRuleSalaryBandLengthRule(int MaxLength) : IHolidayRuleRule
{
    public List<string> Check(HolidayRule item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayRuleAllowedTitleRule(List<string> Allowed) : IHolidayRuleRule
{
    public List<string> Check(HolidayRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record HolidayRuleRequiredGradeRule(string Message) : IHolidayRuleRule
{
    public List<string> Check(HolidayRule item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayRuleMaximumRangeRule(double Min, double Max) : IHolidayRuleRule
{
    public List<string> Check(HolidayRule item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record HolidayRuleRequiredSalaryBandRule(string Message) : IHolidayRuleRule
{
    public List<string> Check(HolidayRule item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday rule.</summary>
public sealed record HolidayRuleValidator(List<IHolidayRuleRule> Rules)
{
    public List<string> Validate(HolidayRule item)
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

    public void RequireValid(HolidayRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayRule> items)
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

/// <summary>Functions of holiday rule rules.</summary>
public static class HolidayRuleRules
{
    public static HolidayRuleValidator DefaultHolidayRuleValidator()
    {
        return new HolidayRuleValidator(
            new List<IHolidayRuleRule>
            {
                new HolidayRuleSalaryBandLengthRule(80),
                new HolidayRuleAllowedTitleRule(new List<string> { "seasonal", "tiered" }),
                new HolidayRuleRequiredGradeRule("holiday rule needs this field"),
                new HolidayRuleMaximumRangeRule(0.2, 796.4),
                new HolidayRuleRequiredSalaryBandRule("holiday rule needs a value"),
            });
    }
}
