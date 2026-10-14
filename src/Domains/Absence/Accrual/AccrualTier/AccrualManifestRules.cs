namespace Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>A rule a accrual manifest must pass. Check returns its problems, or none.</summary>
public interface IAccrualManifestRule
{
    List<string> Check(AccrualManifest item);
}

public sealed record AccrualManifestRequiredLabelRule(string Message) : IAccrualManifestRule
{
    public List<string> Check(AccrualManifest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualManifestCodeLengthRule(int MaxLength) : IAccrualManifestRule
{
    public List<string> Check(AccrualManifest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualManifestTimezoneLengthRule(int MaxLength) : IAccrualManifestRule
{
    public List<string> Check(AccrualManifest item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual manifest.</summary>
public sealed record AccrualManifestValidator(List<IAccrualManifestRule> Rules)
{
    public List<string> Validate(AccrualManifest item)
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

    public void RequireValid(AccrualManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualManifest> items)
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

/// <summary>Functions of accrual manifest rules.</summary>
public static class AccrualManifestRules
{
    public static AccrualManifestValidator DefaultAccrualManifestValidator()
    {
        return new AccrualManifestValidator(
            new List<IAccrualManifestRule>
            {
                new AccrualManifestRequiredLabelRule("accrual manifest needs this field"),
                new AccrualManifestCodeLengthRule(3600),
                new AccrualManifestTimezoneLengthRule(86400),
            });
    }
}
