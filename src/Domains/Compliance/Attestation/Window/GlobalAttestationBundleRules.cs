namespace Acme.Workforce.Compliance.Attestation.Window;

/// <summary>A rule a global attestation bundle must pass. Check returns its problems, or none.</summary>
public interface IGlobalAttestationBundleRule
{
    List<string> Check(GlobalAttestationBundle item);
}

public sealed record GlobalAttestationBundleRequiredNameRule(string Message) : IGlobalAttestationBundleRule
{
    public List<string> Check(GlobalAttestationBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBundleLabelLengthRule(int MaxLength) : IGlobalAttestationBundleRule
{
    public List<string> Check(GlobalAttestationBundle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBundleCodeLengthRule(int MaxLength) : IGlobalAttestationBundleRule
{
    public List<string> Check(GlobalAttestationBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBundlePinnedGuardRule(int Limit) : IGlobalAttestationBundleRule
{
    public List<string> Check(GlobalAttestationBundle item)
    {
        if (item.Pinned && item.BatchSize > Limit)
        {
            return new List<string> { $"batch size {item.BatchSize} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAttestationBundleCapacityRangeRule(int Min, int Max) : IGlobalAttestationBundleRule
{
    public List<string> Check(GlobalAttestationBundle item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global attestation bundle.</summary>
public sealed record GlobalAttestationBundleValidator(List<IGlobalAttestationBundleRule> Rules)
{
    public List<string> Validate(GlobalAttestationBundle item)
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

    public void RequireValid(GlobalAttestationBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAttestationBundle> items)
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

/// <summary>Functions of global attestation bundle rules.</summary>
public static class GlobalAttestationBundleRules
{
    public static GlobalAttestationBundleValidator DefaultGlobalAttestationBundleValidator()
    {
        return new GlobalAttestationBundleValidator(
            new List<IGlobalAttestationBundleRule>
            {
                new GlobalAttestationBundleRequiredNameRule("global attestation bundle needs a value"),
                new GlobalAttestationBundleLabelLengthRule(120),
                new GlobalAttestationBundleCodeLengthRule(16),
                new GlobalAttestationBundlePinnedGuardRule(1000),
                new GlobalAttestationBundleCapacityRangeRule(120, 86400),
            });
    }
}
