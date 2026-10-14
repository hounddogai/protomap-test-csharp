namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor claim is in its life cycle.</summary>
public enum VisitorClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One visitor claim, with what the planners need to act on it.</summary>
public sealed class VisitorClaim(string code, string label, double rate, string unit, double discount, string jobCode, int createdAt, VisitorClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Unit { get; set; } = unit;
    public double Discount { get; set; } = discount;
    public string JobCode { get; set; } = jobCode;
    public int CreatedAt { get; set; } = createdAt;
    public VisitorClaimStatus Status { get; set; } = status;

    /// <summary>Starts a visitor claim in status draft.</summary>
    public static VisitorClaim NewVisitorClaim(string code, string label)
    {
        return new VisitorClaim(code, label, 0.0, "", 0.0, "", 0, VisitorClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorClaimStatus next)
    {
        switch (Status)
        {
            case VisitorClaimStatus.Draft:
                return next == VisitorClaimStatus.Active || next == VisitorClaimStatus.Closed;
            case VisitorClaimStatus.Pending:
                return next == VisitorClaimStatus.Active;
            case VisitorClaimStatus.Active:
                return next == VisitorClaimStatus.Suspended || next == VisitorClaimStatus.Closed;
            case VisitorClaimStatus.Suspended:
                return next == VisitorClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(VisitorClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"visitor claim"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Rate + Discount, 2);
    }

    public VisitorClaim Copy()
    {
        return new VisitorClaim(Code, Label, Rate, Unit, Discount, JobCode, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
