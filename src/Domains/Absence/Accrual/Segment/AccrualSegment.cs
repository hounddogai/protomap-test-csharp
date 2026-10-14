namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual segment is in its life cycle.</summary>
public enum AccrualSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A accrual segment. Its code is unique within a tenant.</summary>
public sealed class AccrualSegment(string code, string name, string source, double minimum, Dictionary<string, string> attributes, string currency, double margin, string grade, string notes, double ratio, string contactEmail, AccrualSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public double Minimum { get; set; } = minimum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Currency { get; set; } = currency;
    public double Margin { get; set; } = margin;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public string ContactEmail { get; set; } = contactEmail;
    public AccrualSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a accrual segment in status requested.</summary>
    public static AccrualSegment NewAccrualSegment(string code, string name)
    {
        return new AccrualSegment(
            code,
            name,
            "",
            0.0,
            new Dictionary<string, string>(),
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            AccrualSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualSegmentStatus next)
    {
        switch (Status)
        {
            case AccrualSegmentStatus.Requested:
                return next == AccrualSegmentStatus.Expired;
            case AccrualSegmentStatus.Granted:
                return next == AccrualSegmentStatus.Revoked || next == AccrualSegmentStatus.Expired;
            case AccrualSegmentStatus.Revoked:
                switch (next)
                {
                    case AccrualSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double EffectiveLoad()
    {
        return Math.Round(Ratio * Margin, 2);
    }

    public AccrualSegment Copy()
    {
        return new AccrualSegment(
            Code,
            Name,
            Source,
            Minimum,
            Attributes,
            Currency,
            Margin,
            Grade,
            Notes,
            Ratio,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
