namespace Acme.Workforce.Compliance.Exception.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary exception request is in its life cycle.</summary>
public enum PrimaryExceptionRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One primary exception request, with what the planners need to act on it.</summary>
public sealed class PrimaryExceptionRequest(string code, string title, int startsAt, string salaryBand, bool pinned, PrimaryExceptionRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Pinned { get; set; } = pinned;
    public PrimaryExceptionRequestStatus Status { get; set; } = status;

    /// <summary>Starts a primary exception request in status draft.</summary>
    public static PrimaryExceptionRequest NewPrimaryExceptionRequest(string code, string title)
    {
        return new PrimaryExceptionRequest(code, title, 0, "", false, PrimaryExceptionRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryExceptionRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryExceptionRequestStatus next)
    {
        switch (Status)
        {
            case PrimaryExceptionRequestStatus.Draft:
                return next == PrimaryExceptionRequestStatus.Pending || next == PrimaryExceptionRequestStatus.Active;
            case PrimaryExceptionRequestStatus.Pending:
                return next == PrimaryExceptionRequestStatus.Active || next == PrimaryExceptionRequestStatus.Suspended;
            case PrimaryExceptionRequestStatus.Active:
                return next == PrimaryExceptionRequestStatus.Suspended;
            case PrimaryExceptionRequestStatus.Suspended:
                return next == PrimaryExceptionRequestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PrimaryExceptionRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary exception request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public PrimaryExceptionRequest Copy()
    {
        return new PrimaryExceptionRequest(Code, Title, StartsAt, SalaryBand, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
