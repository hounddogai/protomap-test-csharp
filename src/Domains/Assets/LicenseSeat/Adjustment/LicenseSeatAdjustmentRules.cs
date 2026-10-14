namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat adjustment must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatAdjustmentRule
{
    List<string> Check(LicenseSeatAdjustment item);
}

public sealed record LicenseSeatAdjustmentRequiredCodeRule(string Message) : ILicenseSeatAdjustmentRule
{
    public List<string> Check(LicenseSeatAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAdjustmentAllowedLabelRule(List<string> Allowed) : ILicenseSeatAdjustmentRule
{
    public List<string> Check(LicenseSeatAdjustment item)
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

public sealed record LicenseSeatAdjustmentRequiredLabelRule(string Message) : ILicenseSeatAdjustmentRule
{
    public List<string> Check(LicenseSeatAdjustment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAdjustmentCodePrefixRule(string Prefix) : ILicenseSeatAdjustmentRule
{
    public List<string> Check(LicenseSeatAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAdjustmentNameLengthRule(int MaxLength) : ILicenseSeatAdjustmentRule
{
    public List<string> Check(LicenseSeatAdjustment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAdjustmentRequiredNameRule(string Message) : ILicenseSeatAdjustmentRule
{
    public List<string> Check(LicenseSeatAdjustment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat adjustment.</summary>
public sealed record LicenseSeatAdjustmentValidator(List<ILicenseSeatAdjustmentRule> Rules)
{
    public List<string> Validate(LicenseSeatAdjustment item)
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

    public void RequireValid(LicenseSeatAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatAdjustment> items)
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

/// <summary>Functions of license seat adjustment rules.</summary>
public static class LicenseSeatAdjustmentRules
{
    public static LicenseSeatAdjustmentValidator DefaultLicenseSeatAdjustmentValidator()
    {
        return new LicenseSeatAdjustmentValidator(
            new List<ILicenseSeatAdjustmentRule>
            {
                new LicenseSeatAdjustmentRequiredCodeRule("license seat adjustment needs this field"),
                new LicenseSeatAdjustmentAllowedLabelRule(new List<string>()),
                new LicenseSeatAdjustmentRequiredLabelRule("license seat adjustment needs a value"),
                new LicenseSeatAdjustmentCodePrefixRule("Q"),
                new LicenseSeatAdjustmentNameLengthRule(255),
                new LicenseSeatAdjustmentRequiredNameRule("license seat adjustment needs this field"),
            });
    }
}
