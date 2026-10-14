namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment request is in its life cycle.</summary>
public enum AssignmentRequestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One assignment request, with what the planners need to act on it.</summary>
public sealed class AssignmentRequest(string code, string label, double score, bool archived, string timezone, AssignmentRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public string Timezone { get; set; } = timezone;
    public AssignmentRequestStatus Status { get; set; } = status;

    /// <summary>Starts a assignment request in status requested.</summary>
    public static AssignmentRequest NewAssignmentRequest(string code, string label)
    {
        return new AssignmentRequest(code, label, 0.0, false, "", AssignmentRequestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentRequestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentRequestStatus next)
    {
        switch (Status)
        {
            case AssignmentRequestStatus.Requested:
                switch (next)
                {
                    case AssignmentRequestStatus.Revoked:
                        return true;
                }
                return false;
            case AssignmentRequestStatus.Granted:
                return next == AssignmentRequestStatus.Revoked || next == AssignmentRequestStatus.Expired;
            case AssignmentRequestStatus.Revoked:
                return next == AssignmentRequestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(AssignmentRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public AssignmentRequest Copy()
    {
        return new AssignmentRequest(Code, Label, Score, Archived, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
