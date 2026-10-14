namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form notice is in its life cycle.</summary>
public enum TaxFormNoticeStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a tax form notice and its current status.</summary>
public sealed class TaxFormNotice(string code, string label, int endsAt, int expiresAt, bool visible, double score, TaxFormNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Visible { get; set; } = visible;
    public double Score { get; set; } = score;
    public TaxFormNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a tax form notice in status idle.</summary>
    public static TaxFormNotice NewTaxFormNotice(string code, string label)
    {
        return new TaxFormNotice(code, label, 0, 0, false, 0.0, TaxFormNoticeStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormNoticeStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormNoticeStatus next)
    {
        switch (Status)
        {
            case TaxFormNoticeStatus.Idle:
                switch (next)
                {
                    case TaxFormNoticeStatus.Warming:
                        return true;
                }
                return false;
            case TaxFormNoticeStatus.Warming:
                switch (next)
                {
                    case TaxFormNoticeStatus.Ready:
                        return true;
                }
                return false;
            case TaxFormNoticeStatus.Ready:
                return next == TaxFormNoticeStatus.Draining;
            case TaxFormNoticeStatus.Draining:
                return next == TaxFormNoticeStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TaxFormNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)ExpiresAt - Score, 2);
    }

    public TaxFormNotice Copy()
    {
        return new TaxFormNotice(Code, Label, EndsAt, ExpiresAt, Visible, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
