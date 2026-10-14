namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default training due window is in its life cycle.</summary>
public enum DefaultTrainingDueWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A default training due window as the training due team keeps it.</summary>
public sealed class DefaultTrainingDueWindow(string code, string name, string phone, int revision, string contactEmail, string jobCode, DefaultTrainingDueWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public int Revision { get; set; } = revision;
    public string ContactEmail { get; set; } = contactEmail;
    public string JobCode { get; set; } = jobCode;
    public DefaultTrainingDueWindowStatus Status { get; set; } = status;

    /// <summary>Starts a default training due window in status requested.</summary>
    public static DefaultTrainingDueWindow NewDefaultTrainingDueWindow(string code, string name)
    {
        return new DefaultTrainingDueWindow(code, name, "", 0, "", "", DefaultTrainingDueWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultTrainingDueWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultTrainingDueWindowStatus next)
    {
        switch (Status)
        {
            case DefaultTrainingDueWindowStatus.Requested:
                switch (next)
                {
                    case DefaultTrainingDueWindowStatus.Revoked:
                        return true;
                }
                return false;
            case DefaultTrainingDueWindowStatus.Granted:
                switch (next)
                {
                    case DefaultTrainingDueWindowStatus.Revoked:
                    case DefaultTrainingDueWindowStatus.Expired:
                        return true;
                }
                return false;
            case DefaultTrainingDueWindowStatus.Revoked:
                return next == DefaultTrainingDueWindowStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DefaultTrainingDueWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default training due window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"default training due window"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public DefaultTrainingDueWindow Copy()
    {
        return new DefaultTrainingDueWindow(Code, Name, Phone, Revision, ContactEmail, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
