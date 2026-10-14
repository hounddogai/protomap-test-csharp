namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default certification budget is in its life cycle.</summary>
public enum DefaultCertificationBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One default certification budget, with what the planners need to act on it.</summary>
public sealed class DefaultCertificationBudget(string code, string label, string channel, int expiresAt, string team, double ratio, DefaultCertificationBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Team { get; set; } = team;
    public double Ratio { get; set; } = ratio;
    public DefaultCertificationBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a default certification budget in status open.</summary>
    public static DefaultCertificationBudget NewDefaultCertificationBudget(string code, string label)
    {
        return new DefaultCertificationBudget(code, label, "", 0, "", 0.0, DefaultCertificationBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultCertificationBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultCertificationBudgetStatus next)
    {
        switch (Status)
        {
            case DefaultCertificationBudgetStatus.Open:
                return next == DefaultCertificationBudgetStatus.Resolved || next == DefaultCertificationBudgetStatus.Archived;
            case DefaultCertificationBudgetStatus.InReview:
                switch (next)
                {
                    case DefaultCertificationBudgetStatus.Resolved:
                    case DefaultCertificationBudgetStatus.Reopened:
                        return true;
                }
                return false;
            case DefaultCertificationBudgetStatus.Resolved:
                return next == DefaultCertificationBudgetStatus.Reopened || next == DefaultCertificationBudgetStatus.Archived;
            case DefaultCertificationBudgetStatus.Reopened:
                switch (next)
                {
                    case DefaultCertificationBudgetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultCertificationBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default certification budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)ExpiresAt * Ratio, 2);
    }

    public DefaultCertificationBudget Copy()
    {
        return new DefaultCertificationBudget(Code, Label, Channel, ExpiresAt, Team, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
