namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool agreement is in its life cycle.</summary>
public enum PoolAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A pool agreement. Its code is unique within a tenant.</summary>
public sealed class PoolAgreement(string code, string title, string label, string segment, string region, string reviewer, double score, PoolAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public string Region { get; set; } = region;
    public string Reviewer { get; set; } = reviewer;
    public double Score { get; set; } = score;
    public PoolAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pool agreement in status created.</summary>
    public static PoolAgreement NewPoolAgreement(string code, string title)
    {
        return new PoolAgreement(code, title, "", "", "", "", 0.0, PoolAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolAgreementStatus next)
    {
        switch (Status)
        {
            case PoolAgreementStatus.Created:
                switch (next)
                {
                    case PoolAgreementStatus.Validated:
                        return true;
                }
                return false;
            case PoolAgreementStatus.Validated:
                return next == PoolAgreementStatus.Retired;
            case PoolAgreementStatus.Published:
                return next == PoolAgreementStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PoolAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pool agreement"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PoolAgreement Copy()
    {
        return new PoolAgreement(Code, Title, Label, Segment, Region, Reviewer, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
