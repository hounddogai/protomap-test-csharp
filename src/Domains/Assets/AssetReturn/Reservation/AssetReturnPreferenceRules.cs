namespace Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>A rule a asset return preference must pass. Check returns its problems, or none.</summary>
public interface IAssetReturnPreferenceRule
{
    List<string> Check(AssetReturnPreference item);
}

public sealed record AssetReturnPreferenceCostRangeRule(double Min, double Max) : IAssetReturnPreferenceRule
{
    public List<string> Check(AssetReturnPreference item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPreferencePhoneLengthRule(int MaxLength) : IAssetReturnPreferenceRule
{
    public List<string> Check(AssetReturnPreference item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPreferenceRequiredCodeRule(string Message) : IAssetReturnPreferenceRule
{
    public List<string> Check(AssetReturnPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPreferenceRateRangeRule(double Min, double Max) : IAssetReturnPreferenceRule
{
    public List<string> Check(AssetReturnPreference item)
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

public sealed record AssetReturnPreferenceLeaveDaysRangeRule(int Min, int Max) : IAssetReturnPreferenceRule
{
    public List<string> Check(AssetReturnPreference item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssetReturnPreferenceRequiredLocationRule(string Message) : IAssetReturnPreferenceRule
{
    public List<string> Check(AssetReturnPreference item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a asset return preference.</summary>
public sealed record AssetReturnPreferenceValidator(List<IAssetReturnPreferenceRule> Rules)
{
    public List<string> Validate(AssetReturnPreference item)
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

    public void RequireValid(AssetReturnPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssetReturnPreference> items)
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

/// <summary>Functions of asset return preference rules.</summary>
public static class AssetReturnPreferenceRules
{
    public static AssetReturnPreferenceValidator DefaultAssetReturnPreferenceValidator()
    {
        return new AssetReturnPreferenceValidator(
            new List<IAssetReturnPreferenceRule>
            {
                new AssetReturnPreferenceCostRangeRule(2.0, 1268.9),
                new AssetReturnPreferencePhoneLengthRule(3600),
                new AssetReturnPreferenceRequiredCodeRule("asset return preference needs an entry"),
                new AssetReturnPreferenceRateRangeRule(7.7, 1809.1),
                new AssetReturnPreferenceLeaveDaysRangeRule(32, 86400),
                new AssetReturnPreferenceRequiredLocationRule("asset return preference needs this field"),
            });
    }
}
