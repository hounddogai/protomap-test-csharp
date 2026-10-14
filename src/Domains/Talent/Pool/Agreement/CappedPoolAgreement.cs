namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pool agreement is in its life cycle.</summary>
public enum CappedPoolAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped pool agreement as the pool team keeps it.</summary>
public sealed class CappedPoolAgreement(string code, string title, int expiresAt, int quantity, double discount, string skill, CappedPoolAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Quantity { get; set; } = quantity;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public CappedPoolAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a capped pool agreement in status new.</summary>
    public static CappedPoolAgreement NewCappedPoolAgreement(string code, string title)
    {
        return new CappedPoolAgreement(code, title, 0, 0, 0.0, "", CappedPoolAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPoolAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPoolAgreementStatus next)
    {
        switch (Status)
        {
            case CappedPoolAgreementStatus.New:
                return next == CappedPoolAgreementStatus.Queued || next == CappedPoolAgreementStatus.Failed;
            case CappedPoolAgreementStatus.Queued:
                switch (next)
                {
                    case CappedPoolAgreementStatus.Done:
                    case CappedPoolAgreementStatus.Failed:
                        return true;
                }
                return false;
            case CappedPoolAgreementStatus.Running:
                return next == CappedPoolAgreementStatus.Done;
            case CappedPoolAgreementStatus.Done:
                return next == CappedPoolAgreementStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedPoolAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pool agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Quantity - (double)ExpiresAt, 2);
    }

    public CappedPoolAgreement Copy()
    {
        return new CappedPoolAgreement(Code, Title, ExpiresAt, Quantity, Discount, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
