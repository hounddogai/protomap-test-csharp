namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction exception is in its life cycle.</summary>
public enum DeductionExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One deduction exception, with what the planners need to act on it.</summary>
public sealed class DeductionException(string code, string name, bool archived, string segment, string notes, double ceiling, Dictionary<string, string> attributes, double rate, string ownerId, string location, string contactEmail, DeductionExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Segment { get; set; } = segment;
    public string Notes { get; set; } = notes;
    public double Ceiling { get; set; } = ceiling;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public string OwnerId { get; set; } = ownerId;
    public string Location { get; set; } = location;
    public string ContactEmail { get; set; } = contactEmail;
    public DeductionExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a deduction exception in status requested.</summary>
    public static DeductionException NewDeductionException(string code, string name)
    {
        return new DeductionException(
            code,
            name,
            false,
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            DeductionExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionExceptionStatus next)
    {
        switch (Status)
        {
            case DeductionExceptionStatus.Requested:
                switch (next)
                {
                    case DeductionExceptionStatus.Granted:
                        return true;
                }
                return false;
            case DeductionExceptionStatus.Granted:
                switch (next)
                {
                    case DeductionExceptionStatus.Revoked:
                    case DeductionExceptionStatus.Expired:
                        return true;
                }
                return false;
            case DeductionExceptionStatus.Revoked:
                switch (next)
                {
                    case DeductionExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double EffectiveVolume()
    {
        return Math.Round(Ceiling + Rate, 2);
    }

    public DeductionException Copy()
    {
        return new DeductionException(
            Code,
            Name,
            Archived,
            Segment,
            Notes,
            Ceiling,
            Attributes,
            Rate,
            OwnerId,
            Location,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
