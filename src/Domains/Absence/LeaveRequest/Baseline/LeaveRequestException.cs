namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request exception is in its life cycle.</summary>
public enum LeaveRequestExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A leave request exception. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestException(string code, string label, string summary, Dictionary<string, string> attributes, double maximum, LeaveRequestExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Maximum { get; set; } = maximum;
    public LeaveRequestExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a leave request exception in status draft.</summary>
    public static LeaveRequestException NewLeaveRequestException(string code, string label)
    {
        return new LeaveRequestException(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            0.0,
            LeaveRequestExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestExceptionStatus next)
    {
        switch (Status)
        {
            case LeaveRequestExceptionStatus.Draft:
                switch (next)
                {
                    case LeaveRequestExceptionStatus.Active:
                    case LeaveRequestExceptionStatus.Closed:
                        return true;
                }
                return false;
            case LeaveRequestExceptionStatus.Pending:
                switch (next)
                {
                    case LeaveRequestExceptionStatus.Active:
                    case LeaveRequestExceptionStatus.Suspended:
                        return true;
                }
                return false;
            case LeaveRequestExceptionStatus.Active:
                return next == LeaveRequestExceptionStatus.Suspended || next == LeaveRequestExceptionStatus.Closed;
            case LeaveRequestExceptionStatus.Suspended:
                return next == LeaveRequestExceptionStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LeaveRequestExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public LeaveRequestException Copy()
    {
        return new LeaveRequestException(Code, Label, Summary, Attributes, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
