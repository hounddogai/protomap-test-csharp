namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional license seat slot must pass. Check returns its problems, or none.</summary>
public interface IRegionalLicenseSeatSlotRule
{
    List<string> Check(RegionalLicenseSeatSlot item);
}

public sealed record RegionalLicenseSeatSlotAllowedCategoryRule(List<string> Allowed) : IRegionalLicenseSeatSlotRule
{
    public List<string> Check(RegionalLicenseSeatSlot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Category)))
        {
            return new List<string> { $"category {item.Category} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record RegionalLicenseSeatSlotRequiredCategoryRule(string Message) : IRegionalLicenseSeatSlotRule
{
    public List<string> Check(RegionalLicenseSeatSlot item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalLicenseSeatSlotWeightRangeRule(double Min, double Max) : IRegionalLicenseSeatSlotRule
{
    public List<string> Check(RegionalLicenseSeatSlot item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a regional license seat slot.</summary>
public sealed record RegionalLicenseSeatSlotValidator(List<IRegionalLicenseSeatSlotRule> Rules)
{
    public List<string> Validate(RegionalLicenseSeatSlot item)
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

    public void RequireValid(RegionalLicenseSeatSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalLicenseSeatSlot> items)
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

/// <summary>Functions of regional license seat slot rules.</summary>
public static class RegionalLicenseSeatSlotRules
{
    public static RegionalLicenseSeatSlotValidator DefaultRegionalLicenseSeatSlotValidator()
    {
        return new RegionalLicenseSeatSlotValidator(
            new List<IRegionalLicenseSeatSlotRule>
            {
                new RegionalLicenseSeatSlotAllowedCategoryRule(new List<string>()),
                new RegionalLicenseSeatSlotRequiredCategoryRule("regional license seat slot needs a value"),
                new RegionalLicenseSeatSlotWeightRangeRule(2.1, 3517.5),
            });
    }
}
