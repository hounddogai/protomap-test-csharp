namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position exception is in its life cycle.</summary>
public enum PositionExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A position exception. Its code is unique within a tenant.</summary>
public sealed class PositionException(string code, string title, string team, double ceiling, string checksum, PositionExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public double Ceiling { get; set; } = ceiling;
    public string Checksum { get; set; } = checksum;
    public PositionExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a position exception in status draft.</summary>
    public static PositionException NewPositionException(string code, string title)
    {
        return new PositionException(code, title, "", 0.0, "", PositionExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionExceptionStatus next)
    {
        switch (Status)
        {
            case PositionExceptionStatus.Draft:
                return next == PositionExceptionStatus.Closed;
            case PositionExceptionStatus.Pending:
                return next == PositionExceptionStatus.Active;
            case PositionExceptionStatus.Active:
                return next == PositionExceptionStatus.Closed;
            case PositionExceptionStatus.Suspended:
                switch (next)
                {
                    case PositionExceptionStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public PositionException Copy()
    {
        return new PositionException(Code, Title, Team, Ceiling, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
