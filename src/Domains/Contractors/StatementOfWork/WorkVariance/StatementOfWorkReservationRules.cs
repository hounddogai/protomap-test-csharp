namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a statement of work reservation must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkReservationRule
{
    List<string> Check(StatementOfWorkReservation item);
}

public sealed record StatementOfWorkReservationJobCodeLengthRule(int MaxLength) : IStatementOfWorkReservationRule
{
    public List<string> Check(StatementOfWorkReservation item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkReservationCodeLengthRule(int MaxLength) : IStatementOfWorkReservationRule
{
    public List<string> Check(StatementOfWorkReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkReservationAllowedJobCodeRule(List<string> Allowed) : IStatementOfWorkReservationRule
{
    public List<string> Check(StatementOfWorkReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkReservationThresholdRangeRule(double Min, double Max) : IStatementOfWorkReservationRule
{
    public List<string> Check(StatementOfWorkReservation item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work reservation.</summary>
public sealed record StatementOfWorkReservationValidator(List<IStatementOfWorkReservationRule> Rules)
{
    public List<string> Validate(StatementOfWorkReservation item)
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

    public void RequireValid(StatementOfWorkReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkReservation> items)
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

/// <summary>Functions of statement of work reservation rules.</summary>
public static class StatementOfWorkReservationRules
{
    public static StatementOfWorkReservationValidator DefaultStatementOfWorkReservationValidator()
    {
        return new StatementOfWorkReservationValidator(
            new List<IStatementOfWorkReservationRule>
            {
                new StatementOfWorkReservationJobCodeLengthRule(1000),
                new StatementOfWorkReservationCodeLengthRule(255),
                new StatementOfWorkReservationAllowedJobCodeRule(new List<string> { "lenient" }),
                new StatementOfWorkReservationThresholdRangeRule(8.8, 3019.0),
            });
    }
}
