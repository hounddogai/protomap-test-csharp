namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception record is in its life cycle.</summary>
public enum ExceptionRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A exception record as the exception team keeps it.</summary>
public sealed class ExceptionRecord(string code, string title, string name, double score, double rate, bool approved, double cost, List<string> tags, ExceptionRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public double Rate { get; set; } = rate;
    public bool Approved { get; set; } = approved;
    public double Cost { get; set; } = cost;
    public List<string> Tags { get; set; } = tags;
    public ExceptionRecordStatus Status { get; set; } = status;

    /// <summary>Starts a exception record in status open.</summary>
    public static ExceptionRecord NewExceptionRecord(string code, string title)
    {
        return new ExceptionRecord(
            code,
            title,
            "",
            0.0,
            0.0,
            false,
            0.0,
            new List<string>(),
            ExceptionRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionRecordStatus next)
    {
        switch (Status)
        {
            case ExceptionRecordStatus.Open:
                return next == ExceptionRecordStatus.Archived;
            case ExceptionRecordStatus.InReview:
                return next == ExceptionRecordStatus.Archived;
            case ExceptionRecordStatus.Resolved:
                return next == ExceptionRecordStatus.Reopened || next == ExceptionRecordStatus.Archived;
            case ExceptionRecordStatus.Reopened:
                return next == ExceptionRecordStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ExceptionRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedVolume()
    {
        return Math.Round(Rate - Cost, 2);
    }

    public ExceptionRecord Copy()
    {
        return new ExceptionRecord(Code, Title, Name, Score, Rate, Approved, Cost, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
