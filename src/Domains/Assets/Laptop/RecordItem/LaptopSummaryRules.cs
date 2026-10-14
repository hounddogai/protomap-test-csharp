namespace Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>A rule a laptop summary must pass. Check returns its problems, or none.</summary>
public interface ILaptopSummaryRule
{
    List<string> Check(LaptopSummary item);
}

public sealed record LaptopSummaryCodeLengthRule(int MaxLength) : ILaptopSummaryRule
{
    public List<string> Check(LaptopSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopSummaryRequiredCodeRule(string Message) : ILaptopSummaryRule
{
    public List<string> Check(LaptopSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopSummaryChannelLengthRule(int MaxLength) : ILaptopSummaryRule
{
    public List<string> Check(LaptopSummary item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopSummaryRequiredGradeRule(string Message) : ILaptopSummaryRule
{
    public List<string> Check(LaptopSummary item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopSummaryRequiredTitleRule(string Message) : ILaptopSummaryRule
{
    public List<string> Check(LaptopSummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop summary.</summary>
public sealed record LaptopSummaryValidator(List<ILaptopSummaryRule> Rules)
{
    public List<string> Validate(LaptopSummary item)
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

    public void RequireValid(LaptopSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopSummary> items)
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

/// <summary>Functions of laptop summary rules.</summary>
public static class LaptopSummaryRules
{
    public static LaptopSummaryValidator DefaultLaptopSummaryValidator()
    {
        return new LaptopSummaryValidator(
            new List<ILaptopSummaryRule>
            {
                new LaptopSummaryCodeLengthRule(32),
                new LaptopSummaryRequiredCodeRule("laptop summary needs a value"),
                new LaptopSummaryChannelLengthRule(32),
                new LaptopSummaryRequiredGradeRule("laptop summary needs an entry"),
                new LaptopSummaryRequiredTitleRule("laptop summary needs this field"),
            });
    }
}
