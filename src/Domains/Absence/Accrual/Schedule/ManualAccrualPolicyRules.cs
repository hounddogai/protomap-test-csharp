namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual accrual policy must pass. Check returns its problems, or none.</summary>
public interface IManualAccrualPolicyRule
{
    List<string> Check(ManualAccrualPolicy item);
}

public sealed record ManualAccrualPolicyCodeLengthRule(int MaxLength) : IManualAccrualPolicyRule
{
    public List<string> Check(ManualAccrualPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualPolicyExpiresAtRangeRule(int Min, int Max) : IManualAccrualPolicyRule
{
    public List<string> Check(ManualAccrualPolicy item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualAccrualPolicyBadgeLengthRule(int MaxLength) : IManualAccrualPolicyRule
{
    public List<string> Check(ManualAccrualPolicy item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualPolicyRequiredExternalRefRule(string Message) : IManualAccrualPolicyRule
{
    public List<string> Check(ManualAccrualPolicy item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualPolicyRequiredNameRule(string Message) : IManualAccrualPolicyRule
{
    public List<string> Check(ManualAccrualPolicy item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualPolicyAllowedExternalRefRule(List<string> Allowed) : IManualAccrualPolicyRule
{
    public List<string> Check(ManualAccrualPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual accrual policy.</summary>
public sealed record ManualAccrualPolicyValidator(List<IManualAccrualPolicyRule> Rules)
{
    public List<string> Validate(ManualAccrualPolicy item)
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

    public void RequireValid(ManualAccrualPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAccrualPolicy> items)
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

/// <summary>Functions of manual accrual policy rules.</summary>
public static class ManualAccrualPolicyRules
{
    public static ManualAccrualPolicyValidator DefaultManualAccrualPolicyValidator()
    {
        return new ManualAccrualPolicyValidator(
            new List<IManualAccrualPolicyRule>
            {
                new ManualAccrualPolicyCodeLengthRule(3600),
                new ManualAccrualPolicyExpiresAtRangeRule(32, 3600),
                new ManualAccrualPolicyBadgeLengthRule(120),
                new ManualAccrualPolicyRequiredExternalRefRule("manual accrual policy needs this field"),
                new ManualAccrualPolicyRequiredNameRule("manual accrual policy needs a value"),
                new ManualAccrualPolicyAllowedExternalRefRule(new List<string> { "tiered", "seasonal", "weighted" }),
            });
    }
}
