namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment notice is in its life cycle.</summary>
public enum AssignmentNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a assignment notice and its current status.</summary>
public sealed class AssignmentNotice(string code, string title, string checksum, int revision, string department, int batchSize, AssignmentNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public int Revision { get; set; } = revision;
    public string Department { get; set; } = department;
    public int BatchSize { get; set; } = batchSize;
    public AssignmentNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a assignment notice in status created.</summary>
    public static AssignmentNotice NewAssignmentNotice(string code, string title)
    {
        return new AssignmentNotice(code, title, "", 0, "", 0, AssignmentNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentNoticeStatus next)
    {
        switch (Status)
        {
            case AssignmentNoticeStatus.Created:
                switch (next)
                {
                    case AssignmentNoticeStatus.Published:
                        return true;
                }
                return false;
            case AssignmentNoticeStatus.Validated:
                switch (next)
                {
                    case AssignmentNoticeStatus.Published:
                        return true;
                }
                return false;
            case AssignmentNoticeStatus.Published:
                return next == AssignmentNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AssignmentNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"assignment notice"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)BatchSize + (double)Revision, 2);
    }

    public AssignmentNotice Copy()
    {
        return new AssignmentNotice(Code, Title, Checksum, Revision, Department, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
