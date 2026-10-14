namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped retro pay reservation must pass. Check returns its problems, or none.</summary>
public interface ICappedRetroPayReservationRule
{
    List<string> Check(CappedRetroPayReservation item);
}

public sealed record CappedRetroPayReservationAllowedCodeRule(List<string> Allowed) : ICappedRetroPayReservationRule
{
    public List<string> Check(CappedRetroPayReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record CappedRetroPayReservationNotesLengthRule(int MaxLength) : ICappedRetroPayReservationRule
{
    public List<string> Check(CappedRetroPayReservation item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedRetroPayReservationRequiredNotesRule(string Message) : ICappedRetroPayReservationRule
{
    public List<string> Check(CappedRetroPayReservation item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedRetroPayReservationRequiredCodeRule(string Message) : ICappedRetroPayReservationRule
{
    public List<string> Check(CappedRetroPayReservation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped retro pay reservation.</summary>
public sealed record CappedRetroPayReservationValidator(List<ICappedRetroPayReservationRule> Rules)
{
    public List<string> Validate(CappedRetroPayReservation item)
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

    public void RequireValid(CappedRetroPayReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedRetroPayReservation> items)
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

/// <summary>Functions of capped retro pay reservation rules.</summary>
public static class CappedRetroPayReservationRules
{
    public static CappedRetroPayReservationValidator DefaultCappedRetroPayReservationValidator()
    {
        return new CappedRetroPayReservationValidator(
            new List<ICappedRetroPayReservationRule>
            {
                new CappedRetroPayReservationAllowedCodeRule(new List<string> { "regional", "legacy", "manual" }),
                new CappedRetroPayReservationNotesLengthRule(80),
                new CappedRetroPayReservationRequiredNotesRule("capped retro pay reservation needs this field"),
                new CappedRetroPayReservationRequiredCodeRule("capped retro pay reservation needs this field"),
            });
    }
}
