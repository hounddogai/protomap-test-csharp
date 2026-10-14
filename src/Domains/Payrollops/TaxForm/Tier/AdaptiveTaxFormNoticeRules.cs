namespace Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>A rule a adaptive tax form notice must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTaxFormNoticeRule
{
    List<string> Check(AdaptiveTaxFormNotice item);
}

public sealed record AdaptiveTaxFormNoticeCodeLengthRule(int MaxLength) : IAdaptiveTaxFormNoticeRule
{
    public List<string> Check(AdaptiveTaxFormNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTaxFormNoticeRequiredCodeRule(string Message) : IAdaptiveTaxFormNoticeRule
{
    public List<string> Check(AdaptiveTaxFormNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTaxFormNoticeRequiredTitleRule(string Message) : IAdaptiveTaxFormNoticeRule
{
    public List<string> Check(AdaptiveTaxFormNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive tax form notice.</summary>
public sealed record AdaptiveTaxFormNoticeValidator(List<IAdaptiveTaxFormNoticeRule> Rules)
{
    public List<string> Validate(AdaptiveTaxFormNotice item)
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

    public void RequireValid(AdaptiveTaxFormNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTaxFormNotice> items)
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

/// <summary>Functions of adaptive tax form notice rules.</summary>
public static class AdaptiveTaxFormNoticeRules
{
    public static AdaptiveTaxFormNoticeValidator DefaultAdaptiveTaxFormNoticeValidator()
    {
        return new AdaptiveTaxFormNoticeValidator(
            new List<IAdaptiveTaxFormNoticeRule>
            {
                new AdaptiveTaxFormNoticeCodeLengthRule(64),
                new AdaptiveTaxFormNoticeRequiredCodeRule("adaptive tax form notice needs this field"),
                new AdaptiveTaxFormNoticeRequiredTitleRule("adaptive tax form notice needs a value"),
            });
    }
}
