namespace Acme.Workforce.Talent.Mobility.Profile;

/// <summary>A rule a mobility baseline must pass. Check returns its problems, or none.</summary>
public interface IMobilityBaselineRule
{
    List<string> Check(MobilityBaseline item);
}

public sealed record MobilityBaselineTitleLengthRule(int MaxLength) : IMobilityBaselineRule
{
    public List<string> Check(MobilityBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityBaselineCostRangeRule(double Min, double Max) : IMobilityBaselineRule
{
    public List<string> Check(MobilityBaseline item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityBaselineExpiresAtRangeRule(int Min, int Max) : IMobilityBaselineRule
{
    public List<string> Check(MobilityBaseline item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityBaselineRequiredLabelRule(string Message) : IMobilityBaselineRule
{
    public List<string> Check(MobilityBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityBaselineCodeLengthRule(int MaxLength) : IMobilityBaselineRule
{
    public List<string> Check(MobilityBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility baseline.</summary>
public sealed record MobilityBaselineValidator(List<IMobilityBaselineRule> Rules)
{
    public List<string> Validate(MobilityBaseline item)
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

    public void RequireValid(MobilityBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityBaseline> items)
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

/// <summary>Functions of mobility baseline rules.</summary>
public static class MobilityBaselineRules
{
    public static MobilityBaselineValidator DefaultMobilityBaselineValidator()
    {
        return new MobilityBaselineValidator(
            new List<IMobilityBaselineRule>
            {
                new MobilityBaselineTitleLengthRule(120),
                new MobilityBaselineCostRangeRule(6.3, 719.1),
                new MobilityBaselineExpiresAtRangeRule(64, 3600),
                new MobilityBaselineRequiredLabelRule("mobility baseline needs a value"),
                new MobilityBaselineCodeLengthRule(80),
            });
    }
}
