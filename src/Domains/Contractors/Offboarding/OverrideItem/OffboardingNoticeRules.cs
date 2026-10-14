namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>A rule a offboarding notice must pass. Check returns its problems, or none.</summary>
public interface IOffboardingNoticeRule
{
    List<string> Check(OffboardingNotice item);
}

public sealed record OffboardingNoticeRequiredCategoryRule(string Message) : IOffboardingNoticeRule
{
    public List<string> Check(OffboardingNotice item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingNoticeDiscountRangeRule(double Min, double Max) : IOffboardingNoticeRule
{
    public List<string> Check(OffboardingNotice item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingNoticeCodeLengthRule(int MaxLength) : IOffboardingNoticeRule
{
    public List<string> Check(OffboardingNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingNoticeCategoryLengthRule(int MaxLength) : IOffboardingNoticeRule
{
    public List<string> Check(OffboardingNotice item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingNoticeArchivedGuardRule(double Limit) : IOffboardingNoticeRule
{
    public List<string> Check(OffboardingNotice item)
    {
        if (item.Archived && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding notice.</summary>
public sealed record OffboardingNoticeValidator(List<IOffboardingNoticeRule> Rules)
{
    public List<string> Validate(OffboardingNotice item)
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

    public void RequireValid(OffboardingNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingNotice> items)
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

/// <summary>Functions of offboarding notice rules.</summary>
public static class OffboardingNoticeRules
{
    public static OffboardingNoticeValidator DefaultOffboardingNoticeValidator()
    {
        return new OffboardingNoticeValidator(
            new List<IOffboardingNoticeRule>
            {
                new OffboardingNoticeRequiredCategoryRule("offboarding notice needs an entry"),
                new OffboardingNoticeDiscountRangeRule(1.2, 1521.6),
                new OffboardingNoticeCodeLengthRule(16),
                new OffboardingNoticeCategoryLengthRule(32),
                new OffboardingNoticeArchivedGuardRule(4.1),
            });
    }
}
