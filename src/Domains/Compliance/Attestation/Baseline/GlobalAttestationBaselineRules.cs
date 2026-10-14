namespace Acme.Workforce.Compliance.Attestation.Baseline;

/// <summary>A rule a global attestation baseline must pass. Check returns its problems, or none.</summary>
public interface IGlobalAttestationBaselineRule
{
    List<string> Check(GlobalAttestationBaseline item);
}

public sealed record GlobalAttestationBaselineManagerIdLengthRule(int MaxLength) : IGlobalAttestationBaselineRule
{
    public List<string> Check(GlobalAttestationBaseline item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBaselineQuantityRangeRule(int Min, int Max) : IGlobalAttestationBaselineRule
{
    public List<string> Check(GlobalAttestationBaseline item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBaselineLabelLengthRule(int MaxLength) : IGlobalAttestationBaselineRule
{
    public List<string> Check(GlobalAttestationBaseline item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBaselineRequiredCodeRule(string Message) : IGlobalAttestationBaselineRule
{
    public List<string> Check(GlobalAttestationBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBaselineExpiresAtRangeRule(int Min, int Max) : IGlobalAttestationBaselineRule
{
    public List<string> Check(GlobalAttestationBaseline item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBaselineCodeLengthRule(int MaxLength) : IGlobalAttestationBaselineRule
{
    public List<string> Check(GlobalAttestationBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global attestation baseline.</summary>
public sealed record GlobalAttestationBaselineValidator(List<IGlobalAttestationBaselineRule> Rules)
{
    public List<string> Validate(GlobalAttestationBaseline item)
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

    public void RequireValid(GlobalAttestationBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAttestationBaseline> items)
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

/// <summary>Functions of global attestation baseline rules.</summary>
public static class GlobalAttestationBaselineRules
{
    public static GlobalAttestationBaselineValidator DefaultGlobalAttestationBaselineValidator()
    {
        return new GlobalAttestationBaselineValidator(
            new List<IGlobalAttestationBaselineRule>
            {
                new GlobalAttestationBaselineManagerIdLengthRule(120),
                new GlobalAttestationBaselineQuantityRangeRule(64, 1000),
                new GlobalAttestationBaselineLabelLengthRule(1000),
                new GlobalAttestationBaselineRequiredCodeRule("global attestation baseline needs an entry"),
                new GlobalAttestationBaselineExpiresAtRangeRule(255, 86400),
                new GlobalAttestationBaselineCodeLengthRule(86400),
            });
    }
}
