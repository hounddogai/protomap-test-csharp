namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>A rule a adaptive leave request ledger must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveLeaveRequestLedgerRule
{
    List<string> Check(AdaptiveLeaveRequestLedger item);
}

public sealed record AdaptiveLeaveRequestLedgerQuantityRangeRule(int Min, int Max) : IAdaptiveLeaveRequestLedgerRule
{
    public List<string> Check(AdaptiveLeaveRequestLedger item)
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

public sealed record AdaptiveLeaveRequestLedgerNameLengthRule(int MaxLength) : IAdaptiveLeaveRequestLedgerRule
{
    public List<string> Check(AdaptiveLeaveRequestLedger item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLeaveRequestLedgerMaximumRangeRule(double Min, double Max) : IAdaptiveLeaveRequestLedgerRule
{
    public List<string> Check(AdaptiveLeaveRequestLedger item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLeaveRequestLedgerRateRangeRule(double Min, double Max) : IAdaptiveLeaveRequestLedgerRule
{
    public List<string> Check(AdaptiveLeaveRequestLedger item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveLeaveRequestLedgerCodeLengthRule(int MaxLength) : IAdaptiveLeaveRequestLedgerRule
{
    public List<string> Check(AdaptiveLeaveRequestLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLeaveRequestLedgerDepartmentLengthRule(int MaxLength) : IAdaptiveLeaveRequestLedgerRule
{
    public List<string> Check(AdaptiveLeaveRequestLedger item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive leave request ledger.</summary>
public sealed record AdaptiveLeaveRequestLedgerValidator(List<IAdaptiveLeaveRequestLedgerRule> Rules)
{
    public List<string> Validate(AdaptiveLeaveRequestLedger item)
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

    public void RequireValid(AdaptiveLeaveRequestLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveLeaveRequestLedger> items)
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

/// <summary>Functions of adaptive leave request ledger rules.</summary>
public static class AdaptiveLeaveRequestLedgerRules
{
    public static AdaptiveLeaveRequestLedgerValidator DefaultAdaptiveLeaveRequestLedgerValidator()
    {
        return new AdaptiveLeaveRequestLedgerValidator(
            new List<IAdaptiveLeaveRequestLedgerRule>
            {
                new AdaptiveLeaveRequestLedgerQuantityRangeRule(64, 255),
                new AdaptiveLeaveRequestLedgerNameLengthRule(80),
                new AdaptiveLeaveRequestLedgerMaximumRangeRule(0.7, 1730.5),
                new AdaptiveLeaveRequestLedgerRateRangeRule(2.9, 4090.1),
                new AdaptiveLeaveRequestLedgerCodeLengthRule(80),
                new AdaptiveLeaveRequestLedgerDepartmentLengthRule(86400),
            });
    }
}
