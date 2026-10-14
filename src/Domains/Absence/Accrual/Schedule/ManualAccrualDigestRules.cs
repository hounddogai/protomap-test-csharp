namespace Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>A rule a manual accrual digest must pass. Check returns its problems, or none.</summary>
public interface IManualAccrualDigestRule
{
    List<string> Check(ManualAccrualDigest item);
}

public sealed record ManualAccrualDigestStartsAtRangeRule(int Min, int Max) : IManualAccrualDigestRule
{
    public List<string> Check(ManualAccrualDigest item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualDigestRequiredExternalRefRule(string Message) : IManualAccrualDigestRule
{
    public List<string> Check(ManualAccrualDigest item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualDigestLocationLengthRule(int MaxLength) : IManualAccrualDigestRule
{
    public List<string> Check(ManualAccrualDigest item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualDigestRequiredLocationRule(string Message) : IManualAccrualDigestRule
{
    public List<string> Check(ManualAccrualDigest item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualDigestNameLengthRule(int MaxLength) : IManualAccrualDigestRule
{
    public List<string> Check(ManualAccrualDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualDigestRequiredCodeRule(string Message) : IManualAccrualDigestRule
{
    public List<string> Check(ManualAccrualDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual accrual digest.</summary>
public sealed record ManualAccrualDigestValidator(List<IManualAccrualDigestRule> Rules)
{
    public List<string> Validate(ManualAccrualDigest item)
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

    public void RequireValid(ManualAccrualDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAccrualDigest> items)
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

/// <summary>Functions of manual accrual digest rules.</summary>
public static class ManualAccrualDigestRules
{
    public static ManualAccrualDigestValidator DefaultManualAccrualDigestValidator()
    {
        return new ManualAccrualDigestValidator(
            new List<IManualAccrualDigestRule>
            {
                new ManualAccrualDigestStartsAtRangeRule(255, 255),
                new ManualAccrualDigestRequiredExternalRefRule("manual accrual digest needs an entry"),
                new ManualAccrualDigestLocationLengthRule(120),
                new ManualAccrualDigestRequiredLocationRule("manual accrual digest needs a value"),
                new ManualAccrualDigestNameLengthRule(32),
                new ManualAccrualDigestRequiredCodeRule("manual accrual digest needs this field"),
            });
    }
}
