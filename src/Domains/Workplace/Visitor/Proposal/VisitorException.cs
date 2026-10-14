namespace Acme.Workforce.Workplace.Visitor.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor exception is in its life cycle.</summary>
public enum VisitorExceptionStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One visitor exception, with what the planners need to act on it.</summary>
public sealed class VisitorException(string code, string title, double cost, double minimum, string jobCode, int startsAt, int endsAt, VisitorExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public string JobCode { get; set; } = jobCode;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public VisitorExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a visitor exception in status created.</summary>
    public static VisitorException NewVisitorException(string code, string title)
    {
        return new VisitorException(code, title, 0.0, 0.0, "", 0, 0, VisitorExceptionStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorExceptionStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorExceptionStatus next)
    {
        switch (Status)
        {
            case VisitorExceptionStatus.Created:
                switch (next)
                {
                    case VisitorExceptionStatus.Published:
                        return true;
                }
                return false;
            case VisitorExceptionStatus.Validated:
                switch (next)
                {
                    case VisitorExceptionStatus.Published:
                        return true;
                }
                return false;
            case VisitorExceptionStatus.Published:
                return next == VisitorExceptionStatus.Retired;
        }
        return false;
    }

    public void MoveTo(VisitorExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"visitor exception"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)StartsAt + (double)EndsAt, 2);
    }

    public VisitorException Copy()
    {
        return new VisitorException(Code, Title, Cost, Minimum, JobCode, StartsAt, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
