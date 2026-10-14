namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default license seat mapping must pass. Check returns its problems, or none.</summary>
public interface IDefaultLicenseSeatMappingRule
{
    List<string> Check(DefaultLicenseSeatMapping item);
}

public sealed record DefaultLicenseSeatMappingPriorityRangeRule(int Min, int Max) : IDefaultLicenseSeatMappingRule
{
    public List<string> Check(DefaultLicenseSeatMapping item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultLicenseSeatMappingBatchSizeRangeRule(int Min, int Max) : IDefaultLicenseSeatMappingRule
{
    public List<string> Check(DefaultLicenseSeatMapping item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultLicenseSeatMappingAllowedCodeRule(List<string> Allowed) : IDefaultLicenseSeatMappingRule
{
    public List<string> Check(DefaultLicenseSeatMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatMappingRateRangeRule(double Min, double Max) : IDefaultLicenseSeatMappingRule
{
    public List<string> Check(DefaultLicenseSeatMapping item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatMappingRequiredTitleRule(string Message) : IDefaultLicenseSeatMappingRule
{
    public List<string> Check(DefaultLicenseSeatMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLicenseSeatMappingTitleLengthRule(int MaxLength) : IDefaultLicenseSeatMappingRule
{
    public List<string> Check(DefaultLicenseSeatMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default license seat mapping.</summary>
public sealed record DefaultLicenseSeatMappingValidator(List<IDefaultLicenseSeatMappingRule> Rules)
{
    public List<string> Validate(DefaultLicenseSeatMapping item)
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

    public void RequireValid(DefaultLicenseSeatMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLicenseSeatMapping> items)
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

/// <summary>Functions of default license seat mapping rules.</summary>
public static class DefaultLicenseSeatMappingRules
{
    public static DefaultLicenseSeatMappingValidator DefaultDefaultLicenseSeatMappingValidator()
    {
        return new DefaultLicenseSeatMappingValidator(
            new List<IDefaultLicenseSeatMappingRule>
            {
                new DefaultLicenseSeatMappingPriorityRangeRule(16, 255),
                new DefaultLicenseSeatMappingBatchSizeRangeRule(86400, 86400),
                new DefaultLicenseSeatMappingAllowedCodeRule(new List<string> { "seasonal" }),
                new DefaultLicenseSeatMappingRateRangeRule(7.5, 713.8),
                new DefaultLicenseSeatMappingRequiredTitleRule("default license seat mapping needs an entry"),
                new DefaultLicenseSeatMappingTitleLengthRule(120),
            });
    }
}
