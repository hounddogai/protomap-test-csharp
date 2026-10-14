namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual checkpoint is in its life cycle.</summary>
public enum AccrualCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a accrual checkpoint and its current status.</summary>
public sealed class AccrualCheckpoint(string code, string title, double minimum, List<string> tags, string description, AccrualCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public List<string> Tags { get; set; } = tags;
    public string Description { get; set; } = description;
    public AccrualCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a accrual checkpoint in status requested.</summary>
    public static AccrualCheckpoint NewAccrualCheckpoint(string code, string title)
    {
        return new AccrualCheckpoint(code, title, 0.0, new List<string>(), "", AccrualCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualCheckpointStatus next)
    {
        switch (Status)
        {
            case AccrualCheckpointStatus.Requested:
                switch (next)
                {
                    case AccrualCheckpointStatus.Revoked:
                    case AccrualCheckpointStatus.Expired:
                        return true;
                }
                return false;
            case AccrualCheckpointStatus.Granted:
                return next == AccrualCheckpointStatus.Expired;
            case AccrualCheckpointStatus.Revoked:
                switch (next)
                {
                    case AccrualCheckpointStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual checkpoint {Code} cannot move from {Status} to {next}");
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

    public double MeasuredValue()
    {
        return Minimum;
    }

    public AccrualCheckpoint Copy()
    {
        return new AccrualCheckpoint(Code, Title, Minimum, Tags, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
