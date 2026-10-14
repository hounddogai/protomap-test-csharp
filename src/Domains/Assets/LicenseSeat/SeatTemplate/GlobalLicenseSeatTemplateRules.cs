namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>A rule a global license seat template must pass. Check returns its problems, or none.</summary>
public interface IGlobalLicenseSeatTemplateRule
{
    List<string> Check(GlobalLicenseSeatTemplate item);
}

public sealed record GlobalLicenseSeatTemplateCodePrefixRule(string Prefix) : IGlobalLicenseSeatTemplateRule
{
    public List<string> Check(GlobalLicenseSeatTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLicenseSeatTemplateRatingRangeRule(double Min, double Max) : IGlobalLicenseSeatTemplateRule
{
    public List<string> Check(GlobalLicenseSeatTemplate item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLicenseSeatTemplateRequiredChecksumRule(string Message) : IGlobalLicenseSeatTemplateRule
{
    public List<string> Check(GlobalLicenseSeatTemplate item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global license seat template.</summary>
public sealed record GlobalLicenseSeatTemplateValidator(List<IGlobalLicenseSeatTemplateRule> Rules)
{
    public List<string> Validate(GlobalLicenseSeatTemplate item)
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

    public void RequireValid(GlobalLicenseSeatTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalLicenseSeatTemplate> items)
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

/// <summary>Functions of global license seat template rules.</summary>
public static class GlobalLicenseSeatTemplateRules
{
    public static GlobalLicenseSeatTemplateValidator DefaultGlobalLicenseSeatTemplateValidator()
    {
        return new GlobalLicenseSeatTemplateValidator(
            new List<IGlobalLicenseSeatTemplateRule>
            {
                new GlobalLicenseSeatTemplateCodePrefixRule("REF"),
                new GlobalLicenseSeatTemplateRatingRangeRule(8.3, 4769.8),
                new GlobalLicenseSeatTemplateRequiredChecksumRule("global license seat template needs this field"),
            });
    }
}
