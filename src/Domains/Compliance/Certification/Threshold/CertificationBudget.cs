namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification budget is in its life cycle.</summary>
public enum CertificationBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A certification budget. Its code is unique within a tenant.</summary>
public sealed class CertificationBudget(string code, string label, int windowDays, string title, int batchSize, int expiresAt, string channel, int limit, CertificationBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public int Limit { get; set; } = limit;
    public CertificationBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a certification budget in status draft.</summary>
    public static CertificationBudget NewCertificationBudget(string code, string label)
    {
        return new CertificationBudget(code, label, 0, "", 0, 0, "", 0, CertificationBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationBudgetStatus next)
    {
        switch (Status)
        {
            case CertificationBudgetStatus.Draft:
                switch (next)
                {
                    case CertificationBudgetStatus.Pending:
                    case CertificationBudgetStatus.Active:
                        return true;
                }
                return false;
            case CertificationBudgetStatus.Pending:
                return next == CertificationBudgetStatus.Closed;
            case CertificationBudgetStatus.Active:
                return next == CertificationBudgetStatus.Suspended || next == CertificationBudgetStatus.Closed;
            case CertificationBudgetStatus.Suspended:
                switch (next)
                {
                    case CertificationBudgetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)BatchSize * (double)ExpiresAt, 2);
    }

    public CertificationBudget Copy()
    {
        return new CertificationBudget(Code, Label, WindowDays, Title, BatchSize, ExpiresAt, Channel, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
