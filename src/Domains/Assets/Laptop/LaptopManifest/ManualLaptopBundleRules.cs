namespace Acme.Workforce.Assets.Laptop.LaptopManifest;

/// <summary>A rule a manual laptop bundle must pass. Check returns its problems, or none.</summary>
public interface IManualLaptopBundleRule
{
    List<string> Check(ManualLaptopBundle item);
}

public sealed record ManualLaptopBundleDiscountRangeRule(double Min, double Max) : IManualLaptopBundleRule
{
    public List<string> Check(ManualLaptopBundle item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualLaptopBundleRequiredCodeRule(string Message) : IManualLaptopBundleRule
{
    public List<string> Check(ManualLaptopBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualLaptopBundleNameLengthRule(int MaxLength) : IManualLaptopBundleRule
{
    public List<string> Check(ManualLaptopBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual laptop bundle.</summary>
public sealed record ManualLaptopBundleValidator(List<IManualLaptopBundleRule> Rules)
{
    public List<string> Validate(ManualLaptopBundle item)
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

    public void RequireValid(ManualLaptopBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualLaptopBundle> items)
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

/// <summary>Functions of manual laptop bundle rules.</summary>
public static class ManualLaptopBundleRules
{
    public static ManualLaptopBundleValidator DefaultManualLaptopBundleValidator()
    {
        return new ManualLaptopBundleValidator(
            new List<IManualLaptopBundleRule>
            {
                new ManualLaptopBundleDiscountRangeRule(1.6, 1740.5),
                new ManualLaptopBundleRequiredCodeRule("manual laptop bundle needs an entry"),
                new ManualLaptopBundleNameLengthRule(120),
            });
    }
}
