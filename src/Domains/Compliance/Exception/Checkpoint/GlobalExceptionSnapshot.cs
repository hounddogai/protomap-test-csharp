namespace Acme.Workforce.Compliance.Exception.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global exception snapshot is in its life cycle.</summary>
public enum GlobalExceptionSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One global exception snapshot, with what the planners need to act on it.</summary>
public sealed class GlobalExceptionSnapshot(string code, string title, double rating, string jobCode, string name, GlobalExceptionSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string JobCode { get; set; } = jobCode;
    public string Name { get; set; } = name;
    public GlobalExceptionSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a global exception snapshot in status created.</summary>
    public static GlobalExceptionSnapshot NewGlobalExceptionSnapshot(string code, string title)
    {
        return new GlobalExceptionSnapshot(code, title, 0.0, "", "", GlobalExceptionSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalExceptionSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalExceptionSnapshotStatus next)
    {
        switch (Status)
        {
            case GlobalExceptionSnapshotStatus.Created:
                return next == GlobalExceptionSnapshotStatus.Retired;
            case GlobalExceptionSnapshotStatus.Validated:
                switch (next)
                {
                    case GlobalExceptionSnapshotStatus.Retired:
                        return true;
                }
                return false;
            case GlobalExceptionSnapshotStatus.Published:
                return next == GlobalExceptionSnapshotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GlobalExceptionSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global exception snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global exception snapshot"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public GlobalExceptionSnapshot Copy()
    {
        return new GlobalExceptionSnapshot(Code, Title, Rating, JobCode, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
