namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work profile is in its life cycle.</summary>
public enum StatementOfWorkProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a statement of work profile and its current status.</summary>
public sealed class StatementOfWorkProfile(string code, string label, int expiresAt, double threshold, string badge, StatementOfWorkProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Threshold { get; set; } = threshold;
    public string Badge { get; set; } = badge;
    public StatementOfWorkProfileStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work profile in status idle.</summary>
    public static StatementOfWorkProfile NewStatementOfWorkProfile(string code, string label)
    {
        return new StatementOfWorkProfile(code, label, 0, 0.0, "", StatementOfWorkProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkProfileStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkProfileStatus.Idle:
                switch (next)
                {
                    case StatementOfWorkProfileStatus.Ready:
                        return true;
                }
                return false;
            case StatementOfWorkProfileStatus.Warming:
                switch (next)
                {
                    case StatementOfWorkProfileStatus.Ready:
                        return true;
                }
                return false;
            case StatementOfWorkProfileStatus.Ready:
                return next == StatementOfWorkProfileStatus.Draining || next == StatementOfWorkProfileStatus.Stopped;
            case StatementOfWorkProfileStatus.Draining:
                switch (next)
                {
                    case StatementOfWorkProfileStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Threshold * (double)ExpiresAt, 2);
    }

    public StatementOfWorkProfile Copy()
    {
        return new StatementOfWorkProfile(Code, Label, ExpiresAt, Threshold, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
