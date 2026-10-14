namespace Acme.Workforce.Assets.Warranty.Milestone;

/// <summary>A rule a seasonal warranty target must pass. Check returns its problems, or none.</summary>
public interface ISeasonalWarrantyTargetRule
{
    List<string> Check(SeasonalWarrantyTarget item);
}

public sealed record SeasonalWarrantyTargetFloorValueRangeRule(double Min, double Max) : ISeasonalWarrantyTargetRule
{
    public List<string> Check(SeasonalWarrantyTarget item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalWarrantyTargetExternalRefLengthRule(int MaxLength) : ISeasonalWarrantyTargetRule
{
    public List<string> Check(SeasonalWarrantyTarget item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalWarrantyTargetRequiredCountryRule(string Message) : ISeasonalWarrantyTargetRule
{
    public List<string> Check(SeasonalWarrantyTarget item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal warranty target.</summary>
public sealed record SeasonalWarrantyTargetValidator(List<ISeasonalWarrantyTargetRule> Rules)
{
    public List<string> Validate(SeasonalWarrantyTarget item)
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

    public void RequireValid(SeasonalWarrantyTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalWarrantyTarget> items)
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

/// <summary>Functions of seasonal warranty target rules.</summary>
public static class SeasonalWarrantyTargetRules
{
    public static SeasonalWarrantyTargetValidator DefaultSeasonalWarrantyTargetValidator()
    {
        return new SeasonalWarrantyTargetValidator(
            new List<ISeasonalWarrantyTargetRule>
            {
                new SeasonalWarrantyTargetFloorValueRangeRule(7.3, 3757.3),
                new SeasonalWarrantyTargetExternalRefLengthRule(120),
                new SeasonalWarrantyTargetRequiredCountryRule("seasonal warranty target needs a value"),
            });
    }
}
