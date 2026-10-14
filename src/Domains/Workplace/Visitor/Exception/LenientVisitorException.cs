namespace Acme.Workforce.Workplace.Visitor.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient visitor exception is in its life cycle.</summary>
public enum LenientVisitorExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient visitor exception as the visitor team keeps it.</summary>
public sealed class LenientVisitorException(string code, string name, double ratio, int retries, string badge, LenientVisitorExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public int Retries { get; set; } = retries;
    public string Badge { get; set; } = badge;
    public LenientVisitorExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a lenient visitor exception in status open.</summary>
    public static LenientVisitorException NewLenientVisitorException(string code, string name)
    {
        return new LenientVisitorException(code, name, 0.0, 0, "", LenientVisitorExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVisitorExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVisitorExceptionStatus next)
    {
        switch (Status)
        {
            case LenientVisitorExceptionStatus.Open:
                return next == LenientVisitorExceptionStatus.InReview;
            case LenientVisitorExceptionStatus.InReview:
                return next == LenientVisitorExceptionStatus.Reopened || next == LenientVisitorExceptionStatus.Archived;
            case LenientVisitorExceptionStatus.Resolved:
                switch (next)
                {
                    case LenientVisitorExceptionStatus.Reopened:
                    case LenientVisitorExceptionStatus.Archived:
                        return true;
                }
                return false;
            case LenientVisitorExceptionStatus.Reopened:
                switch (next)
                {
                    case LenientVisitorExceptionStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientVisitorExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient visitor exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient visitor exception"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Ratio + (double)Retries, 2);
    }

    public LenientVisitorException Copy()
    {
        return new LenientVisitorException(Code, Name, Ratio, Retries, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
