namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient statement of work checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILenientStatementOfWorkCheckpointRule
{
    List<string> Check(LenientStatementOfWorkCheckpoint item);
}

public sealed record LenientStatementOfWorkCheckpointCodeLengthRule(int MaxLength) : ILenientStatementOfWorkCheckpointRule
{
    public List<string> Check(LenientStatementOfWorkCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkCheckpointRequiredContactEmailRule(string Message) : ILenientStatementOfWorkCheckpointRule
{
    public List<string> Check(LenientStatementOfWorkCheckpoint item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkCheckpointAllowedContactEmailRule(List<string> Allowed) : ILenientStatementOfWorkCheckpointRule
{
    public List<string> Check(LenientStatementOfWorkCheckpoint item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LenientStatementOfWorkCheckpointNameLengthRule(int MaxLength) : ILenientStatementOfWorkCheckpointRule
{
    public List<string> Check(LenientStatementOfWorkCheckpoint item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient statement of work checkpoint.</summary>
public sealed record LenientStatementOfWorkCheckpointValidator(List<ILenientStatementOfWorkCheckpointRule> Rules)
{
    public List<string> Validate(LenientStatementOfWorkCheckpoint item)
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

    public void RequireValid(LenientStatementOfWorkCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientStatementOfWorkCheckpoint> items)
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

/// <summary>Functions of lenient statement of work checkpoint rules.</summary>
public static class LenientStatementOfWorkCheckpointRules
{
    public static LenientStatementOfWorkCheckpointValidator DefaultLenientStatementOfWorkCheckpointValidator()
    {
        return new LenientStatementOfWorkCheckpointValidator(
            new List<ILenientStatementOfWorkCheckpointRule>
            {
                new LenientStatementOfWorkCheckpointCodeLengthRule(16),
                new LenientStatementOfWorkCheckpointRequiredContactEmailRule(
                    "lenient statement of work checkpoint needs a value"),
                new LenientStatementOfWorkCheckpointAllowedContactEmailRule(
                    new List<string> { "pending", "weighted", "legacy" }),
                new LenientStatementOfWorkCheckpointNameLengthRule(86400),
            });
    }
}
