namespace Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>A rule a laptop ledger must pass. Check returns its problems, or none.</summary>
public interface ILaptopLedgerRule
{
    List<string> Check(LaptopLedger item);
}

public sealed record LaptopLedgerRevisionRangeRule(int Min, int Max) : ILaptopLedgerRule
{
    public List<string> Check(LaptopLedger item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LaptopLedgerLabelLengthRule(int MaxLength) : ILaptopLedgerRule
{
    public List<string> Check(LaptopLedger item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopLedgerRequiredCostCenterRule(string Message) : ILaptopLedgerRule
{
    public List<string> Check(LaptopLedger item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LaptopLedgerCodeLengthRule(int MaxLength) : ILaptopLedgerRule
{
    public List<string> Check(LaptopLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LaptopLedgerRequiredLabelRule(string Message) : ILaptopLedgerRule
{
    public List<string> Check(LaptopLedger item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a laptop ledger.</summary>
public sealed record LaptopLedgerValidator(List<ILaptopLedgerRule> Rules)
{
    public List<string> Validate(LaptopLedger item)
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

    public void RequireValid(LaptopLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LaptopLedger> items)
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

/// <summary>Functions of laptop ledger rules.</summary>
public static class LaptopLedgerRules
{
    public static LaptopLedgerValidator DefaultLaptopLedgerValidator()
    {
        return new LaptopLedgerValidator(
            new List<ILaptopLedgerRule>
            {
                new LaptopLedgerRevisionRangeRule(80, 120),
                new LaptopLedgerLabelLengthRule(86400),
                new LaptopLedgerRequiredCostCenterRule("laptop ledger needs this field"),
                new LaptopLedgerCodeLengthRule(255),
                new LaptopLedgerRequiredLabelRule("laptop ledger needs an entry"),
            });
    }
}
