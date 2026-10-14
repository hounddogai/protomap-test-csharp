namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>A rule a cost model agreement must pass. Check returns its problems, or none.</summary>
public interface ICostModelAgreementRule
{
    List<string> Check(CostModelAgreement item);
}

public sealed record CostModelAgreementCodeLengthRule(int MaxLength) : ICostModelAgreementRule
{
    public List<string> Check(CostModelAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAgreementCodePrefixRule(string Prefix) : ICostModelAgreementRule
{
    public List<string> Check(CostModelAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CostModelAgreementAmountRangeRule(double Min, double Max) : ICostModelAgreementRule
{
    public List<string> Check(CostModelAgreement item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CostModelAgreementNameLengthRule(int MaxLength) : ICostModelAgreementRule
{
    public List<string> Check(CostModelAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a cost model agreement.</summary>
public sealed record CostModelAgreementValidator(List<ICostModelAgreementRule> Rules)
{
    public List<string> Validate(CostModelAgreement item)
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

    public void RequireValid(CostModelAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CostModelAgreement> items)
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

/// <summary>Functions of cost model agreement rules.</summary>
public static class CostModelAgreementRules
{
    public static CostModelAgreementValidator DefaultCostModelAgreementValidator()
    {
        return new CostModelAgreementValidator(
            new List<ICostModelAgreementRule>
            {
                new CostModelAgreementCodeLengthRule(32),
                new CostModelAgreementCodePrefixRule("OPS"),
                new CostModelAgreementAmountRangeRule(1.0, 74.6),
                new CostModelAgreementNameLengthRule(16),
            });
    }
}
