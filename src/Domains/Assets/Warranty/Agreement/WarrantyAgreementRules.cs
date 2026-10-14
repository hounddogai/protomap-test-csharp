namespace Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>A rule a warranty agreement must pass. Check returns its problems, or none.</summary>
public interface IWarrantyAgreementRule
{
    List<string> Check(WarrantyAgreement item);
}

public sealed record WarrantyAgreementEmployeeIdLengthRule(int MaxLength) : IWarrantyAgreementRule
{
    public List<string> Check(WarrantyAgreement item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAgreementCodeLengthRule(int MaxLength) : IWarrantyAgreementRule
{
    public List<string> Check(WarrantyAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAgreementCostRangeRule(double Min, double Max) : IWarrantyAgreementRule
{
    public List<string> Check(WarrantyAgreement item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyAgreementRequiredCodeRule(string Message) : IWarrantyAgreementRule
{
    public List<string> Check(WarrantyAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty agreement.</summary>
public sealed record WarrantyAgreementValidator(List<IWarrantyAgreementRule> Rules)
{
    public List<string> Validate(WarrantyAgreement item)
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

    public void RequireValid(WarrantyAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyAgreement> items)
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

/// <summary>Functions of warranty agreement rules.</summary>
public static class WarrantyAgreementRules
{
    public static WarrantyAgreementValidator DefaultWarrantyAgreementValidator()
    {
        return new WarrantyAgreementValidator(
            new List<IWarrantyAgreementRule>
            {
                new WarrantyAgreementEmployeeIdLengthRule(64),
                new WarrantyAgreementCodeLengthRule(16),
                new WarrantyAgreementCostRangeRule(4.6, 2273.1),
                new WarrantyAgreementRequiredCodeRule("warranty agreement needs an entry"),
            });
    }
}
