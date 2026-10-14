namespace Acme.Workforce.Assets.Laptop.Slot;

/// <summary>A rule a laptop target must pass. Check returns its problems, or none.</summary>
public interface ILaptopTargetRule
{
    List<string> Check(LaptopTarget item);
}

public sealed record LaptopTargetRequiredLabelRule(string Message) : ILaptopTargetRule
{
    public List<string> Check(LaptopTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopTargetCodeLengthRule(int MaxLength) : ILaptopTargetRule
{
    public List<string> Check(LaptopTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopTargetLocaleLengthRule(int MaxLength) : ILaptopTargetRule
{
    public List<string> Check(LaptopTarget item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopTargetRateRangeRule(double Min, double Max) : ILaptopTargetRule
{
    public List<string> Check(LaptopTarget item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a laptop target.</summary>
public sealed record LaptopTargetValidator(List<ILaptopTargetRule> Rules)
{
    public List<string> Validate(LaptopTarget item)
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

    public void RequireValid(LaptopTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopTarget> items)
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

/// <summary>Functions of laptop target rules.</summary>
public static class LaptopTargetRules
{
    public static LaptopTargetValidator DefaultLaptopTargetValidator()
    {
        return new LaptopTargetValidator(
            new List<ILaptopTargetRule>
            {
                new LaptopTargetRequiredLabelRule("laptop target needs this field"),
                new LaptopTargetCodeLengthRule(64),
                new LaptopTargetLocaleLengthRule(1000),
                new LaptopTargetRateRangeRule(7.4, 2603.1),
            });
    }
}
