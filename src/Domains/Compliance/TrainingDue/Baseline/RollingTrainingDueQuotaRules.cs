namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>A rule a rolling training due quota must pass. Check returns its problems, or none.</summary>
public interface IRollingTrainingDueQuotaRule
{
    List<string> Check(RollingTrainingDueQuota item);
}

public sealed record RollingTrainingDueQuotaEndsAtRangeRule(int Min, int Max) : IRollingTrainingDueQuotaRule
{
    public List<string> Check(RollingTrainingDueQuota item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingTrainingDueQuotaRequiredNameRule(string Message) : IRollingTrainingDueQuotaRule
{
    public List<string> Check(RollingTrainingDueQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingTrainingDueQuotaCurrencyLengthRule(int MaxLength) : IRollingTrainingDueQuotaRule
{
    public List<string> Check(RollingTrainingDueQuota item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingTrainingDueQuotaFloorValueRangeRule(double Min, double Max) : IRollingTrainingDueQuotaRule
{
    public List<string> Check(RollingTrainingDueQuota item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingTrainingDueQuotaRequiredCountryRule(string Message) : IRollingTrainingDueQuotaRule
{
    public List<string> Check(RollingTrainingDueQuota item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingTrainingDueQuotaRequiredCodeRule(string Message) : IRollingTrainingDueQuotaRule
{
    public List<string> Check(RollingTrainingDueQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling training due quota.</summary>
public sealed record RollingTrainingDueQuotaValidator(List<IRollingTrainingDueQuotaRule> Rules)
{
    public List<string> Validate(RollingTrainingDueQuota item)
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

    public void RequireValid(RollingTrainingDueQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingTrainingDueQuota> items)
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

/// <summary>Functions of rolling training due quota rules.</summary>
public static class RollingTrainingDueQuotaRules
{
    public static RollingTrainingDueQuotaValidator DefaultRollingTrainingDueQuotaValidator()
    {
        return new RollingTrainingDueQuotaValidator(
            new List<IRollingTrainingDueQuotaRule>
            {
                new RollingTrainingDueQuotaEndsAtRangeRule(80, 1000),
                new RollingTrainingDueQuotaRequiredNameRule("rolling training due quota needs a value"),
                new RollingTrainingDueQuotaCurrencyLengthRule(32),
                new RollingTrainingDueQuotaFloorValueRangeRule(0.9, 2947.8),
                new RollingTrainingDueQuotaRequiredCountryRule("rolling training due quota needs a value"),
                new RollingTrainingDueQuotaRequiredCodeRule("rolling training due quota needs this field"),
            });
    }
}
