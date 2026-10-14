namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>A rule a retro pay reservation must pass. Check returns its problems, or none.</summary>
public interface IRetroPayReservationRule
{
    List<string> Check(RetroPayReservation item);
}

public sealed record RetroPayReservationBadgeLengthRule(int MaxLength) : IRetroPayReservationRule
{
    public List<string> Check(RetroPayReservation item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayReservationCodeLengthRule(int MaxLength) : IRetroPayReservationRule
{
    public List<string> Check(RetroPayReservation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayReservationRequiredLabelRule(string Message) : IRetroPayReservationRule
{
    public List<string> Check(RetroPayReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay reservation.</summary>
public sealed record RetroPayReservationValidator(List<IRetroPayReservationRule> Rules)
{
    public List<string> Validate(RetroPayReservation item)
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

    public void RequireValid(RetroPayReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayReservation> items)
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

/// <summary>Functions of retro pay reservation rules.</summary>
public static class RetroPayReservationRules
{
    public static RetroPayReservationValidator DefaultRetroPayReservationValidator()
    {
        return new RetroPayReservationValidator(
            new List<IRetroPayReservationRule>
            {
                new RetroPayReservationBadgeLengthRule(86400),
                new RetroPayReservationCodeLengthRule(80),
                new RetroPayReservationRequiredLabelRule("retro pay reservation needs an entry"),
            });
    }
}
