namespace Acme.Workforce.Payrollops.Garnishment.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a garnishment threshold must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentThresholdRule
{
    List<string> Check(GarnishmentThreshold item);
}

public sealed record GarnishmentThresholdCodeLengthRule(int MaxLength) : IGarnishmentThresholdRule
{
    public List<string> Check(GarnishmentThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentThresholdAllowedContactEmailRule(List<string> Allowed) : IGarnishmentThresholdRule
{
    public List<string> Check(GarnishmentThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentThresholdRequiredLabelRule(string Message) : IGarnishmentThresholdRule
{
    public List<string> Check(GarnishmentThreshold item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentThresholdScoreRangeRule(double Min, double Max) : IGarnishmentThresholdRule
{
    public List<string> Check(GarnishmentThreshold item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentThresholdSalaryBandLengthRule(int MaxLength) : IGarnishmentThresholdRule
{
    public List<string> Check(GarnishmentThreshold item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentThresholdRequiredSalaryBandRule(string Message) : IGarnishmentThresholdRule
{
    public List<string> Check(GarnishmentThreshold item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment threshold.</summary>
public sealed record GarnishmentThresholdValidator(List<IGarnishmentThresholdRule> Rules)
{
    public List<string> Validate(GarnishmentThreshold item)
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

    public void RequireValid(GarnishmentThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentThreshold> items)
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

/// <summary>Functions of garnishment threshold rules.</summary>
public static class GarnishmentThresholdRules
{
    public static GarnishmentThresholdValidator DefaultGarnishmentThresholdValidator()
    {
        return new GarnishmentThresholdValidator(
            new List<IGarnishmentThresholdRule>
            {
                new GarnishmentThresholdCodeLengthRule(255),
                new GarnishmentThresholdAllowedContactEmailRule(new List<string> { "legacy" }),
                new GarnishmentThresholdRequiredLabelRule("garnishment threshold needs this field"),
                new GarnishmentThresholdScoreRangeRule(5.8, 2127.1),
                new GarnishmentThresholdSalaryBandLengthRule(64),
                new GarnishmentThresholdRequiredSalaryBandRule("garnishment threshold needs a value"),
            });
    }
}
