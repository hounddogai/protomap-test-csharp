namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation notice is in its life cycle.</summary>
public enum AttestationNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a attestation notice and its current status.</summary>
public sealed class AttestationNotice(string code, string title, string grade, int revision, double fte, string badge, string salaryBand, Dictionary<string, string> attributes, double rate, double cost, AttestationNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public int Revision { get; set; } = revision;
    public double Fte { get; set; } = fte;
    public string Badge { get; set; } = badge;
    public string SalaryBand { get; set; } = salaryBand;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Rate { get; set; } = rate;
    public double Cost { get; set; } = cost;
    public AttestationNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a attestation notice in status draft.</summary>
    public static AttestationNotice NewAttestationNotice(string code, string title)
    {
        return new AttestationNotice(
            code,
            title,
            "",
            0,
            0.0,
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            0.0,
            AttestationNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationNoticeStatus next)
    {
        switch (Status)
        {
            case AttestationNoticeStatus.Draft:
                return next == AttestationNoticeStatus.Suspended;
            case AttestationNoticeStatus.Pending:
                return next == AttestationNoticeStatus.Suspended;
            case AttestationNoticeStatus.Active:
                return next == AttestationNoticeStatus.Closed;
            case AttestationNoticeStatus.Suspended:
                switch (next)
                {
                    case AttestationNoticeStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"attestation notice"} {Code}";
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

    public double ProjectedLoad()
    {
        return Math.Round(Fte * Rate, 2);
    }

    public AttestationNotice Copy()
    {
        return new AttestationNotice(
            Code,
            Title,
            Grade,
            Revision,
            Fte,
            Badge,
            SalaryBand,
            Attributes,
            Rate,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
