namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception manifest is in its life cycle.</summary>
public enum ExceptionManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a exception manifest and its current status.</summary>
public sealed class ExceptionManifest(string code, string title, string employeeId, int batchSize, string locale, string grade, double fte, ExceptionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public int BatchSize { get; set; } = batchSize;
    public string Locale { get; set; } = locale;
    public string Grade { get; set; } = grade;
    public double Fte { get; set; } = fte;
    public ExceptionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a exception manifest in status new.</summary>
    public static ExceptionManifest NewExceptionManifest(string code, string title)
    {
        return new ExceptionManifest(code, title, "", 0, "", "", 0.0, ExceptionManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionManifestStatus next)
    {
        switch (Status)
        {
            case ExceptionManifestStatus.New:
                switch (next)
                {
                    case ExceptionManifestStatus.Done:
                    case ExceptionManifestStatus.Failed:
                        return true;
                }
                return false;
            case ExceptionManifestStatus.Queued:
                return next == ExceptionManifestStatus.Done || next == ExceptionManifestStatus.Failed;
            case ExceptionManifestStatus.Running:
                return next == ExceptionManifestStatus.Done || next == ExceptionManifestStatus.Failed;
            case ExceptionManifestStatus.Done:
                return next == ExceptionManifestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ExceptionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte + (double)BatchSize, 2);
    }

    public ExceptionManifest Copy()
    {
        return new ExceptionManifest(Code, Title, EmployeeId, BatchSize, Locale, Grade, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
