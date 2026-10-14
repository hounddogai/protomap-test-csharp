namespace Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>A rule a visitor agreement must pass. Check returns its problems, or none.</summary>
public interface IVisitorAgreementRule
{
    List<string> Check(VisitorAgreement item);
}

public sealed record VisitorAgreementLocationLengthRule(int MaxLength) : IVisitorAgreementRule
{
    public List<string> Check(VisitorAgreement item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAgreementNameLengthRule(int MaxLength) : IVisitorAgreementRule
{
    public List<string> Check(VisitorAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAgreementCodeLengthRule(int MaxLength) : IVisitorAgreementRule
{
    public List<string> Check(VisitorAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorAgreementRequiredLocationRule(string Message) : IVisitorAgreementRule
{
    public List<string> Check(VisitorAgreement item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor agreement.</summary>
public sealed record VisitorAgreementValidator(List<IVisitorAgreementRule> Rules)
{
    public List<string> Validate(VisitorAgreement item)
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

    public void RequireValid(VisitorAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorAgreement> items)
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

/// <summary>Functions of visitor agreement rules.</summary>
public static class VisitorAgreementRules
{
    public static VisitorAgreementValidator DefaultVisitorAgreementValidator()
    {
        return new VisitorAgreementValidator(
            new List<IVisitorAgreementRule>
            {
                new VisitorAgreementLocationLengthRule(120),
                new VisitorAgreementNameLengthRule(1000),
                new VisitorAgreementCodeLengthRule(16),
                new VisitorAgreementRequiredLocationRule("visitor agreement needs an entry"),
            });
    }
}
