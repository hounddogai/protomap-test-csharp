namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position digest is in its life cycle.</summary>
public enum PositionDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A position digest as the position team keeps it.</summary>
public sealed class PositionDigest(string code, string name, string employeeId, bool archived, string locale, bool visible, bool approved, double score, PositionDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public bool Archived { get; set; } = archived;
    public string Locale { get; set; } = locale;
    public bool Visible { get; set; } = visible;
    public bool Approved { get; set; } = approved;
    public double Score { get; set; } = score;
    public PositionDigestStatus Status { get; set; } = status;

    /// <summary>Starts a position digest in status new.</summary>
    public static PositionDigest NewPositionDigest(string code, string name)
    {
        return new PositionDigest(code, name, "", false, "", false, false, 0.0, PositionDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionDigestStatus next)
    {
        switch (Status)
        {
            case PositionDigestStatus.New:
                switch (next)
                {
                    case PositionDigestStatus.Queued:
                        return true;
                }
                return false;
            case PositionDigestStatus.Queued:
                return next == PositionDigestStatus.Running;
            case PositionDigestStatus.Running:
                return next == PositionDigestStatus.Failed;
            case PositionDigestStatus.Done:
                return next == PositionDigestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PositionDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PositionDigest Copy()
    {
        return new PositionDigest(Code, Name, EmployeeId, Archived, Locale, Visible, Approved, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
