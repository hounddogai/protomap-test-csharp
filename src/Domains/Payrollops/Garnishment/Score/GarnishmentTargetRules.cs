namespace Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>A rule a garnishment target must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentTargetRule
{
    List<string> Check(GarnishmentTarget item);
}

public sealed record GarnishmentTargetRequiredPhoneRule(string Message) : IGarnishmentTargetRule
{
    public List<string> Check(GarnishmentTarget item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTargetTitleLengthRule(int MaxLength) : IGarnishmentTargetRule
{
    public List<string> Check(GarnishmentTarget item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentTargetRatioRangeRule(double Min, double Max) : IGarnishmentTargetRule
{
    public List<string> Check(GarnishmentTarget item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentTargetCountryLengthRule(int MaxLength) : IGarnishmentTargetRule
{
    public List<string> Check(GarnishmentTarget item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment target.</summary>
public sealed record GarnishmentTargetValidator(List<IGarnishmentTargetRule> Rules)
{
    public List<string> Validate(GarnishmentTarget item)
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

    public void RequireValid(GarnishmentTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentTarget> items)
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

/// <summary>Functions of garnishment target rules.</summary>
public static class GarnishmentTargetRules
{
    public static GarnishmentTargetValidator DefaultGarnishmentTargetValidator()
    {
        return new GarnishmentTargetValidator(
            new List<IGarnishmentTargetRule>
            {
                new GarnishmentTargetRequiredPhoneRule("garnishment target needs this field"),
                new GarnishmentTargetTitleLengthRule(64),
                new GarnishmentTargetRatioRangeRule(3.9, 1140.2),
                new GarnishmentTargetCountryLengthRule(32),
            });
    }
}
