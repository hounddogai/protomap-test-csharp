namespace Acme.Workforce.Absence.Balance.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance tier is in its life cycle.</summary>
public enum BalanceTierStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A balance tier. Its code is unique within a tenant.</summary>
public sealed class BalanceTier(string code, string label, int endsAt, int durationMinutes, string badge, string salaryBand, int revision, BalanceTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Badge { get; set; } = badge;
    public string SalaryBand { get; set; } = salaryBand;
    public int Revision { get; set; } = revision;
    public BalanceTierStatus Status { get; set; } = status;

    /// <summary>Starts a balance tier in status requested.</summary>
    public static BalanceTier NewBalanceTier(string code, string label)
    {
        return new BalanceTier(code, label, 0, 0, "", "", 0, BalanceTierStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceTierStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceTierStatus next)
    {
        switch (Status)
        {
            case BalanceTierStatus.Requested:
                switch (next)
                {
                    case BalanceTierStatus.Granted:
                    case BalanceTierStatus.Expired:
                        return true;
                }
                return false;
            case BalanceTierStatus.Granted:
                return next == BalanceTierStatus.Revoked;
            case BalanceTierStatus.Revoked:
                switch (next)
                {
                    case BalanceTierStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Revision - (double)DurationMinutes, 2);
    }

    public BalanceTier Copy()
    {
        return new BalanceTier(Code, Label, EndsAt, DurationMinutes, Badge, SalaryBand, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
