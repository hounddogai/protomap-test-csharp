namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>A rule a lenient tax form notice must pass. Check returns its problems, or none.</summary>
public interface ILenientTaxFormNoticeRule
{
    List<string> Check(LenientTaxFormNotice item);
}

public sealed record LenientTaxFormNoticeHoursRangeRule(double Min, double Max) : ILenientTaxFormNoticeRule
{
    public List<string> Check(LenientTaxFormNotice item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTaxFormNoticeSkillLengthRule(int MaxLength) : ILenientTaxFormNoticeRule
{
    public List<string> Check(LenientTaxFormNotice item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTaxFormNoticeRequiredCodeRule(string Message) : ILenientTaxFormNoticeRule
{
    public List<string> Check(LenientTaxFormNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient tax form notice.</summary>
public sealed record LenientTaxFormNoticeValidator(List<ILenientTaxFormNoticeRule> Rules)
{
    public List<string> Validate(LenientTaxFormNotice item)
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

    public void RequireValid(LenientTaxFormNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientTaxFormNotice> items)
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

/// <summary>Functions of lenient tax form notice rules.</summary>
public static class LenientTaxFormNoticeRules
{
    public static LenientTaxFormNoticeValidator DefaultLenientTaxFormNoticeValidator()
    {
        return new LenientTaxFormNoticeValidator(
            new List<ILenientTaxFormNoticeRule>
            {
                new LenientTaxFormNoticeHoursRangeRule(3.2, 1083.0),
                new LenientTaxFormNoticeSkillLengthRule(255),
                new LenientTaxFormNoticeRequiredCodeRule("lenient tax form notice needs this field"),
            });
    }
}
