namespace Acme.Workforce.Workplace.Visitor.RecordItem;

/// <summary>A rule a visitor adjustment must pass. Check returns its problems, or none.</summary>
public interface IVisitorAdjustmentRule
{
    List<string> Check(VisitorAdjustment item);
}

public sealed record VisitorAdjustmentTitleLengthRule(int MaxLength) : IVisitorAdjustmentRule
{
    public List<string> Check(VisitorAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAdjustmentAttemptsRangeRule(int Min, int Max) : IVisitorAdjustmentRule
{
    public List<string> Check(VisitorAdjustment item)
    {
        var valueValue = item.Attempts;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"attempts {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"attempts {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorAdjustmentCodeLengthRule(int MaxLength) : IVisitorAdjustmentRule
{
    public List<string> Check(VisitorAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAdjustmentWindowDaysRangeRule(int Min, int Max) : IVisitorAdjustmentRule
{
    public List<string> Check(VisitorAdjustment item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record VisitorAdjustmentQuantityRangeRule(int Min, int Max) : IVisitorAdjustmentRule
{
    public List<string> Check(VisitorAdjustment item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAdjustmentRequiredChannelRule(string Message) : IVisitorAdjustmentRule
{
    public List<string> Check(VisitorAdjustment item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor adjustment.</summary>
public sealed record VisitorAdjustmentValidator(List<IVisitorAdjustmentRule> Rules)
{
    public List<string> Validate(VisitorAdjustment item)
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

    public void RequireValid(VisitorAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorAdjustment> items)
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

/// <summary>Functions of visitor adjustment rules.</summary>
public static class VisitorAdjustmentRules
{
    public static VisitorAdjustmentValidator DefaultVisitorAdjustmentValidator()
    {
        return new VisitorAdjustmentValidator(
            new List<IVisitorAdjustmentRule>
            {
                new VisitorAdjustmentTitleLengthRule(16),
                new VisitorAdjustmentAttemptsRangeRule(120, 120),
                new VisitorAdjustmentCodeLengthRule(86400),
                new VisitorAdjustmentWindowDaysRangeRule(3600, 3600),
                new VisitorAdjustmentQuantityRangeRule(16, 120),
                new VisitorAdjustmentRequiredChannelRule("visitor adjustment needs a value"),
            });
    }
}
