namespace Acme.Workforce.Compliance.Exception.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient exception variance is in its life cycle.</summary>
public enum LenientExceptionVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient exception variance. Its code is unique within a tenant.</summary>
public sealed class LenientExceptionVariance(string code, string label, string team, string managerId, string grade, double ceiling, LenientExceptionVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string ManagerId { get; set; } = managerId;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public LenientExceptionVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a lenient exception variance in status draft.</summary>
    public static LenientExceptionVariance NewLenientExceptionVariance(string code, string label)
    {
        return new LenientExceptionVariance(code, label, "", "", "", 0.0, LenientExceptionVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientExceptionVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientExceptionVarianceStatus next)
    {
        switch (Status)
        {
            case LenientExceptionVarianceStatus.Draft:
                return next == LenientExceptionVarianceStatus.Closed;
            case LenientExceptionVarianceStatus.Pending:
                return next == LenientExceptionVarianceStatus.Suspended;
            case LenientExceptionVarianceStatus.Active:
                switch (next)
                {
                    case LenientExceptionVarianceStatus.Suspended:
                    case LenientExceptionVarianceStatus.Closed:
                        return true;
                }
                return false;
            case LenientExceptionVarianceStatus.Suspended:
                switch (next)
                {
                    case LenientExceptionVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientExceptionVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient exception variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient exception variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public LenientExceptionVariance Copy()
    {
        return new LenientExceptionVariance(Code, Label, Team, ManagerId, Grade, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
