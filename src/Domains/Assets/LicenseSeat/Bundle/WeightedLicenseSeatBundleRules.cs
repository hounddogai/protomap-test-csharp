namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted license seat bundle must pass. Check returns its problems, or none.</summary>
public interface IWeightedLicenseSeatBundleRule
{
    List<string> Check(WeightedLicenseSeatBundle item);
}

public sealed record WeightedLicenseSeatBundleCodeLengthRule(int MaxLength) : IWeightedLicenseSeatBundleRule
{
    public List<string> Check(WeightedLicenseSeatBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatBundleRequiredLabelRule(string Message) : IWeightedLicenseSeatBundleRule
{
    public List<string> Check(WeightedLicenseSeatBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatBundleLabelLengthRule(int MaxLength) : IWeightedLicenseSeatBundleRule
{
    public List<string> Check(WeightedLicenseSeatBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatBundleAllowedLabelRule(List<string> Allowed) : IWeightedLicenseSeatBundleRule
{
    public List<string> Check(WeightedLicenseSeatBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatBundleRequiredGradeRule(string Message) : IWeightedLicenseSeatBundleRule
{
    public List<string> Check(WeightedLicenseSeatBundle item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedLicenseSeatBundleGradeLengthRule(int MaxLength) : IWeightedLicenseSeatBundleRule
{
    public List<string> Check(WeightedLicenseSeatBundle item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted license seat bundle.</summary>
public sealed record WeightedLicenseSeatBundleValidator(List<IWeightedLicenseSeatBundleRule> Rules)
{
    public List<string> Validate(WeightedLicenseSeatBundle item)
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

    public void RequireValid(WeightedLicenseSeatBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedLicenseSeatBundle> items)
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

/// <summary>Functions of weighted license seat bundle rules.</summary>
public static class WeightedLicenseSeatBundleRules
{
    public static WeightedLicenseSeatBundleValidator DefaultWeightedLicenseSeatBundleValidator()
    {
        return new WeightedLicenseSeatBundleValidator(
            new List<IWeightedLicenseSeatBundleRule>
            {
                new WeightedLicenseSeatBundleCodeLengthRule(80),
                new WeightedLicenseSeatBundleRequiredLabelRule("weighted license seat bundle needs a value"),
                new WeightedLicenseSeatBundleLabelLengthRule(3600),
                new WeightedLicenseSeatBundleAllowedLabelRule(new List<string>()),
                new WeightedLicenseSeatBundleRequiredGradeRule("weighted license seat bundle needs a value"),
                new WeightedLicenseSeatBundleGradeLengthRule(32),
            });
    }
}
