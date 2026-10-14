namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination exception is in its life cycle.</summary>
public enum NominationExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a nomination exception and its current status.</summary>
public sealed class NominationException(string code, string name, int revision, string contactEmail, string source, double maximum, double minimum, int limit, NominationExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string ContactEmail { get; set; } = contactEmail;
    public string Source { get; set; } = source;
    public double Maximum { get; set; } = maximum;
    public double Minimum { get; set; } = minimum;
    public int Limit { get; set; } = limit;
    public NominationExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a nomination exception in status new.</summary>
    public static NominationException NewNominationException(string code, string name)
    {
        return new NominationException(code, name, 0, "", "", 0.0, 0.0, 0, NominationExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationExceptionStatus next)
    {
        switch (Status)
        {
            case NominationExceptionStatus.New:
                switch (next)
                {
                    case NominationExceptionStatus.Queued:
                    case NominationExceptionStatus.Running:
                        return true;
                }
                return false;
            case NominationExceptionStatus.Queued:
                switch (next)
                {
                    case NominationExceptionStatus.Done:
                    case NominationExceptionStatus.Failed:
                        return true;
                }
                return false;
            case NominationExceptionStatus.Running:
                switch (next)
                {
                    case NominationExceptionStatus.Failed:
                        return true;
                }
                return false;
            case NominationExceptionStatus.Done:
                return next == NominationExceptionStatus.Failed;
        }
        return false;
    }

    public void MoveTo(NominationExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Revision - Minimum, 2);
    }

    public NominationException Copy()
    {
        return new NominationException(Code, Name, Revision, ContactEmail, Source, Maximum, Minimum, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
