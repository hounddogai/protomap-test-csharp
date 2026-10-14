namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy leave request template is in its life cycle.</summary>
public enum LegacyLeaveRequestTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One legacy leave request template, with what the planners need to act on it.</summary>
public sealed class LegacyLeaveRequestTemplate(string code, string label, string summary, int endsAt, int priority, int revision, double margin, LegacyLeaveRequestTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public int EndsAt { get; set; } = endsAt;
    public int Priority { get; set; } = priority;
    public int Revision { get; set; } = revision;
    public double Margin { get; set; } = margin;
    public LegacyLeaveRequestTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a legacy leave request template in status open.</summary>
    public static LegacyLeaveRequestTemplate NewLegacyLeaveRequestTemplate(string code, string label)
    {
        return new LegacyLeaveRequestTemplate(code, label, "", 0, 0, 0, 0.0, LegacyLeaveRequestTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyLeaveRequestTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyLeaveRequestTemplateStatus next)
    {
        switch (Status)
        {
            case LegacyLeaveRequestTemplateStatus.Open:
                return next == LegacyLeaveRequestTemplateStatus.Resolved || next == LegacyLeaveRequestTemplateStatus.Reopened;
            case LegacyLeaveRequestTemplateStatus.InReview:
                switch (next)
                {
                    case LegacyLeaveRequestTemplateStatus.Archived:
                        return true;
                }
                return false;
            case LegacyLeaveRequestTemplateStatus.Resolved:
                switch (next)
                {
                    case LegacyLeaveRequestTemplateStatus.Reopened:
                    case LegacyLeaveRequestTemplateStatus.Archived:
                        return true;
                }
                return false;
            case LegacyLeaveRequestTemplateStatus.Reopened:
                return next == LegacyLeaveRequestTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyLeaveRequestTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy leave request template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Revision - (double)Priority, 2);
    }

    public LegacyLeaveRequestTemplate Copy()
    {
        return new LegacyLeaveRequestTemplate(Code, Label, Summary, EndsAt, Priority, Revision, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
