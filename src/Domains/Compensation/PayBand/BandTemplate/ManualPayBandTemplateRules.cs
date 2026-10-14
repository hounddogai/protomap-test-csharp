namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual pay band template must pass. Check returns its problems, or none.</summary>
public interface IManualPayBandTemplateRule
{
    List<string> Check(ManualPayBandTemplate item);
}

public sealed record ManualPayBandTemplateMaximumRangeRule(double Min, double Max) : IManualPayBandTemplateRule
{
    public List<string> Check(ManualPayBandTemplate item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPayBandTemplateRequiredNameRule(string Message) : IManualPayBandTemplateRule
{
    public List<string> Check(ManualPayBandTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPayBandTemplateQuantityRangeRule(int Min, int Max) : IManualPayBandTemplateRule
{
    public List<string> Check(ManualPayBandTemplate item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualPayBandTemplateCodePrefixRule(string Prefix) : IManualPayBandTemplateRule
{
    public List<string> Check(ManualPayBandTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPayBandTemplateSequenceRangeRule(int Min, int Max) : IManualPayBandTemplateRule
{
    public List<string> Check(ManualPayBandTemplate item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualPayBandTemplateAllowedCodeRule(List<string> Allowed) : IManualPayBandTemplateRule
{
    public List<string> Check(ManualPayBandTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual pay band template.</summary>
public sealed record ManualPayBandTemplateValidator(List<IManualPayBandTemplateRule> Rules)
{
    public List<string> Validate(ManualPayBandTemplate item)
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

    public void RequireValid(ManualPayBandTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPayBandTemplate> items)
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

/// <summary>Functions of manual pay band template rules.</summary>
public static class ManualPayBandTemplateRules
{
    public static ManualPayBandTemplateValidator DefaultManualPayBandTemplateValidator()
    {
        return new ManualPayBandTemplateValidator(
            new List<IManualPayBandTemplateRule>
            {
                new ManualPayBandTemplateMaximumRangeRule(1.2, 4824.4),
                new ManualPayBandTemplateRequiredNameRule("manual pay band template needs this field"),
                new ManualPayBandTemplateQuantityRangeRule(80, 3600),
                new ManualPayBandTemplateCodePrefixRule("OPS"),
                new ManualPayBandTemplateSequenceRangeRule(16, 3600),
                new ManualPayBandTemplateAllowedCodeRule(new List<string> { "tiered", "seasonal", "scheduled" }),
            });
    }
}
