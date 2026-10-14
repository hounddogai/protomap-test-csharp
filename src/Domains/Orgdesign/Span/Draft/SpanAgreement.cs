namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span agreement is in its life cycle.</summary>
public enum SpanAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a span agreement and its current status.</summary>
public sealed class SpanAgreement(string code, string name, string department, string costCenter, int endsAt, SpanAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string CostCenter { get; set; } = costCenter;
    public int EndsAt { get; set; } = endsAt;
    public SpanAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a span agreement in status requested.</summary>
    public static SpanAgreement NewSpanAgreement(string code, string name)
    {
        return new SpanAgreement(code, name, "", "", 0, SpanAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanAgreementStatus next)
    {
        switch (Status)
        {
            case SpanAgreementStatus.Requested:
                switch (next)
                {
                    case SpanAgreementStatus.Granted:
                    case SpanAgreementStatus.Expired:
                        return true;
                }
                return false;
            case SpanAgreementStatus.Granted:
                return next == SpanAgreementStatus.Revoked || next == SpanAgreementStatus.Expired;
            case SpanAgreementStatus.Revoked:
                return next == SpanAgreementStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SpanAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public SpanAgreement Copy()
    {
        return new SpanAgreement(Code, Name, Department, CostCenter, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
