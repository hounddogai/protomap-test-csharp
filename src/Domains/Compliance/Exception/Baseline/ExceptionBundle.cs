namespace Acme.Workforce.Compliance.Exception.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception bundle is in its life cycle.</summary>
public enum ExceptionBundleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A exception bundle. Its code is unique within a tenant.</summary>
public sealed class ExceptionBundle(string code, string title, int revision, double rate, string managerId, int attempts, ExceptionBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public double Rate { get; set; } = rate;
    public string ManagerId { get; set; } = managerId;
    public int Attempts { get; set; } = attempts;
    public ExceptionBundleStatus Status { get; set; } = status;

    /// <summary>Starts a exception bundle in status created.</summary>
    public static ExceptionBundle NewExceptionBundle(string code, string title)
    {
        return new ExceptionBundle(code, title, 0, 0.0, "", 0, ExceptionBundleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionBundleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionBundleStatus next)
    {
        switch (Status)
        {
            case ExceptionBundleStatus.Created:
                return next == ExceptionBundleStatus.Validated;
            case ExceptionBundleStatus.Validated:
                return next == ExceptionBundleStatus.Published;
            case ExceptionBundleStatus.Published:
                return next == ExceptionBundleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ExceptionBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Attempts - Rate, 2);
    }

    public ExceptionBundle Copy()
    {
        return new ExceptionBundle(Code, Title, Revision, Rate, ManagerId, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
