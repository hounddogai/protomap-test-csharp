namespace Acme.Workforce.Assets.Warranty.Notice;

/// <summary>A rule a warranty notice must pass. Check returns its problems, or none.</summary>
public interface IWarrantyNoticeRule
{
    List<string> Check(WarrantyNotice item);
}

public sealed record WarrantyNoticeCostRangeRule(double Min, double Max) : IWarrantyNoticeRule
{
    public List<string> Check(WarrantyNotice item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyNoticeRatingRangeRule(double Min, double Max) : IWarrantyNoticeRule
{
    public List<string> Check(WarrantyNotice item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyNoticeCodePrefixRule(string Prefix) : IWarrantyNoticeRule
{
    public List<string> Check(WarrantyNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyNoticeBudgetRangeRule(double Min, double Max) : IWarrantyNoticeRule
{
    public List<string> Check(WarrantyNotice item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WarrantyNoticePhoneLengthRule(int MaxLength) : IWarrantyNoticeRule
{
    public List<string> Check(WarrantyNotice item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty notice.</summary>
public sealed record WarrantyNoticeValidator(List<IWarrantyNoticeRule> Rules)
{
    public List<string> Validate(WarrantyNotice item)
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

    public void RequireValid(WarrantyNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyNotice> items)
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

/// <summary>Functions of warranty notice rules.</summary>
public static class WarrantyNoticeRules
{
    public static WarrantyNoticeValidator DefaultWarrantyNoticeValidator()
    {
        return new WarrantyNoticeValidator(
            new List<IWarrantyNoticeRule>
            {
                new WarrantyNoticeCostRangeRule(2.8, 563.6),
                new WarrantyNoticeRatingRangeRule(4.3, 3149.5),
                new WarrantyNoticeCodePrefixRule("OPS"),
                new WarrantyNoticeBudgetRangeRule(3.4, 4784.5),
                new WarrantyNoticePhoneLengthRule(255),
            });
    }
}
