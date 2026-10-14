namespace Acme.Workforce.Compliance.Exception.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception profile is in its life cycle.</summary>
public enum ExceptionProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A exception profile. Its code is unique within a tenant.</summary>
public sealed class ExceptionProfile(string code, string name, double margin, string ownerId, string channel, string description, string grade, string notes, bool approved, int retries, ExceptionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public string OwnerId { get; set; } = ownerId;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public bool Approved { get; set; } = approved;
    public int Retries { get; set; } = retries;
    public ExceptionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a exception profile in status new.</summary>
    public static ExceptionProfile NewExceptionProfile(string code, string name)
    {
        return new ExceptionProfile(code, name, 0.0, "", "", "", "", "", false, 0, ExceptionProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionProfileStatus next)
    {
        switch (Status)
        {
            case ExceptionProfileStatus.New:
                switch (next)
                {
                    case ExceptionProfileStatus.Queued:
                    case ExceptionProfileStatus.Running:
                        return true;
                }
                return false;
            case ExceptionProfileStatus.Queued:
                switch (next)
                {
                    case ExceptionProfileStatus.Running:
                    case ExceptionProfileStatus.Failed:
                        return true;
                }
                return false;
            case ExceptionProfileStatus.Running:
                switch (next)
                {
                    case ExceptionProfileStatus.Done:
                    case ExceptionProfileStatus.Failed:
                        return true;
                }
                return false;
            case ExceptionProfileStatus.Done:
                switch (next)
                {
                    case ExceptionProfileStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Margin * (double)Retries, 2);
    }

    public ExceptionProfile Copy()
    {
        return new ExceptionProfile(
            Code,
            Name,
            Margin,
            OwnerId,
            Channel,
            Description,
            Grade,
            Notes,
            Approved,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
