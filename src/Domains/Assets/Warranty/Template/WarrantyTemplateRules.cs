namespace Acme.Workforce.Assets.Warranty.Template;

/// <summary>A rule a warranty template must pass. Check returns its problems, or none.</summary>
public interface IWarrantyTemplateRule
{
    List<string> Check(WarrantyTemplate item);
}

public sealed record WarrantyTemplateQuantityRangeRule(int Min, int Max) : IWarrantyTemplateRule
{
    public List<string> Check(WarrantyTemplate item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyTemplateRequiredOwnerIdRule(string Message) : IWarrantyTemplateRule
{
    public List<string> Check(WarrantyTemplate item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WarrantyTemplateNameLengthRule(int MaxLength) : IWarrantyTemplateRule
{
    public List<string> Check(WarrantyTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty template.</summary>
public sealed record WarrantyTemplateValidator(List<IWarrantyTemplateRule> Rules)
{
    public List<string> Validate(WarrantyTemplate item)
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

    public void RequireValid(WarrantyTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyTemplate> items)
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

/// <summary>Functions of warranty template rules.</summary>
public static class WarrantyTemplateRules
{
    public static WarrantyTemplateValidator DefaultWarrantyTemplateValidator()
    {
        return new WarrantyTemplateValidator(
            new List<IWarrantyTemplateRule>
            {
                new WarrantyTemplateQuantityRangeRule(80, 120),
                new WarrantyTemplateRequiredOwnerIdRule("warranty template needs this field"),
                new WarrantyTemplateNameLengthRule(120),
            });
    }
}
