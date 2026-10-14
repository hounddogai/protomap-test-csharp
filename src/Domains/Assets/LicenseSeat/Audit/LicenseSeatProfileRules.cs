namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat profile must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatProfileRule
{
    List<string> Check(LicenseSeatProfile item);
}

public sealed record LicenseSeatProfileCostRangeRule(double Min, double Max) : ILicenseSeatProfileRule
{
    public List<string> Check(LicenseSeatProfile item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatProfileCodeLengthRule(int MaxLength) : ILicenseSeatProfileRule
{
    public List<string> Check(LicenseSeatProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatProfileTitleLengthRule(int MaxLength) : ILicenseSeatProfileRule
{
    public List<string> Check(LicenseSeatProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatProfileAllowedRegionRule(List<string> Allowed) : ILicenseSeatProfileRule
{
    public List<string> Check(LicenseSeatProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat profile.</summary>
public sealed record LicenseSeatProfileValidator(List<ILicenseSeatProfileRule> Rules)
{
    public List<string> Validate(LicenseSeatProfile item)
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

    public void RequireValid(LicenseSeatProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatProfile> items)
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

/// <summary>Functions of license seat profile rules.</summary>
public static class LicenseSeatProfileRules
{
    public static LicenseSeatProfileValidator DefaultLicenseSeatProfileValidator()
    {
        return new LicenseSeatProfileValidator(
            new List<ILicenseSeatProfileRule>
            {
                new LicenseSeatProfileCostRangeRule(4.3, 797.5),
                new LicenseSeatProfileCodeLengthRule(16),
                new LicenseSeatProfileTitleLengthRule(3600),
                new LicenseSeatProfileAllowedRegionRule(new List<string> { "lenient", "legacy" }),
            });
    }
}
