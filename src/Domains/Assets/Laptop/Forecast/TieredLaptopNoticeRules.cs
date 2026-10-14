namespace Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>A rule a tiered laptop notice must pass. Check returns its problems, or none.</summary>
public interface ITieredLaptopNoticeRule
{
    List<string> Check(TieredLaptopNotice item);
}

public sealed record TieredLaptopNoticeRevisionRangeRule(int Min, int Max) : ITieredLaptopNoticeRule
{
    public List<string> Check(TieredLaptopNotice item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TieredLaptopNoticeMaximumRangeRule(double Min, double Max) : ITieredLaptopNoticeRule
{
    public List<string> Check(TieredLaptopNotice item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredLaptopNoticeCodeLengthRule(int MaxLength) : ITieredLaptopNoticeRule
{
    public List<string> Check(TieredLaptopNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered laptop notice.</summary>
public sealed record TieredLaptopNoticeValidator(List<ITieredLaptopNoticeRule> Rules)
{
    public List<string> Validate(TieredLaptopNotice item)
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

    public void RequireValid(TieredLaptopNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredLaptopNotice> items)
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

/// <summary>Functions of tiered laptop notice rules.</summary>
public static class TieredLaptopNoticeRules
{
    public static TieredLaptopNoticeValidator DefaultTieredLaptopNoticeValidator()
    {
        return new TieredLaptopNoticeValidator(
            new List<ITieredLaptopNoticeRule>
            {
                new TieredLaptopNoticeRevisionRangeRule(32, 80),
                new TieredLaptopNoticeMaximumRangeRule(8.8, 271.3),
                new TieredLaptopNoticeCodeLengthRule(1000),
            });
    }
}
