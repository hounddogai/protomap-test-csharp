namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception segment is in its life cycle.</summary>
public enum ExceptionSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a exception segment and its current status.</summary>
public sealed class ExceptionSegment(string code, string title, string managerId, string source, string notes, int limit, string region, ExceptionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public string Source { get; set; } = source;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public string Region { get; set; } = region;
    public ExceptionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a exception segment in status requested.</summary>
    public static ExceptionSegment NewExceptionSegment(string code, string title)
    {
        return new ExceptionSegment(code, title, "", "", "", 0, "", ExceptionSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionSegmentStatus next)
    {
        switch (Status)
        {
            case ExceptionSegmentStatus.Requested:
                switch (next)
                {
                    case ExceptionSegmentStatus.Revoked:
                    case ExceptionSegmentStatus.Expired:
                        return true;
                }
                return false;
            case ExceptionSegmentStatus.Granted:
                switch (next)
                {
                    case ExceptionSegmentStatus.Expired:
                        return true;
                }
                return false;
            case ExceptionSegmentStatus.Revoked:
                return next == ExceptionSegmentStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ExceptionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public ExceptionSegment Copy()
    {
        return new ExceptionSegment(Code, Title, ManagerId, Source, Notes, Limit, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
