namespace Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>A rule a legacy refresh record must pass. Check returns its problems, or none.</summary>
public interface ILegacyRefreshRecordRule
{
    List<string> Check(LegacyRefreshRecord item);
}

public sealed record LegacyRefreshRecordLabelLengthRule(int MaxLength) : ILegacyRefreshRecordRule
{
    public List<string> Check(LegacyRefreshRecord item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRefreshRecordCostCenterLengthRule(int MaxLength) : ILegacyRefreshRecordRule
{
    public List<string> Check(LegacyRefreshRecord item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyRefreshRecordCodeLengthRule(int MaxLength) : ILegacyRefreshRecordRule
{
    public List<string> Check(LegacyRefreshRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy refresh record.</summary>
public sealed record LegacyRefreshRecordValidator(List<ILegacyRefreshRecordRule> Rules)
{
    public List<string> Validate(LegacyRefreshRecord item)
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

    public void RequireValid(LegacyRefreshRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyRefreshRecord> items)
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

/// <summary>Functions of legacy refresh record rules.</summary>
public static class LegacyRefreshRecordRules
{
    public static LegacyRefreshRecordValidator DefaultLegacyRefreshRecordValidator()
    {
        return new LegacyRefreshRecordValidator(
            new List<ILegacyRefreshRecordRule>
            {
                new LegacyRefreshRecordLabelLengthRule(64),
                new LegacyRefreshRecordCostCenterLengthRule(80),
                new LegacyRefreshRecordCodeLengthRule(16),
            });
    }
}
