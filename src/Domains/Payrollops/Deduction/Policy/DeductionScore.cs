namespace Acme.Workforce.Payrollops.Deduction.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction score is in its life cycle.</summary>
public enum DeductionScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a deduction score and its current status.</summary>
public sealed class DeductionScore(string code, string title, string location, string unit, string ownerId, double amount, DeductionScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public string Unit { get; set; } = unit;
    public string OwnerId { get; set; } = ownerId;
    public double Amount { get; set; } = amount;
    public DeductionScoreStatus Status { get; set; } = status;

    /// <summary>Starts a deduction score in status created.</summary>
    public static DeductionScore NewDeductionScore(string code, string title)
    {
        return new DeductionScore(code, title, "", "", "", 0.0, DeductionScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionScoreStatus next)
    {
        switch (Status)
        {
            case DeductionScoreStatus.Created:
                return next == DeductionScoreStatus.Published;
            case DeductionScoreStatus.Validated:
                switch (next)
                {
                    case DeductionScoreStatus.Published:
                        return true;
                }
                return false;
            case DeductionScoreStatus.Published:
                switch (next)
                {
                    case DeductionScoreStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public DeductionScore Copy()
    {
        return new DeductionScore(Code, Title, Location, Unit, OwnerId, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
